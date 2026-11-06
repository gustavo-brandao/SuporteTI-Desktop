using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;

namespace SuporteTI.Grafico;

internal sealed record RetornoElevado(Resposta Resultado, byte[]? Html);
internal sealed record ArquivoElevado(string Hash, byte[] Dados, byte[] Zona);
internal sealed record ParametrosIntervencao(string Alvo, string IPv4 = "", string Prefixo = "", string Gateway = "",
    string Dns1 = "", string Dns2 = "");

internal static class ElevacaoConsulta
{
    private static readonly Dictionary<string, string> Arquivos = new()
    {
        [@"Administrativo.ps1"] = "Administrativo",
        [@"Diagnosticos.ps1"] = "Diagnosticos",
        [@"Intervencoes.ps1"] = "Intervencoes",
        [@"FluxosSensiveis.ps1"] = "FluxosSensiveis",
        [@"Coletores\Manutencao.ps1"] = "Manutencao",
        [@"Coletores\Perfis.ps1"] = "Perfis",
        [@"Coletores\Logon.ps1"] = "Logon",
        [@"Coletores\SuporteTI.Windows.dll"] = "Windows",
        [@"Coletores\Operacoes.ps1"] = "Operacoes",
        [@"Coletores\Rede.ps1"] = "Rede",
        [@"Coletores\Discos.ps1"] = "Discos",
        [@"Ferramentas\smartmontools\integridade.json"] = "Integridade",
        [@"Ferramentas\smartmontools\bin\smartctl.exe"] = "Smartctl",
        [@"Ferramentas\smartmontools\bin\drivedb.h"] = "Drivedb"
    };
    private static readonly HashSet<string> Operacoes =
        ["diskhealth", "gpcomputador", "gpcomputerhtml", "sfcverify", "dismcheck", "dismscan", "chkdskread",
            "spooler", "spoolerclear", "cache", "gpupdatecomputer", "netdhcp", "netstatic",
            "sfcrepair", "dismrestore", "chkdskfull", "perfillocal", "autologonenable", "autologondisable"];
    private static readonly SecurityIdentifier Administradores = new(WellKnownSidType.BuiltinAdministratorsSid, null);
    private static readonly SecurityIdentifier Sistema = new(WellKnownSidType.LocalSystemSid, null);
    private const int LimiteResposta = 16 * 1024 * 1024;

    internal static bool EhAdministrativa(string operacao) => Operacoes.Contains(operacao);

