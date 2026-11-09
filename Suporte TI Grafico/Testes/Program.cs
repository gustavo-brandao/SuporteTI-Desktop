using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using SuporteTI.Grafico;

if (args.Contains("--catalogo"))
{
    Console.OutputEncoding = new UTF8Encoding(false);
    Console.WriteLine(JsonSerializer.Serialize(CatalogoFerramentas.Itens.Select(item =>
        new { item.Id, item.Grupo, item.Area, Rotulo = item.ToString() })));
    return;
}

var temporaria = Directory.CreateTempSubdirectory("Suporte TI gráfico ");
var passou = 0;
try
{
    var idsAnteriores = ("sistema desempenho discos bateria dispositivos servicos programas autologon chromecache " +
        "rede dominio ping velocidade netdhcp netstatic falhas codigofalha update filaimpressao spooler spoolerclear cache " +
        "gpusuario gphtml gpupdateuser gpupdatecomputer ultimo eventos evtx diskhealth gpcomputador gpcomputerhtml " +
        "sfcverify dismcheck dismscan chkdskread sfcrepair dismrestore chkdskfull perfillocal autologonenable autologondisable monitor mapaocupacao").Split(' ');
    Igual(true, CatalogoFerramentas.Itens.Select(item => item.Id).Order().SequenceEqual(idsAnteriores.Order()),
        "Catálogo preserva as 44 ferramentas sem duplicar IDs");
    Igual(true, CatalogoFerramentas.Grupos.Count == 7 && CatalogoFerramentas.Itens.All(item =>
        CatalogoFerramentas.Grupos.Contains(item.Grupo)), "Toda ferramenta tem um dos sete destinos");
    Igual(true, CatalogoFerramentas.Itens.Where(item => item.Id is "diskhealth" or "discos" or "mapaocupacao")
        .All(item => item.Grupo == "Hardware" && item.Area == "Discos"), "Discos e mapa compartilham a mesma área");
    Igual(true, CatalogoFerramentas.Buscar("saude discos").Any(item => item.Id == "diskhealth"),
        "Busca combina palavras e ignora acentos");
    Igual(0, CatalogoFerramentas.Buscar("ferramenta inexistente zzz").Length, "Busca sem correspondência");
    var nomes = new[] { "Computador", "Windows", "BIOS", "CPU", "Memória em uso" };
    string Json(string estado, bool temDados, bool parcial = false) => JsonSerializer.Serialize(new
    {
        versao = 1,
        operacao = "sistema",
        estado,
        secoes = nomes.Select((nome, i) => new
        {
            nome,
            campos = temDados && !(parcial && i == 2)
                ? new[] { new { nome = "Descrição", valor = "Leitura de teste" } } : [],
            aviso = !temDados || (parcial && i == 2) ? "Indisponível" : null
        })
    });

    var completo = Json("concluido", true);
    Igual("concluido", ConsultaPowerShell.Interpretar(completo).Estado, "JSON concluído");
    Igual("parcial", ConsultaPowerShell.Interpretar(Json("parcial", true, true)).Estado, "JSON parcial");
    Igual("indisponivel", ConsultaPowerShell.Interpretar(Json("indisponivel", false)).Estado, "JSON sem dados");
    await Falha<InvalidDataException>(() => Task.FromResult(ConsultaPowerShell.Interpretar("{")), "JSON inválido");
    await Falha<InvalidDataException>(() => Task.FromResult(ConsultaPowerShell.Interpretar(completo.Replace("\"versao\":1", "\"versao\":2"))), "Versão desconhecida");
    await Falha<InvalidDataException>(() => Task.FromResult(ConsultaPowerShell.Interpretar(Json("concluido", false))), "Estado incoerente");
    await Falha<InvalidDataException>(() => Task.FromResult(ConsultaPowerShell.Interpretar(completo, "bateria")), "Resposta de outro diagnóstico rejeitada");
    await Falha<ArgumentException>(() => new ConsultaPowerShell().ConsultarAsync("editar", default), "Operação fora do catálogo rejeitada");

    var resposta = await Fixture("resposta.ps1",
        "[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)\n" +
        "[Console]::Error.WriteLine('Diagnóstico fora do JSON')\n" +
        "[Console]::WriteLine('" + completo + "')").ConsultarSistemaAsync(default);
    Igual("Memória em uso", resposta.Secoes[4].Nome, "Processo real em caminho com espaços e acentos");
    await Falha<InvalidDataException>(() => Fixture("invalido.ps1", "'saida inesperada'").ConsultarSistemaAsync(default), "Resposta inválida via processo");
    await Falha<InvalidOperationException>(() => Fixture("falha.ps1", "[Console]::Error.WriteLine('erro de teste'); exit 7").ConsultarSistemaAsync(default), "Falha do processo", "código 7");
    await Falha<InvalidOperationException>(() => new ConsultaPowerShell(Path.Combine(temporaria.FullName, "ausente.ps1"), TimeSpan.FromSeconds(2)).ConsultarSistemaAsync(default), "Módulo ausente");
    await Falha<TimeoutException>(() => Fixture("tempo.ps1", "Start-Sleep -Seconds 20", TimeSpan.FromSeconds(2)).ConsultarSistemaAsync(default), "Limite de tempo");
    using var cancelar = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    await Falha<OperationCanceledException>(() => Fixture("cancelar.ps1", "Start-Sleep -Seconds 20").ConsultarSistemaAsync(cancelar.Token), "Cancelamento");

    var real = await new ConsultaPowerShell().ConsultarSistemaAsync(default);
    Igual("sistema", real.Operacao, "Contrato da consulta instalada");
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("format", "", default), "Elevação recusa operação fora da lista");
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("chkdskread", "C: /f", default), "CHKDSK recusa parâmetro livre");
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("chkdskfull", "C: & whoami", default), "CHKDSK completo recusa argumento livre");
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("netstatic", "", default, new("adaptador inválido")),
        "Configuração de IPv4 exige identificador do adaptador");
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("spoolerclear", "", default), "Limpeza exige fila local selecionada");
    var pacoteAlterado = temporaria.CreateSubdirectory("pacote alterado");
    File.WriteAllText(Path.Combine(pacoteAlterado.FullName, "Administrativo.ps1"), "arquivo alterado");
    await Falha<InvalidDataException>(() => Task.FromResult(ElevacaoConsulta.ValidarArquivos(pacoteAlterado.FullName)),
        "Arquivo alterado é recusado antes da elevação");

    var nomeCanal = "SuporteTI-Teste-" + Guid.NewGuid().ToString("N");
    using (var canal = ElevacaoConsulta.CriarCanal(nomeCanal))
    using (var prazoCanal = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
    {
        var processoCanal = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"))
        {
            UseShellExecute = false, CreateNoWindow = true
        };
        var comandoCanal = "$c=[IO.Pipes.NamedPipeClientStream]::new('.', '" + nomeCanal +
            "', 'InOut', 'None', 'Identification'); try { $c.Connect(5000); $c.WriteByte($c.ReadByte()+1); $c.Flush() } finally { $c.Dispose() }";
        foreach (var arg in new[] { "-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(comandoCanal)) })
            processoCanal.ArgumentList.Add(arg);
        // Entra antes do processo esperado para comprovar a rejeição por PID.
        using var intruso = new NamedPipeClientStream(".", nomeCanal, PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.Identification);
        await intruso.ConnectAsync(prazoCanal.Token);
        using var clienteCanal = Process.Start(processoCanal)!;
        await ElevacaoConsulta.ConectarAsync(canal, clienteCanal, prazoCanal.Token);
        await canal.WriteAsync(new byte[] { 41 }, prazoCanal.Token);
        var amostraCanal = new byte[1];
        var tamanho = await canal.ReadAsync(amostraCanal, prazoCanal.Token);
        Igual(true, tamanho == 1 && amostraCanal[0] == 42, "Canal rejeita PID errado e troca dados com o filho");
    }
    var htmlComputador = LogConsulta.SalvarHtmlComputador(temporaria.FullName, "<html>GPO</html>"u8.ToArray(), DateTimeOffset.Now);
    Igual("<html>GPO</html>", File.ReadAllText(htmlComputador), "HTML é salvo no destino pela sessão original");
    foreach (var operacao in new[] { "desempenho", "discos", "bateria", "dispositivos", "servicos", "programas", "autologon" })
        Igual(operacao, (await new ConsultaPowerShell().ConsultarAsync(operacao, default)).Operacao, $"Contrato de {operacao}");
    var cliente = new ConsultaPowerShell();
    foreach (var operacao in new[] { "rede", "dominio", "falhas" })
        Igual(operacao, (await cliente.ConsultarAsync(operacao, default)).Operacao, $"Contrato de {operacao}");
    var pingInvalido = await cliente.ConsultarAsync("ping", default, "127.0.0.1 -t");
    Igual("indisponivel", pingInvalido.Estado, "Ping rejeita argumentos extras");
    var codigo = await cliente.ConsultarAsync("codigofalha", default, "5", "Win32");
    var hexadecimal = await cliente.ConsultarAsync("codigofalha", default, "0x5", "Win32");
    Igual(true, codigo.Estado == "concluido" && codigo.Secoes[0].Campos.SequenceEqual(hexadecimal.Secoes[0].Campos),
        "Código decimal e hexadecimal têm a mesma descrição");
    Igual("concluido", (await cliente.ConsultarAsync("codigofalha", default, "-2147024891", "HRESULT")).Estado,
        "Código HRESULT decimal com sinal");
    var ambiguo = await cliente.ConsultarAsync("codigofalha", default, "5");
    Igual(true, ambiguo.Secoes.Any(secao => secao.Nome == "Win32") &&
        ambiguo.Secoes.Any(secao => secao.Nome == "Paradas"), "Pesquisa automática mantém as duas famílias do código 5");
    var hresultAutomatico = await cliente.ConsultarAsync("codigofalha", default, "-2147024891");
    Igual(true, hresultAutomatico.Secoes.Any(secao => secao.Nome == "HRESULT"), "Pesquisa automática aceita HRESULT decimal com sinal");
    var excecaoAutomatica = await cliente.ConsultarAsync("codigofalha", default, "0xC0000005");
    Igual(true, excecaoAutomatica.Secoes.Any(secao => secao.Nome == "Excecoes"), "Pesquisa automática identifica NTSTATUS");
    Igual("indisponivel", (await cliente.ConsultarAsync("codigofalha", default, "-0")).Estado,
        "Entrada inválida não causa estouro na pesquisa automática");

    var modulosSimulados = temporaria.CreateSubdirectory("coletores simulados");
    var coletores = modulosSimulados.CreateSubdirectory("Coletores");
    var modulosReais = Path.Combine(AppContext.BaseDirectory, "Modulos");
    foreach (var arquivo in Directory.EnumerateFiles(modulosReais, "*.ps1"))
        File.Copy(arquivo, Path.Combine(modulosSimulados.FullName, Path.GetFileName(arquivo)));
    foreach (var nome in new[] { "Falhas.ps1", "SuporteTI.Windows.dll" })
        File.Copy(Path.Combine(modulosReais, "Coletores", nome), Path.Combine(coletores.FullName, nome));
    var simulado = new ConsultaPowerShell(Path.Combine(modulosSimulados.FullName, "Consultar.ps1"), TimeSpan.FromSeconds(25));
    foreach (var (expressao, esperado) in new (string, double?)[]
    {
        ("[pscustomobject]@{}", null), ("$null", null), ("12.5", 12.5),
        ("125", 125), ("[double]::NaN", null), ("[double]::PositiveInfinity", null), ("-1", null)
    })
    {
        File.WriteAllText(Path.Combine(coletores.FullName, "Discos.ps1"),
            "function Get-SuporteSmartResumo { param([switch]$Estruturado) " +
            "[pscustomobject]@{ Nome='Disco simulado'; Texto='Leitura de teste'; Desgaste=" + expressao + " } }",
            new UTF8Encoding(true));
        var discosSimulados = await simulado.ConsultarAsync("discos", default);
        Igual(esperado, discosSimulados.Secoes[1].Campos.Single().Percentual,
            $"Desgaste transporta número ou null para {expressao}");
    }
    var diagnosticosSimulados = Path.Combine(modulosSimulados.FullName, "Diagnosticos.ps1");
    var diagnosticosOriginais = File.ReadAllBytes(diagnosticosSimulados);
    File.AppendAllText(diagnosticosSimulados, """

        function Get-CimInstance {
            param($ClassName, $Filter, $OperationTimeoutSec)
            [pscustomobject]@{ Name=' '; DeviceID='PCI\TESTE'; ConfigManagerErrorCode=28; PNPClass='System'; Status='Error' }
        }
        """);
    var pnpSemNome = await simulado.ConsultarAsync("dispositivos", default);
    File.WriteAllBytes(diagnosticosSimulados, diagnosticosOriginais);
    Igual("PCI\\TESTE", pnpSemNome.Secoes[0].Campos.Single().Nome, "Dispositivo sem nome usa o identificador PnP");
    var semCatalogo = await simulado.ConsultarAsync("codigofalha", default, "5", "Win32");
    Igual("parcial", semCatalogo.Estado, "Catálogo ausente preserva aviso e código");
    File.AppendAllText(Path.Combine(coletores.FullName, "Falhas.ps1"), """

        function Get-WinEvent {
            param($FilterHashtable, $MaxEvents)
            $evento = [pscustomobject]@{
                LogName='System'; ProviderName='Microsoft-Windows-Kernel-Power'; Id=41
                RecordId=1; TimeCreated=Get-Date
            }
            $evento | Add-Member ScriptMethod ToXml { '<Event><EventData><Data Name="BugcheckCode">239</Data></EventData></Event>' }
            $evento
        }
        """);
    var triagem = await simulado.ConsultarAsync("falhas", default);
    Igual(true, triagem.Secoes.SelectMany(secao => secao.Campos).Any(campo => campo.Valor == "0x000000EF" && campo.Familia == "Paradas"),
        "Triagem preserva código e família para preencher a pesquisa");
    foreach (var operacao in new[] { "update", "filaimpressao", "gpusuario", "eventos" })
        Igual(operacao, (await cliente.ConsultarAsync(operacao, default)).Operacao, $"Contrato de {operacao}");
    var destinoInvalido = await cliente.ConsultarAsync("evtx", default, "caminho-relativo");
    Igual("indisponivel", destinoInvalido.Estado, "Exportação rejeita destino relativo");
    var atendimentoSimulado = Path.Combine(modulosSimulados.FullName, "Atendimento.ps1");
    File.AppendAllText(atendimentoSimulado, """

        function Get-Service { param($Name) [pscustomobject]@{ Status = 'Stopped' } }
        function Get-Printer { throw 'O spooler parado não deve enumerar filas.' }
        function Get-WinEvent { throw 'Acesso negado simulado.' }
        """);
    var spoolerParado = await simulado.ConsultarAsync("filaimpressao", default);
    Igual(true, spoolerParado.Secoes[0].Campos.Any(campo => campo.Nome == "Spooler" && campo.Valor.Contains("Stopped")),
        "Spooler parado é mostrado sem enumerar filas");
    Igual("indisponivel", (await simulado.ConsultarAsync("eventos", default)).Estado, "Eventos preservam o aviso de acesso negado");
    File.AppendAllText(atendimentoSimulado, """

        function Get-Service { param($Name) [pscustomobject]@{ Status = 'Running' } }
        function Get-Printer { [pscustomobject]@{ Name='Fila teste'; PortName='LPT1'; PrinterStatus='Normal' } }
        function Get-PrintJob { throw 'Fila inacessível.' }
        """);
    var filaInacessivel = await simulado.ConsultarAsync("filaimpressao", default);
    Igual(true, filaInacessivel.Estado == "parcial" && filaInacessivel.Secoes[0].Campos.Any(campo => campo.Nome == "Fila teste"),
        "Fila visível é preservada quando seus trabalhos estão inacessíveis");
    var redeOriginal = File.ReadAllText(Path.Combine(modulosReais, "Coletores", "Rede.ps1"));
    File.WriteAllText(Path.Combine(coletores.FullName, "Rede.ps1"), redeOriginal + """

        function New-Object {
            param([string]$TypeName)
            $objeto = [pscustomobject]@{ Tentativa = 0 }
            $objeto | Add-Member ScriptMethod Send {
                param($endereco,$prazo)
                $this.Tentativa++
                if ($endereco.ToString() -eq '127.0.0.1' -and $this.Tentativa -lt 4) {
                    return [pscustomobject]@{ Status = 'Success'; RoundtripTime = 4 }
                }
                [pscustomobject]@{ Status = 'TimedOut'; RoundtripTime = 0 }
            }
            $objeto | Add-Member ScriptMethod Dispose {}
            $objeto
        }
        function Measure-SuporteVelocidade {
            param($Modo)
            if ($Modo -eq 'Upload') { throw 'Tempo limite simulado.' }
            'Download: 42 Mbps (amostra de 8 MiB)'
        }
        """, new UTF8Encoding(true));
    var pingParcial = await simulado.ConsultarAsync("ping", default, "127.0.0.1");
    var pingSemResposta = await simulado.ConsultarAsync("ping", default, "127.0.0.2");
    Igual(true, pingParcial.Secoes[0].Campos.Any(campo => campo.Nome == "Respostas" && campo.Valor.Contains("25%")), "Ping preserva perda parcial");
    Igual(true, pingSemResposta.Secoes[0].Campos.Any(campo => campo.Valor.Contains("SEM RESPOSTA")), "Ping preserva ausência de respostas");
    var velocidade = await simulado.ConsultarAsync("velocidade", default);
    Igual(true, velocidade.Estado == "parcial" && velocidade.Secoes[0].Campos.Length > 0 &&
        velocidade.Secoes[1].Aviso == "Tempo limite simulado.", "Falha no upload preserva a medição de download");
    var agora = DateTimeOffset.Now;
    var caminhoLog = LogConsulta.Salvar(temporaria.FullName, "Velocidade", velocidade, agora, agora);
    var outroLog = LogConsulta.Salvar(temporaria.FullName, "Velocidade", velocidade, agora, agora);
    var conteudoLog = File.ReadAllText(caminhoLog);
    Igual(true, caminhoLog != outroLog && conteudoLog.Contains("42 Mbps") && conteudoLog.Contains("Tempo limite simulado."),
        "Logs únicos preservam resultado e aviso");

    var pastaMapa = temporaria.CreateSubdirectory("mapa");
    var subpasta = pastaMapa.CreateSubdirectory("maior");
    File.WriteAllBytes(Path.Combine(pastaMapa.FullName, "menor.bin"), new byte[128]);
    File.WriteAllBytes(Path.Combine(subpasta.FullName, "maior.bin"), new byte[512]);
    var mapa = VarreduraDisco.Ler(pastaMapa.FullName, default);
    Igual(640L, mapa.Raiz.Bytes, "Mapa soma arquivos e subpastas");
    Igual("maior", mapa.Raiz.Filhos![0].Nome, "Mapa ordena por tamanho");
    Igual(512L, mapa.Raiz.Filhos[0].Bytes, "Tamanho da subpasta");
    Igual(true, VarreduraDisco.Ler(pastaMapa.FullName, default, maxItens: 1).Limite is not null, "Varredura sinaliza limite de itens");
    await Falha<OperationCanceledException>(() => Task.FromResult(VarreduraDisco.Ler(pastaMapa.FullName, new CancellationToken(true))), "Varredura aceita cancelamento");
    Igual<double?>(75, MonitorRecursos.CalcularCpu((100, 200, 300), (125, 275, 325)), "Carga de CPU desconta ociosidade");
    Igual<double?>(null, MonitorRecursos.CalcularCpu((100, 200, 300), (100, 200, 300)), "CPU sem intervalo fica indisponível");
    var monitor = new MonitorRecursos();
    monitor.Ler();
    await Task.Delay(350);
    var amostra = monitor.Ler();
    Igual(true, amostra.Cpu is >= 0 and <= 100 && amostra.Ram is >= 0 and <= 100, "Amostra real de CPU e RAM");
    if (args.Contains("--exigir-cim"))
    {
        Igual(true, real.Secoes[0].Campos.Any(campo => campo.Nome == "Nome" && campo.Valor.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)), "Nome real do computador");
        Igual(true, real.Secoes[1].Campos.Any(campo => campo.Nome == "Build" && campo.Valor == Environment.OSVersion.Version.Build.ToString()), "Build real do Windows");
    }
    Console.WriteLine($"Resultado: {passou} testes aprovados. Estado da consulta instalada: {real.Estado}.");
}
finally
{
    temporaria.Delete(recursive: true);
}

ConsultaPowerShell Fixture(string nome, string script, TimeSpan? limite = null)
{
    var caminho = Path.Combine(temporaria.FullName, nome);
    File.WriteAllText(caminho, "param([string]$Operacao)\n" + script, new UTF8Encoding(true));
    return new ConsultaPowerShell(caminho, limite ?? TimeSpan.FromSeconds(25));
}

void Igual<T>(T esperado, T obtido, string teste)
{
    if (!EqualityComparer<T>.Default.Equals(esperado, obtido))
        throw new InvalidOperationException($"{teste}: valor inesperado.");
    passou++;
    Console.WriteLine($"OK  {teste}");
}

async Task Falha<T>(Func<Task> acao, string teste, string? trecho = null) where T : Exception
{
    try
    {
        await acao();
    }
    catch (T erro) when (trecho is null || erro.Message.Contains(trecho))
    {
        passou++;
        Console.WriteLine($"OK  {teste}");
        return;
    }
    throw new InvalidOperationException($"{teste}: era esperada uma falha {typeof(T).Name}.");
}
