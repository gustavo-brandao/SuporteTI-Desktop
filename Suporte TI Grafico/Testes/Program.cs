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
    {
        var fluxo = FluxoFerramentas.Obter(item.Id);
        var opcao = fluxo?.Opcoes.Single(opcao => opcao.Id == item.Id);
        var principal = FluxoFerramentas.Listar(item.Grupo, item.Area).Single(principal => principal.Id == FluxoFerramentas.Principal(item.Id));
        return new { item.Id, item.Grupo, item.Area, Rotulo = item.ToString(), Principal = principal.ToString(),
            Escopo = opcao?.Escopo, Acao = opcao?.Acao };
    })));
    return;
}

var temporaria = Directory.CreateTempSubdirectory("Suporte TI gráfico ");
var passou = 0;
try
{
    Igual(true, new double?[] { null, -1, 101, double.NaN, double.PositiveInfinity }.All(n => MedidasVisuais.Percentual(n) is null),
        "Barras não apresentam ausência, valores inválidos ou não finitos como zero");
    Igual(true, MedidasVisuais.Percentual(0) == 0 && MedidasVisuais.Percentual(100) == 100,
        "Barras preservam extremos reais de 0 e 100 por cento");
    Igual(true, new double?[] { null, -1, double.NaN, double.PositiveInfinity }.All(n => MedidasVisuais.Taxa(n) is null) && MedidasVisuais.Taxa(0) == 0,
        "Velocímetro distingue taxa zero de leitura indisponível");
    Igual(true, MedidasVisuais.Escala(100, 101) == 250 && MedidasVisuais.Escala(250, 900) == 1000 &&
        MedidasVisuais.Escala(1000, 40) == 1000 && MedidasVisuais.Escala(1000, null) == 1000,
        "Escala cobre a taxa observada e não encolhe nas quedas ou leituras parciais");
    Igual(true, MedidasVisuais.Escala(100, 12500) >= 12500 && double.IsFinite(MedidasVisuais.Escala(100, double.PositiveInfinity)),
        "Escala cobre conexões acima de 10 Gbps e recusa infinito");
    Igual(true, MedidasVisuais.LerTaxa($"{1234.5:N1} Mbps") == 1234.5 && MedidasVisuais.LerTaxa("indisponível") is null,
        "Resultado final do velocímetro respeita a cultura e a indisponibilidade");
    await TesteVelocidadeCenarios.Executar((condicao, nome) => Igual(true, condicao, nome));
    if (args.Contains("--testar-velocidade"))
    {
        Console.WriteLine($"Resultado: {passou} testes de velocidade aprovados, sem tráfego externo.");
        return;
    }
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
    Igual(true, CatalogoFerramentas.Itens.All(item =>
    {
        var fluxo = FluxoFerramentas.Obter(item.Id);
        var opcao = fluxo?.Opcoes.Single(opcao => opcao.Id == item.Id);
        return FluxoFerramentas.Listar(item.Grupo, item.Area).Count(principal => principal.Id == FluxoFerramentas.Principal(item.Id)) == 1 &&
            (opcao is null || fluxo!.Resolver(opcao.Escopo, opcao.Acao).Id == item.Id);
    }), "As 44 rotas chegam a uma operação explícita nos formulários");
    Igual(35, CatalogoFerramentas.Itens.GroupBy(item => (item.Grupo, item.Area))
        .Sum(area => FluxoFerramentas.Listar(area.Key.Grupo, area.Key.Area).Length), "Agrupamento reduz o seletor de 44 para 35 entradas");
    var gpo = FluxoFerramentas.Obter("gpusuario")!;
    Igual(true, gpo.Acoes("Usuário da sessão").Select(opcao => opcao.Id).SequenceEqual(["gpusuario", "gphtml", "gpupdateuser"]) &&
        gpo.Acoes("Usuário da sessão").All(opcao => !ElevacaoConsulta.EhAdministrativa(opcao.Id)), "GPO do usuário conserva identidade da sessão");
    Igual(true, gpo.Acoes("Computador").Select(opcao => opcao.Id).SequenceEqual(["gpcomputador", "gpcomputerhtml", "gpupdatecomputer"]) &&
        gpo.Acoes("Computador").All(opcao => ElevacaoConsulta.EhAdministrativa(opcao.Id)), "GPO do computador mantém UAC nas três ações");
    Igual(true, FluxoFerramentas.Obter("netstatic")!.Opcoes.Select(opcao => opcao.Id).SequenceEqual(["netdhcp", "netstatic"]),
        "IPv4 não confunde configuração automática com manual");
    Igual(true, FluxoFerramentas.Obter("eventos")!.Opcoes.Select(opcao => opcao.Id).SequenceEqual(["falhas", "eventos"]),
        "Leituras de eventos preservam seus coletores distintos");
    Igual(true, FluxoFerramentas.Obter("spoolerclear")!.Opcoes.Select(opcao => opcao.Id).SequenceEqual(["filaimpressao", "spooler", "spoolerclear"]),
        "Impressão começa em diagnóstico e preserva duas intervenções distintas");
    await Falha<InvalidOperationException>(() => Task.FromResult(gpo.Resolver("Outro usuário", "Consultar políticas")),
        "Escopo desconhecido não seleciona uma operação por aproximação");
    await Falha<InvalidOperationException>(() => Task.FromResult(gpo.Resolver("Computador", "Atualizar e reiniciar")),
        "Ação desconhecida não executa uma manutenção alternativa");
    var resumoInconclusivo = ResumoDiscos.Criar(new Resposta(1, "discos", "parcial",
        [new Secao("Discos", [
            new Campo("C:", "99% ocupado", Percentual: 99, Metrica: "ocupacao"),
            new Campo("Disco 0", "Indicadores limitados", Percentual: 0, Metrica: "desgaste", EstadoDisco: "Inconclusivo")], "Leitura limitada")]));
    Igual(true, resumoInconclusivo.Contains("0 com alerta") && resumoInconclusivo.Contains("1 inconclusivo(s)") &&
        resumoInconclusivo.Contains("Leitura parcial"), "Ocupação elevada e desgaste zero não inventam saúde positiva");
    var resumoAlertas = ResumoDiscos.Criar(new Resposta(1, "diskhealth", "parcial",
        [new Secao("Discos", [
            new Campo("Disco 0", "Alerta", Metrica: "desgaste", EstadoDisco: "Atencao"),
            new Campo("Disco 1", "Degradado", Metrica: "desgaste", EstadoDisco: "Critico/degradado")], "Consulta limitada")]));
    Igual(true, resumoAlertas.Contains("2 com alerta") && resumoAlertas.Contains("Leitura parcial"),
        "Resumo conserva alertas nativos e leitura parcial");
    Igual(true, ResumoDiscos.Criar(new Resposta(1, "discos", "indisponivel",
        [new Secao("Discos", [], "Sem dados")])).Contains("Saúde física indisponível"),
        "Ausência de discos lidos conserva indisponibilidade");
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

    var preferencia = Path.Combine(temporaria.FullName, "configuração do usuário", "aparencia.json");
    Igual(ModoTema.Escuro, PreferenciasAparencia.Ler(preferencia).Tema, "Primeira abertura usa Escuro mesmo em sistema claro");
    foreach (var tema in Enum.GetValues<ModoTema>())
    {
        PreferenciasAparencia.Salvar(preferencia, tema);
        Igual(tema, PreferenciasAparencia.Ler(preferencia).Tema, "Preferência persistida: " + tema);
    }
    foreach (var jsonInvalido in new[] { "{", "null", "{\"versao\":2,\"tema\":\"Claro\"}", "{\"versao\":1,\"tema\":\"Outro\"}",
        "{\"versao\":1,\"tema\":2}", new string('x', 17000) })
    {
        File.WriteAllText(preferencia, jsonInvalido);
        Igual(true, PreferenciasAparencia.Ler(preferencia) is { Tema: ModoTema.Escuro, Aviso: not null }, "Preferência inválida ou futura usa Escuro com aviso");
    }
    PreferenciasAparencia.Salvar(preferencia, ModoTema.Claro);
    File.SetAttributes(preferencia, FileAttributes.ReadOnly);
    try
    {
        await Falha<UnauthorizedAccessException>(() => Task.Run(() => PreferenciasAparencia.Salvar(preferencia, ModoTema.Escuro)),
            "Falha de gravação não substitui a preferência anterior");
        Igual(ModoTema.Claro, PreferenciasAparencia.Ler(preferencia).Tema, "Arquivo anterior permanece íntegro após falha");
    }
    finally { File.SetAttributes(preferencia, FileAttributes.Normal); }
    Igual(0, Directory.GetFiles(Path.GetDirectoryName(preferencia)!, "*.tmp").Length, "Gravação não deixa temporários após falha");
    Igual(true, PreferenciasAparencia.UsarEscuro(ModoTema.Escuro, false), "Escuro explícito prevalece sobre sistema claro");
    Igual(false, PreferenciasAparencia.UsarEscuro(ModoTema.Claro, true), "Claro explícito prevalece sobre sistema escuro");
    Igual(true, PreferenciasAparencia.UsarEscuro(ModoTema.Sistema, true), "Sistema resolve preferência escura");
    Igual(false, PreferenciasAparencia.UsarEscuro(ModoTema.Sistema, false), "Sistema resolve preferência clara");
    foreach (var paleta in new[] { PaletaTema.Escuro, PaletaTema.Claro })
    {
        double Luma(System.Drawing.Color cor)
        {
            double Linear(byte c) { var v=c/255.0; return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4); }
            return .2126*Linear(cor.R)+.7152*Linear(cor.G)+.0722*Linear(cor.B);
        }
        double Contraste(System.Drawing.Color a,System.Drawing.Color b) => (Math.Max(Luma(a),Luma(b))+.05)/(Math.Min(Luma(a),Luma(b))+.05);
        Igual(true, Contraste(paleta.Texto,paleta.Superficie)>=4.5 && Contraste(paleta.Secundario,paleta.Superficie)>=4.5 &&
            Contraste(paleta.Link,paleta.Superficie)>=4.5 && Contraste(paleta.NoDestaque,paleta.Destaque)>=4.5,
            "Contraste mínimo de texto, descrição, links e seleção na paleta " + (paleta==PaletaTema.Escuro?"Escuro":"Claro"));
        Igual(true, Contraste(paleta.Texto,paleta.NavegacaoAtiva)>=4.5 && Contraste(paleta.Destaque,paleta.NavegacaoAtiva)>=3 &&
            Contraste(paleta.Secundario,paleta.Fundo)>=4.5,
            "Navegação ativa e abas preservam contraste na paleta " + (paleta==PaletaTema.Escuro?"Escuro":"Claro"));
    }
    await Falha<ArgumentException>(() => ElevacaoConsulta.ConsultarAsync("diskhealth", "", default, tema:"Valor inválido"),
        "Tema administrativo fora da lista é recusado antes do UAC");
    var completo = Json("concluido", true);
    var evidenciaFiltro = new Campo("Código", "0xC0000005", Familia: "Excecoes");
    var respostaFiltro = new Resposta(1, "falhas", "parcial", [new Secao("Eventos", [evidenciaFiltro,
        new Campo("Texto Unicode", "Ação çã 漢字")], "Canal sem permissão")]);
    Igual(true, LinhasResultado.Filtrar(respostaFiltro, "  0xc0000005  ").Select(linha => linha.Campo.Valor)
        .SequenceEqual(["0xC0000005", "Canal sem permissão"]), "Filtro ignora caixa e preserva avisos fora da correspondência");
    Igual(3, LinhasResultado.Filtrar(respostaFiltro, "Eventos").Count(), "Busca pela seção inclui os campos e o aviso uma vez");
    Igual(true, ReferenceEquals(evidenciaFiltro, LinhasResultado.Filtrar(respostaFiltro, "Código").First().Campo),
        "Projeção mantém a evidência e família original do código");
    Igual(true, LinhasResultado.Filtrar(respostaFiltro, "inexistente").Single().Aviso,
        "Filtro sem correspondência conserva o aviso parcial");
    using (var unicode = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("çã漢字"))))
        Igual("çã漢字", await ConsultaPowerShell.LerLimitadoAsync(unicode, 4, default), "Leitor limitado conserva Unicode no limite exato");
    using (var excedente = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("12345"))))
        await Falha<InvalidDataException>(() => ConsultaPowerShell.LerLimitadoAsync(excedente, 4, default), "Leitor recusa saída acima do limite");
    using (var vazio = new StreamReader(new MemoryStream()))
        Igual("", await ConsultaPowerShell.LerLimitadoAsync(vazio, 0, default), "Leitor aceita saída vazia");
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
    await Falha<InvalidDataException>(() => Fixture("saida-excessiva.ps1", "[Console]::Error.Write(('x' * 70000)); Start-Sleep -Seconds 30")
        .ConsultarSistemaAsync(default), "Saída excessiva interrompe o processo sem esperar o prazo", "limite de tamanho");
    for (var repeticao = 1; repeticao <= 3; repeticao++) await VerificarArvore("cancelar", repeticao);
    await VerificarArvore("concluir", 1);
    await VerificarArvore("prazo", 1);

    async Task VerificarArvore(string modo, int repeticao)
    {
        var caminhoPai = Path.Combine(temporaria.FullName, $"pai-{modo}-{repeticao}.txt");
        var caminhoFilho = Path.Combine(temporaria.FullName, $"filho-{modo}-{repeticao}.txt");
        var programa = $$"""
            [IO.File]::WriteAllText('{{caminhoPai.Replace("'", "''")}}', [string]$PID)
            $filho = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') -ArgumentList '-NoLogo','-NoProfile','-NonInteractive','-Command','Start-Sleep -Seconds 30' -PassThru -WindowStyle Hidden
            [IO.File]::WriteAllText('{{caminhoFilho.Replace("'", "''")}}', [string]$filho.Id)
            {{(modo == "concluir" ? "@'\n" + completo + "\n'@" : "Start-Sleep -Seconds 30")}}
            """;
        using var token = new CancellationTokenSource();
        var tarefa = Fixture($"arvore-{modo}-{repeticao}.ps1", programa, TimeSpan.FromSeconds(5)).ConsultarSistemaAsync(token.Token);
        var processos = new List<Process>();
        try
        {
            var espera = Stopwatch.StartNew();
            while (!File.Exists(caminhoFilho) && !tarefa.IsCompleted && espera.Elapsed.TotalSeconds < 10) await Task.Delay(25);
            foreach (var caminho in new[] { caminhoPai, caminhoFilho })
            {
                if (!File.Exists(caminho)) throw new InvalidOperationException("A simulação não criou os processos esperados.");
                var pid = int.Parse(File.ReadAllText(caminho));
                try { processos.Add(Process.GetProcessById(pid)); }
                catch (ArgumentException) { /* Já encerrado pela consulta. */ }
            }
            if (modo == "cancelar") token.Cancel();
            if (modo == "concluir") await tarefa;
            else if (modo == "prazo") await Falha<TimeoutException>(() => tarefa, "Prazo encerra consulta com filho");
            else await Falha<OperationCanceledException>(() => tarefa, $"Cancelamento da árvore · repetição {repeticao}");
            espera.Restart();
            while (processos.Any(processo => !processo.HasExited) && espera.Elapsed.TotalSeconds < 3) await Task.Delay(25);
            Igual(true, processos.All(processo => processo.HasExited), $"Zero processos restantes · {modo} · {repeticao}");
        }
        finally
        {
            token.Cancel();
            try { await tarefa; } catch { }
            // A limpeza de segurança alcança somente os PIDs criados por esta simulação.
            foreach (var processo in processos)
            {
                if (!processo.HasExited) { processo.Kill(entireProcessTree: true); await processo.WaitForExitAsync(); }
                processo.Dispose();
            }
        }
    }

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
    Igual(conteudoLog, LogConsulta.Formatar("Velocidade", velocidade, agora, agora),
        "Cópia completa e log usam o mesmo resultado com avisos");
    var valorExtenso = "Diagnóstico çã 漢字 😀\r\nsegunda linha " + new string('x', 12000);
    var textoParcial = new Resposta(1, "discos", "parcial",
        [new Secao("Discos", [new Campo("Modelo", valorExtenso)], "SMART indisponível")]);
    var textoCopiavel = LogConsulta.Formatar("Discos", textoParcial, agora, agora);
    Igual(true, textoCopiavel.Contains(valorExtenso) && textoCopiavel.Contains("SMART indisponível") &&
        textoCopiavel.Contains("Resultado: parcial"), "Texto completo conserva Unicode, múltiplas linhas e valores longos");

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