    public static async Task<RetornoElevado> ConsultarAsync(string operacao, string volume, CancellationToken token,
        ParametrosIntervencao? parametros = null)
    {
        if (!EhAdministrativa(operacao))
            throw new ArgumentException("Consulta administrativa desconhecida.", nameof(operacao));
        if (operacao is "chkdskread" or "chkdskfull" && (volume.Length != 2 || volume[0] is < 'A' or > 'Z' || volume[1] != ':'))
            throw new ArgumentException("Selecione a letra de um volume local.", nameof(volume));
        if (operacao is "netdhcp" or "netstatic" && (parametros is null ||
            !Guid.TryParse(parametros.Alvo, out var adaptador) || adaptador == Guid.Empty))
            throw new ArgumentException("Selecione um adaptador físico.", nameof(parametros));
        if (operacao == "spoolerclear" && (string.IsNullOrWhiteSpace(parametros?.Alvo) || parametros.Alvo.Length > 512))
            throw new ArgumentException("Selecione uma fila local.", nameof(parametros));

        var origem = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Modulos"));
        var unc = origem.StartsWith(@"\\", StringComparison.Ordinal);
        if (!unc && new DriveInfo(Path.GetPathRoot(origem)!).DriveType == DriveType.Network)
            throw new InvalidOperationException("Abra o pacote por seu caminho UNC para que a conta administrativa possa acessar os arquivos.");

        var esperados = ValidarArquivos(origem);
        var canal = "SuporteTI-" + Guid.NewGuid().ToString("N");
        using var servidor = CriarCanal(canal);
        using var entrada = new StreamReader(AbrirRecurso("Entrada"), Encoding.UTF8);
        var dados = JsonSerializer.SerializeToUtf8Bytes(new
        {
            Operacao = operacao, Volume = volume, Canal = canal, Rede = unc, Parametros = parametros,
            Arquivos = esperados.Select(item => new { Nome = item.Key, item.Value.Hash, Tamanho = item.Value.Dados.Length })
        });
        var comando = entrada.ReadToEnd().Replace("__DADOS__", Convert.ToBase64String(dados), StringComparison.Ordinal);
        var inicio = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"),
            WorkingDirectory = Environment.SystemDirectory,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden
        };
        foreach (var argumento in new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-STA", "-ExecutionPolicy",
            unc ? "AllSigned" : "RemoteSigned", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(comando)) })
            inicio.ArgumentList.Add(argumento);

        Process processo;
        token.ThrowIfCancellationRequested();
        try
        {
            processo = await Task.Run(() => Process.Start(inicio) ?? throw new InvalidOperationException("O Windows não iniciou a consulta administrativa."), token)
                .ConfigureAwait(false);
        }
        catch (Win32Exception erro) when (erro.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("O pedido de UAC foi cancelado.", erro);
        }

        using (processo)
        using (var prazo = CancellationTokenSource.CreateLinkedTokenSource(token))
        {
            if (operacao is not ("sfcrepair" or "dismrestore" or "chkdskfull" or "perfillocal" or "autologonenable"))
                prazo.CancelAfter(TimeSpan.FromMinutes(32));
            try
            {
                await ConectarAsync(servidor, processo, prazo.Token).ConfigureAwait(false);
                var leitura = LerRespostaAsync(servidor, prazo.Token);
                try
                {
                    foreach (var arquivo in esperados.Values)
                    {
                        await servidor.WriteAsync(BitConverter.GetBytes(arquivo.Zona.Length), prazo.Token).ConfigureAwait(false);
                        await servidor.WriteAsync(arquivo.Zona, prazo.Token).ConfigureAwait(false);
                        await servidor.WriteAsync(arquivo.Dados, prazo.Token).ConfigureAwait(false);
                    }
                }
                catch (IOException) when (!prazo.IsCancellationRequested)
                {
                    // O filho pode fechar a entrada após devolver um erro de preparação.
                }
                var json = await leitura.ConfigureAwait(false);
                await processo.WaitForExitAsync(prazo.Token).ConfigureAwait(false);
                using var envelope = JsonDocument.Parse(json);
                var resultado = ConsultaPowerShell.Interpretar(envelope.RootElement.GetProperty("resultado").GetRawText(), operacao);
                var html = envelope.RootElement.TryGetProperty("html", out var anexo) && anexo.ValueKind == JsonValueKind.String
                    ? anexo.GetBytesFromBase64() : null;
                if ((html is not null && operacao != "gpcomputerhtml") ||
                    (operacao == "gpcomputerhtml" && resultado.Estado == "concluido" && html is not { Length: > 0 }))
                    throw new InvalidDataException("Anexo administrativo inesperado ou ausente.");
                return new RetornoElevado(resultado, html);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                throw new TimeoutException("A consulta administrativa ultrapassou o tempo de espera.");
            }
        }
    }

    internal static NamedPipeServerStream CriarCanal(string nome)
    {
        using var identidade = WindowsIdentity.GetCurrent();
        var usuario = identidade.User ?? throw new InvalidOperationException("A sessão do Windows não informou seu identificador.");
        var permissoes = new PipeSecurity();
        permissoes.SetAccessRuleProtection(true, false);
        permissoes.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
            PipeAccessRights.FullControl, AccessControlType.Deny));
        foreach (var conta in new[] { usuario, Administradores, Sistema })
            permissoes.AddAccessRule(new PipeAccessRule(conta, PipeAccessRights.FullControl, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(nome, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
            0, 0, permissoes);
    }

    internal static Dictionary<string, ArquivoElevado> ValidarArquivos(string origem)
    {
        var esperados = new Dictionary<string, ArquivoElevado>(StringComparer.Ordinal);
        foreach (var (relativo, recurso) in Arquivos)
        {
            using var embutido = AbrirRecurso(recurso);
            var hash = SHA256.HashData(embutido);
            var caminho = Path.Combine(origem, relativo);
            ConfirmarCaminho(caminho);
            using var fonte = File.OpenRead(caminho);
            if (fonte.Length != embutido.Length)
                throw new InvalidDataException($"Arquivo de diagnóstico alterado: {relativo}. Extraia novamente o pacote.");
            var dados = new byte[checked((int)embutido.Length)];
            fonte.ReadExactly(dados);
            if (!SHA256.HashData(dados).AsSpan().SequenceEqual(hash))
                throw new InvalidDataException($"Arquivo de diagnóstico alterado: {relativo}. Extraia novamente o pacote.");
            esperados.Add(relativo, new ArquivoElevado(Convert.ToHexString(hash), dados, LerZona(caminho)));
        }
        return esperados;
    }

    private static void ConfirmarCaminho(string caminho)
    {
        var raiz = Path.GetPathRoot(caminho);
        for (string? parte = caminho; parte is not null && !string.Equals(parte.TrimEnd('\\'),
            raiz?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase); parte = Path.GetDirectoryName(parte))
        {
            if (File.GetAttributes(parte).HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidDataException("A elevação exige arquivos e pastas sem redirecionamento por ponto de reanálise.");
        }
    }

    private static byte[] LerZona(string caminho)
    {
        try
        {
            using var fluxo = File.OpenRead(caminho + ":Zone.Identifier");
            if (fluxo.Length > 16_384)
                throw new InvalidDataException("A marca de origem do arquivo excedeu o limite de tamanho.");
            var zona = new byte[(int)fluxo.Length];
            fluxo.ReadExactly(zona);
            return zona;
        }
        catch (IOException erro) when ((erro.HResult & 0xffff) is 1 or 2 or 3 or 50 or 123)
        {
            return [];
        }
    }

    internal static async Task ConectarAsync(NamedPipeServerStream servidor, Process processo, CancellationToken token)
    {
        using var espera = CancellationTokenSource.CreateLinkedTokenSource(token);
        espera.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            while (true)
            {
                await servidor.WaitForConnectionAsync(espera.Token).ConfigureAwait(false);
                if (GetNamedPipeClientProcessId(servidor.SafePipeHandle, out var pid) && pid == (uint)processo.Id)
                    return;
                servidor.Disconnect();
            }
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new TimeoutException("O processo administrativo não abriu o canal de retorno. Confira a política de execução do Windows.");
        }
    }

    private static async Task<byte[]> LerRespostaAsync(Stream canal, CancellationToken token)
    {
        using var dados = new MemoryStream();
        var bloco = new byte[8192];
        int lidos;
        while ((lidos = await canal.ReadAsync(bloco, token).ConfigureAwait(false)) > 0)
        {
            if (dados.Length + lidos > LimiteResposta)
                throw new InvalidDataException("O resultado administrativo excedeu o limite de tamanho.");
            dados.Write(bloco, 0, lidos);
        }
        return dados.ToArray();
    }

    private static Stream AbrirRecurso(string nome) =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream("Elevacao." + nome)
        ?? throw new InvalidOperationException("O executável não inclui a configuração administrativa.");

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);
}
