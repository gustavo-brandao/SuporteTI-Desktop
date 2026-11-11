using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace SuporteTI.Grafico;

internal sealed record ProgressoVelocidade(string Etapa, double Segundos, long Bytes, double? Mbps, double Fracao);

// A edição gráfica mede por tempo, em quatro conexões. Os coletores do terminal continuam independentes.
internal sealed class TesteVelocidade
{
    internal sealed record Opcoes(TimeSpan Duracao, long LimiteBytes, int Conexoes = 4,
        int Bloco = 8 * 1024 * 1024)
    {
        public static Opcoes Padrao => new(TimeSpan.FromSeconds(10), 512L * 1024 * 1024);
    }
    private readonly HttpClient cliente;
    private readonly Opcoes opcoes;

    internal TesteVelocidade(HttpClient cliente, Opcoes? opcoes = null)
    {
        this.cliente = cliente;
        this.opcoes = opcoes ?? Opcoes.Padrao;
        if (this.opcoes.Duracao <= TimeSpan.Zero || this.opcoes.LimiteBytes < 1 ||
            this.opcoes.Conexoes is < 1 or > 4 || this.opcoes.Bloco < 1)
            throw new ArgumentOutOfRangeException(nameof(opcoes));
    }

    public static async Task<Resposta> ExecutarAsync(CancellationToken cancelar, IProgress<ProgressoVelocidade>? progresso)
    {
        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false, MaxConnectionsPerServer = 4, ConnectTimeout = TimeSpan.FromSeconds(5)
        };
        using var cliente = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        return await new TesteVelocidade(cliente).MedirAsync(cancelar, progresso).ConfigureAwait(false);
    }

    internal async Task<Resposta> MedirAsync(CancellationToken cancelar, IProgress<ProgressoVelocidade>? progresso = null)
    {
        cancelar.ThrowIfCancellationRequested();
        var secoes = new List<Secao>();
        var inicio = Stopwatch.StartNew();
        progresso?.Report(new("Latência", 0, 0, null, 0));
        var tempos = new List<double>();
        string? avisoLatencia = null;
        for (var i = 0; i < 3; i++)
        {
            try
            {
                using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancelar);
                prazo.CancelAfter(TimeSpan.FromSeconds(5));
                var relogio = Stopwatch.StartNew();
                using var pedido = Pedido(false, 0);
                using var resposta = await cliente.SendAsync(pedido, HttpCompletionOption.ResponseHeadersRead, prazo.Token).ConfigureAwait(false);
                resposta.EnsureSuccessStatusCode();
                tempos.Add(relogio.Elapsed.TotalMilliseconds);
            }
            catch (Exception erro) when (erro is HttpRequestException or IOException or OperationCanceledException)
            {
                cancelar.ThrowIfCancellationRequested();
                avisoLatencia = "Não foi possível completar as três amostras HTTPS.";
                break;
            }
        }
        secoes.Add(new("Latência HTTPS · Cloudflare", tempos.Count == 0 ? [] :
            [new("Mediana", $"{tempos.Order().ElementAt(tempos.Count / 2):N1} ms"),
             new("Amostras", tempos.Count.ToString())], avisoLatencia));
        foreach (var upload in new[] { false, true })
            secoes.Add(await MedirDirecaoAsync(upload, inicio, cancelar, progresso).ConfigureAwait(false));
        secoes.Add(new("Método", [
            new("Servidor", "Cloudflare · HTTPS"),
            new("Coleta", $"Até {opcoes.Duracao.TotalSeconds:N0} s por direção · {opcoes.Conexoes} conexões paralelas"),
            new("Limite de dados", $"{opcoes.LimiteBytes / 1048576.0:N0} MiB por direção; pode encerrar antes ao atingir o limite"),
            new("Interpretação", "Vazão útil estimada até este servidor. Wi-Fi, VPN, proxy, tráfego concorrente e servidor afetam o resultado; não garante a velocidade contratada."),
            new("Critério do upload", "A velocidade considera somente blocos confirmados pelo servidor; blocos interrompidos não entram no resultado.")], null));
        var possuiCampos = secoes.Take(3).Any(s => s.Campos.Length > 0);
        var aviso = secoes.Any(s => s.Aviso is not null);
        if (!possuiCampos)
        {
            var metodo = secoes[^1];
            secoes[^1] = metodo with { Campos = [], Aviso = string.Join(" · ", metodo.Campos.Select(c => c.Nome + ": " + c.Valor)) };
        }
        return new(1, "velocidade", !possuiCampos ? "indisponivel" : aviso ? "parcial" : "concluido", secoes.ToArray());
    }

    private async Task<Secao> MedirDirecaoAsync(bool upload, Stopwatch total, CancellationToken cancelar,
        IProgress<ProgressoVelocidade>? progresso)
    {
        var etapa = upload ? "Upload" : "Download";
        var relogio = Stopwatch.StartNew();
        long reservados = 0, transferidos = 0, confirmados = 0;
        var falhas = 0;
        string? detalheFalha = null;
        var limiteAtingido = false;
        using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancelar);
        prazo.CancelAfter(opcoes.Duracao);
        var trava = new object();
        progresso?.Report(new(etapa, total.Elapsed.TotalSeconds, 0, null, upload ? .5 : 0));
        async Task Transferir()
        {
            var buffer = new byte[65536];
            if (upload) RandomNumberGenerator.Fill(buffer);
            var blocoAtual = upload ? Math.Min(opcoes.Bloco, 256 * 1024) : opcoes.Bloco;
            while (!prazo.IsCancellationRequested)
            {
                int tamanho;
                lock (trava)
                {
                    tamanho = (int)Math.Min(blocoAtual, opcoes.LimiteBytes - reservados);
                    if (tamanho == 0) break;
                    reservados += tamanho;
                }
                try
                {
                    var tempoPedido = Stopwatch.StartNew();
                    using var pedido = Pedido(upload, tamanho);
                    if (upload)
                    {
                        pedido.Content = new ConteudoMedido(tamanho, buffer, n => Interlocked.Add(ref transferidos, n));
                        pedido.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    }
                    using var resposta = await cliente.SendAsync(pedido, HttpCompletionOption.ResponseHeadersRead, prazo.Token).ConfigureAwait(false);
                    resposta.EnsureSuccessStatusCode();
                    if (upload)
                    {
                        Interlocked.Add(ref confirmados, tamanho);
                        // Começar pequeno permite medir uploads lentos; ampliar só após confirmação rápida.
                        if (tempoPedido.Elapsed.TotalSeconds < .4) blocoAtual = Math.Min(opcoes.Bloco, blocoAtual * 2);
                        else if (tempoPedido.Elapsed.TotalSeconds > 1.5) blocoAtual = Math.Max(Math.Min(opcoes.Bloco, 65536), blocoAtual / 2);
                    }
                    else
                    {
                        await using var fluxo = await resposta.Content.ReadAsStreamAsync(prazo.Token).ConfigureAwait(false);
                        var recebidos = 0;
                        while (recebidos < tamanho)
                        {
                            var lidos = await fluxo.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, tamanho - recebidos)), prazo.Token).ConfigureAwait(false);
                            if (lidos == 0) throw new IOException("Resposta incompleta.");
                            recebidos += lidos;
                            Interlocked.Add(ref transferidos, lidos);
                            Interlocked.Add(ref confirmados, lidos);
                        }
                    }
                }
                catch (OperationCanceledException) when (prazo.IsCancellationRequested) { break; }
                catch (Exception erro) when (erro is HttpRequestException or IOException or OperationCanceledException)
                {
                    var detalhe = erro is HttpRequestException { StatusCode: { } codigo } ? $"HTTP {(int)codigo}" :
                        erro is HttpRequestException http ? $"{http.HttpRequestError} (0x{erro.HResult:X8})" : $"{erro.GetType().Name} (0x{erro.HResult:X8})";
                    Interlocked.CompareExchange(ref detalheFalha, detalhe, null);
                    Interlocked.Increment(ref falhas); break;
                }
            }
        }
        var tarefas = Enumerable.Range(0, opcoes.Conexoes).Select(_ => Transferir()).ToArray();
        var fim = Task.WhenAll(tarefas);
        void Informar()
        {
            var bytes = Interlocked.Read(ref confirmados);
            var fracao = Math.Max(relogio.Elapsed.TotalSeconds / opcoes.Duracao.TotalSeconds,
                (double)Interlocked.Read(ref transferidos) / opcoes.LimiteBytes);
            progresso?.Report(new(etapa, total.Elapsed.TotalSeconds, Interlocked.Read(ref transferidos),
                bytes > 0 ? Mbps(bytes, relogio.Elapsed.TotalSeconds) : null, (upload ? .5 : 0) + Math.Clamp(fracao, 0, 1) / 2));
        }
        while (!fim.IsCompleted)
        {
            Informar();
            await Task.WhenAny(fim, Task.Delay(150, cancelar)).ConfigureAwait(false);
            if (cancelar.IsCancellationRequested) { prazo.Cancel(); break; }
        }
        await fim.ConfigureAwait(false);
        cancelar.ThrowIfCancellationRequested();
        relogio.Stop();
        limiteAtingido = Interlocked.Read(ref confirmados) >= opcoes.LimiteBytes;
        Informar();
        var bytesValidos = Interlocked.Read(ref confirmados);
        string? aviso = bytesValidos == 0 ? "Nenhuma transferência válida. Confira a conexão, o proxy ou o acesso ao servidor." :
            falhas > 0 ? "Uma ou mais conexões falharam. A medição contém somente os dados recebidos ou confirmados." : null;
        if (bytesValidos > 0 && relogio.Elapsed.TotalSeconds < 1 && limiteAtingido)
            aviso = "Amostra curta: limite de dados atingido em menos de um segundo. Interprete como estimativa.";
        if (detalheFalha is not null && aviso is not null) aviso += " Detalhe: " + detalheFalha + ".";
        return new(etapa + " · Cloudflare", bytesValidos == 0 ? [] : [
            new(etapa, $"{Mbps(bytesValidos, relogio.Elapsed.TotalSeconds):N1} Mbps"),
            new("Dados válidos", $"{bytesValidos / 1048576.0:N1} MiB"),
            new("Duração", $"{relogio.Elapsed.TotalSeconds:N1} s"),
            new("Encerramento", limiteAtingido ? "Limite de dados atingido" : falhas == opcoes.Conexoes ? "Conexões indisponíveis" : "Janela de medição encerrada")], aviso);
    }

    internal static double Mbps(long bytes, double segundos) => 8 * (double)bytes / Math.Max(segundos, .001) / 1e6;
    internal static string? ObterTaxa(Resposta? resposta, string direcao) => resposta?.Secoes
        .FirstOrDefault(s => s.Nome == direcao + " · Cloudflare")?.Campos.FirstOrDefault(c => c.Nome == direcao)?.Valor;

    private static HttpRequestMessage Pedido(bool upload, int bytes)
    {
        var uri = upload ? "https://speed.cloudflare.com/__up" : $"https://speed.cloudflare.com/__down?bytes={bytes}";
        var pedido = new HttpRequestMessage(upload ? HttpMethod.Post : HttpMethod.Get, uri)
        { Version = HttpVersion.Version11, VersionPolicy = HttpVersionPolicy.RequestVersionExact };
        pedido.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
        return pedido;
    }

    private sealed class ConteudoMedido(int tamanho, byte[] buffer, Action<int> registrar) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = tamanho; return true; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            for (var enviados = 0; enviados < tamanho;)
            {
                var n = Math.Min(buffer.Length, tamanho - enviados);
                await stream.WriteAsync(buffer.AsMemory(0, n), cancellationToken).ConfigureAwait(false);
                registrar(n); enviados += n;
            }
        }
    }
}
