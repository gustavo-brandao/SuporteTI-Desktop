using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuporteTI.Grafico;

internal sealed record Campo(string Nome, string Valor, string? Familia = null, double? Percentual = null);
internal sealed record Secao(string Nome, Campo[] Campos, string? Aviso);
internal sealed record Resposta(int Versao, string Operacao, string Estado, Secao[] Secoes);

internal sealed class ConsultaPowerShell
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    private static readonly string[] SecoesEsperadas = ["Computador", "Windows", "BIOS", "CPU", "Memória em uso"];
    private static readonly HashSet<string> Operacoes = ["sistema", "desempenho", "discos", "bateria", "dispositivos", "servicos", "programas", "autologon",
        "rede", "dominio", "ping", "velocidade", "falhas", "codigofalha", "update", "filaimpressao", "gpusuario", "gphtml", "eventos", "evtx",
        "adaptadores", "filaslocais", "chromecache", "gpupdateuser"];
    private readonly string script;
    private readonly TimeSpan limite;

    public ConsultaPowerShell() : this(Path.Combine(AppContext.BaseDirectory, "Modulos", "Consultar.ps1"), TimeSpan.FromSeconds(25))
    {
    }

    internal ConsultaPowerShell(string script, TimeSpan limite)
    {
        this.script = script;
        this.limite = limite;
    }

    public Task<Resposta> ConsultarSistemaAsync(CancellationToken cancelamento) => ConsultarAsync("sistema", cancelamento);

    public async Task<Resposta> ConsultarAsync(string operacao, CancellationToken cancelamento,
        string entrada = "", string familia = "")
    {
        if (!Operacoes.Contains(operacao))
            throw new ArgumentException("Consulta desconhecida.", nameof(operacao));
        cancelamento.ThrowIfCancellationRequested();
        if (!File.Exists(script))
            throw new InvalidOperationException("O módulo de consulta não foi encontrado. Extraia todo o ZIP.");

        using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancelamento);
        prazo.CancelAfter(operacao switch
        {
            "evtx" => TimeSpan.FromSeconds(90),
            "chromecache" or "gpupdateuser" => TimeSpan.FromMinutes(5),
            "discos" or "velocidade" or "falhas" or "gpusuario" or "gphtml" or "update" => TimeSpan.FromSeconds(45),
            _ => limite
        });
        using var processo = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };
        foreach (var argumento in new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "RemoteSigned", "-File", script, "-Operacao", operacao })
            processo.StartInfo.ArgumentList.Add(argumento);
        if (operacao is "ping" or "codigofalha" or "gphtml" or "evtx")
        {
            processo.StartInfo.ArgumentList.Add("-Entrada64");
            processo.StartInfo.ArgumentList.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(entrada)));
        }
        if (operacao == "codigofalha" && !string.IsNullOrEmpty(familia))
        {
            processo.StartInfo.ArgumentList.Add("-Familia");
            processo.StartInfo.ArgumentList.Add(familia);
        }

        processo.Start();
        try
        {
            var saida = processo.StandardOutput.ReadToEndAsync(prazo.Token);
            var erros = processo.StandardError.ReadToEndAsync(prazo.Token);
            await Task.WhenAll(saida, erros, processo.WaitForExitAsync(prazo.Token)).ConfigureAwait(false);
            if (processo.ExitCode != 0)
                throw new InvalidOperationException($"O PowerShell encerrou com código {processo.ExitCode}.\n{(await erros).Trim()}".TrimEnd());

            return Interpretar(await saida, operacao);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!processo.HasExited)
                    processo.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (processo.HasExited)
            {
                // O processo encerrou durante o cancelamento.
            }
            if (cancelamento.IsCancellationRequested)
                throw;
            throw new TimeoutException("A consulta excedeu o limite de tempo. Tente novamente.");
        }
    }

    internal static Resposta Interpretar(string texto, string operacao = "sistema")
    {
        Resposta? resposta;
        try
        {
            resposta = JsonSerializer.Deserialize<Resposta>(texto, Json);
        }
        catch (JsonException erro)
        {
            throw new InvalidDataException("O PowerShell retornou uma resposta JSON inválida.", erro);
        }

        if (resposta is not { Versao: 1, Secoes: { Length: > 0 } } || resposta.Operacao != operacao ||
            (operacao == "sistema" && !resposta.Secoes.Select(secao => secao?.Nome).SequenceEqual(SecoesEsperadas)) ||
            resposta.Secoes.Any(secao =>
                secao is null || string.IsNullOrWhiteSpace(secao.Nome) || secao.Campos is null ||
                (secao.Campos.Length == 0 && string.IsNullOrWhiteSpace(secao.Aviso)) ||
                secao.Campos.Any(campo => campo is null || string.IsNullOrWhiteSpace(campo.Nome) ||
                    string.IsNullOrWhiteSpace(campo.Valor))))
            throw new InvalidDataException("A resposta do PowerShell não atende ao contrato da consulta.");

        var estadoEsperado = resposta.Secoes.All(secao => secao.Campos.Length == 0) ? "indisponivel"
            : resposta.Secoes.Any(secao => !string.IsNullOrWhiteSpace(secao.Aviso)) ? "parcial" : "concluido";
        if (resposta.Estado != estadoEsperado)
            throw new InvalidDataException("O estado informado diverge dos dados da consulta.");
        return resposta;
    }
}
