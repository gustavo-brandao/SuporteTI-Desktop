using System.Net;

namespace SuporteTI.Grafico;

internal static class TesteVelocidadeCenarios
{
    internal static async Task Executar(Action<bool, string> verificar)
    {
        verificar(TesteVelocidade.Mbps(12500000, 1) == 100, "Velocidade converte bytes e tempo para Mbps decimais");
        verificar(double.IsFinite(TesteVelocidade.Mbps(1, 0)), "Velocidade evita divisão por zero");
        var limite = new TesteVelocidade.Opcoes(TimeSpan.FromMilliseconds(120), 262144, 4, 65536);
        using var handler = new Servidor();
        using var cliente = new HttpClient(handler);
        var progresso = new Captura();
        var r = await new TesteVelocidade(cliente, limite).MedirAsync(default, progresso);
        verificar(r.Operacao == "velocidade" && r.Secoes.Length == 4, "Novo teste retorna latência, download, upload e método");
        verificar(handler.Maximo == 4, "Teste executa quatro conexões simultâneas");
        verificar(handler.Download == limite.LimiteBytes && handler.Upload == limite.LimiteBytes, "Orçamento compartilhado limita os bytes das quatro conexões");
        verificar(r.Secoes[1].Campos.Any(c => c.Nome == "Download" && c.Valor.Contains("Mbps")) &&
            r.Secoes[2].Campos.Any(c => c.Nome == "Upload"), "Resultados apresentam download e upload em Mbps");
        verificar(r.Secoes[1].Aviso?.Contains("Amostra curta") == true, "Limite atingido cedo avisa sobre amostra curta");
        verificar(progresso.Itens.Select(p => p.Etapa).Distinct().SequenceEqual(["Latência", "Download", "Upload"]) &&
            progresso.Itens.All(p => p.Fracao is >= 0 and <= 1 && p.Bytes >= 0), "Progresso usa fases e dados reais dentro dos limites");
        verificar(handler.DestinosValidos, "Medição usa somente endpoints HTTPS fixos e não segue redirecionamento");
        using var restritoHandler = new Servidor { LimiteDownload = 8 * 1024 * 1024 };
        using var restritoCliente = new HttpClient(restritoHandler);
        await new TesteVelocidade(restritoCliente, new(TimeSpan.FromSeconds(2), 17 * 1024 * 1024))
            .MedirAsync(default);
        verificar(restritoHandler.Download == 17 * 1024 * 1024,
            "Coleta repete blocos compatíveis quando o servidor recusa requisições maiores com HTTP 403");

        using var falhaHandler = new Servidor { FalharUpload = true };
        using var falhaCliente = new HttpClient(falhaHandler);
        var parcial = await new TesteVelocidade(falhaCliente, limite).MedirAsync(default);
        verificar(parcial.Estado == "parcial" && parcial.Secoes[1].Campos.Length > 0 && parcial.Secoes[2].Aviso is not null,
            "Motor nativo preserva download quando upload falha");
        verificar(parcial.Secoes[2].Aviso!.Contains("HTTP 503"), "Aviso conserva o código HTTP da falha de transferência");
        verificar(TesteVelocidade.ObterTaxa(parcial, "Upload") is null && TesteVelocidade.ObterTaxa(parcial, "Download")!.EndsWith("Mbps"),
            "Resumo distingue taxa medida e texto do método quando uma direção está indisponível");
        var log = LogConsulta.Formatar("Velocidade", parcial, DateTimeOffset.Now, DateTimeOffset.Now);
        verificar(log.Contains("Mbps") && log.Contains("Nenhuma transferência válida") && log.Contains("Limite de dados"),
            "Log conserva taxa, falha parcial e método da medição nativa");

        using var incompletoHandler = new Servidor { Incompleto = true };
        using var incompletoCliente = new HttpClient(incompletoHandler);
        var incompleto = await new TesteVelocidade(incompletoCliente, limite).MedirAsync(default);
        verificar(incompleto.Secoes[1].Aviso?.Contains("conexões falharam") == true, "Download incompleto é sinalizado sem inventar bytes");

        using var lentoHandler = new Servidor { AtrasoUpload = 500 };
        using var lentoCliente = new HttpClient(lentoHandler);
        var lento = await new TesteVelocidade(lentoCliente, limite).MedirAsync(default);
        verificar(lento.Secoes[2].Campos.Length == 0 && lento.Secoes[2].Aviso is not null,
            "Upload sem confirmação do servidor não apresenta taxa de escrita em buffer como velocidade");
        using var baixaTaxaHandler = new Servidor { BytesPorSegundoUpload = 1024 * 1024 };
        using var baixaTaxaCliente = new HttpClient(baixaTaxaHandler);
        var baixaTaxa = await new TesteVelocidade(baixaTaxaCliente, new(TimeSpan.FromMilliseconds(400), 1024 * 1024))
            .MedirAsync(default);
        verificar(baixaTaxa.Secoes[2].Campos.Length > 0,
            "Blocos iniciais menores permitem obter confirmação também em upload lento");

        using var prazoHandler = new Servidor { AtrasoUpload = 20 };
        using var prazoCliente = new HttpClient(prazoHandler);
        var relogio = System.Diagnostics.Stopwatch.StartNew();
        var porTempo = await new TesteVelocidade(prazoCliente,
            new(TimeSpan.FromMilliseconds(120), 64 * 1024 * 1024, 4, 65536)).MedirAsync(default);
        verificar(relogio.ElapsedMilliseconds >= 200 && porTempo.Secoes[2].Campos.Length > 0,
            "Coleta continua pela janela de tempo em vez de encerrar na primeira amostra");

        using var cancelamento = new CancellationTokenSource();
        cancelamento.CancelAfter(30);
        try
        {
            await new TesteVelocidade(lentoCliente, limite).MedirAsync(cancelamento.Token);
            verificar(false, "Medição deve aceitar cancelamento");
        }
        catch (OperationCanceledException) { verificar(true, "Cancelamento interrompe as transferências ativas"); }

        using var offlineHandler = new Servidor { Offline = true };
        using var offlineCliente = new HttpClient(offlineHandler);
        var offline = await new TesteVelocidade(offlineCliente, limite).MedirAsync(default);
        verificar(offline.Estado == "indisponivel" && offline.Secoes.Take(3).All(s => s.Campos.Length == 0 && s.Aviso is not null),
            "Servidor indisponível não produz velocidades falsas");
        foreach (var resposta in new[] { r, parcial, offline })
        {
            var json = System.Text.Json.JsonSerializer.Serialize(resposta, new System.Text.Json.JsonSerializerOptions
                { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            verificar(ConsultaPowerShell.Interpretar(json, "velocidade").Estado == resposta.Estado,
                "Medição nativa respeita o contrato de resultados: " + resposta.Estado);
        }
    }

    private sealed class Captura : IProgress<ProgressoVelocidade>
    {
        internal readonly List<ProgressoVelocidade> Itens = [];
        public void Report(ProgressoVelocidade value) => Itens.Add(value);
    }
    private sealed class Servidor : HttpMessageHandler
    {
        internal bool FalharUpload, Incompleto, Offline;
        internal int AtrasoUpload = 10;
        internal int LimiteDownload = int.MaxValue;
        internal int BytesPorSegundoUpload;
        internal long Download, Upload;
        internal bool DestinosValidos = true;
        private int ativos;
        internal int Maximo;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            DestinosValidos &= request.RequestUri is { Scheme: "https", Host: "speed.cloudflare.com" };
            if (Offline) throw new HttpRequestException("Offline simulado.");
            if (request.Method == HttpMethod.Post)
            {
                if (FalharUpload) return new(HttpStatusCode.ServiceUnavailable);
                var atual = Interlocked.Increment(ref ativos);
                Maximo = Math.Max(Maximo, atual);
                try
                {
                    await request.Content!.CopyToAsync(Stream.Null, cancellationToken);
                    var atraso = AtrasoUpload + (BytesPorSegundoUpload > 0 ?
                        (int)(1000 * (request.Content.Headers.ContentLength ?? 0) / BytesPorSegundoUpload) : 0);
                    await Task.Delay(atraso, cancellationToken);
                    Interlocked.Add(ref Upload, request.Content.Headers.ContentLength ?? 0);
                    return new(HttpStatusCode.OK) { Content = new ByteArrayContent([]) };
                }
                finally { Interlocked.Decrement(ref ativos); }
            }
            var n = int.Parse(request.RequestUri!.Query.Split('=').Last());
            if (n > LimiteDownload) return new(HttpStatusCode.Forbidden);
            if (n > 0) { await Task.Delay(10, cancellationToken); Interlocked.Add(ref Download, n); }
            return new(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[Incompleto ? n / 2 : n]) };
        }
    }
}
