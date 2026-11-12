using System.Reflection;
using System.Security.Principal;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal : Form
{
    private static bool AltoContraste => Estilo.AltoContraste;
    private static Color Fundo => Estilo.Paleta.Fundo;
    private static Color Navegacao => Estilo.Paleta.Navegacao;
    private static Color Superficie => Estilo.Paleta.Superficie;
    private static Color Alternada => Estilo.Paleta.Alternada;
    private static Color Borda => Estilo.Paleta.Borda;
    private static Color Destaque => Estilo.Paleta.Destaque;
    private static Color Texto => Estilo.Paleta.Texto;
    private static Color Secundario => Estilo.Paleta.Secundario;
    private readonly ConsultaPowerShell consulta;
    private readonly Button consultar = new BotaoModerno() { Text = "&Consultar computador", AutoSize = true };
    private readonly Button cancelar = new BotaoModerno() { Text = "Ca&ncelar", AutoSize = true, Enabled = false };
    private readonly Button salvar = new BotaoModerno() { Text = "Salvar &log…", AutoSize = true, Enabled = false };
    private readonly ComboBox diagnosticos = new SeletorModerno() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, DropDownWidth = 440, AccessibleName = "Escolher diagnóstico" };
    private readonly TextBox filtro = new() { PlaceholderText = "Filtrar resultados…", Width = 175, AccessibleName = "Filtrar resultados" };
    private readonly TextBox entrada = new() { Width = 240, MaxLength = 512 };
    private readonly ComboBox volumesAdmin = new SeletorModerno() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 75, AccessibleName = "Volume local para CHKDSK" };
    private readonly Label titulo = CriarRotulo("Informações do computador", 18, true);
    private readonly CabecalhoTrabalho cabecalhoTrabalho = new();
    private readonly Label descricao = CriarRotulo("", 10, false);
    private readonly Label instrucoes = CriarRotulo("", 10, false);
    private Resposta? resultado;
    private DateTimeOffset inicioResultado, fimResultado;
    private string nomeResultado = "";
    private (Resposta Dados, string Nome, DateTimeOffset Inicio, DateTimeOffset Fim)? ultimoResultado;
    private string grupo = "";
    private readonly TabControl abas = new AbasModernas() { Dock = DockStyle.Fill, AccessibleName = "Áreas de ferramentas" };
    private readonly Panel conteudoResultados = new() { Dock = DockStyle.Fill };
    private readonly TableLayoutPanel painelTabela = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
    private readonly PainelVelocidade painelVelocidade = new() { Visible = false };
    private readonly TransicaoEntrada transicao = new();
    private readonly Label resumoDiscos = CriarRotulo("", 10, true);
    private Control? telaMonitor, telaMapa;
    private readonly Label estado = new() { AutoSize = true, Text = "Pronto para consultar." };
    private readonly GradeDiagnostico grade = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AccessibleName = "Resultados da consulta"
    };
    private CancellationTokenSource? ativa;
    private bool fecharAposConsulta;
    private bool administrativaEmAndamento;

    public JanelaPrincipal(ConsultaPowerShell consulta)
    {
        this.consulta = consulta;
        Font = new Font("Segoe UI", 10f);
        var versao = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion.Split('+')[0];
        Text = $"Suporte TI Gráfico | {versao}";
        ClientSize = new Size(1120, 740);
        MinimumSize = new Size(780, 560);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AltoContraste ? SystemColors.Window : Fundo;
        ForeColor = AltoContraste ? SystemColors.WindowText : Texto;

        var layout = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, Dock = DockStyle.Fill };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var menu = new NavegacaoLateral(versao, IconeMenu);
        var botoes = menu.Botoes; var sobre = menu.Sobre; var configuracoes = menu.Configuracoes;
        menu.LarguraAlterada += (_, _) => layout.ColumnStyles[0].Width = menu.LarguraPreferida;
        SizeChanged += (_, _) => menu.Adaptar(ClientSize.Width * 96 / DeviceDpi);
        DpiChanged += (_, _) => menu.Adaptar(ClientSize.Width * 96 / DeviceDpi);
        KeyPreview = true;
        KeyDown += (_, evento) =>
        {
            if (evento.Control && evento.KeyCode == Keys.K && !NavegacaoBloqueada)
            { menu.FocarBusca(); evento.SuppressKeyPress = true; }
        };
        layout.Controls.Add(menu, 0, 0);

        var areaTrabalho = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Padding = new Padding(16, 0, 20, 16), Margin = Padding.Empty };
        areaTrabalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        areaTrabalho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        areaTrabalho.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        areaTrabalho.Controls.Add(cabecalhoTrabalho, 0, 0);
        var painel = new Cartao { Dock = DockStyle.Fill, Padding = new Padding(8), Margin = new Padding(0, 12, 0, 0) };
        areaTrabalho.Controls.Add(painel, 0, 1);
        var telaComputador = CriarTelaComputador();
        telaMonitor = CriarTelaMonitor();
        telaMapa = CriarTelaMapa();
        conteudoResultados.Controls.Add(telaMonitor);
        conteudoResultados.Controls.Add(telaMapa);
        var telaSobre = CriarTelaSobre(versao);
        var telaAparencia = CriarTelaAparencia();
        telaAparencia.Visible = false;
        painel.Controls.Add(telaAparencia);
        configuracoes.Click += (_, _) =>
        {
            if (NavegacaoBloqueada) return;
            abas.Visible = telaSobre.Visible = false; telaAparencia.Visible = true; telaAparencia.BringToFront();
            cabecalhoTrabalho.Definir("Configurações", "Aparência e preferências");
            foreach (var item in botoes.Values) MarcarPagina(item, false);
            MarcarPagina(sobre, false); MarcarPagina(configuracoes, true);
            transicao.Iniciar(telaAparencia);
        };
        painel.Controls.Add(abas);
        painel.Controls.Add(telaSobre);
        abas.Selecting += (_, evento) =>
        {
            if (NavegacaoBloqueada)
                evento.Cancel = true;
        };
        var organizandoAbas = false;
        foreach (var (nome, botao) in botoes)
            botao.Click += (_, _) => MostrarPagina(nome);
        abas.SelectedIndexChanged += (_, _) =>
        {
            if (organizandoAbas || abas.SelectedTab is not { } aba)
                return;
            aba.Controls.Add(telaComputador);
            MostrarArea(aba.Text);
            transicao.Iniciar(telaComputador);
        };
        sobre.Click += (_, _) =>
        {
            if (NavegacaoBloqueada)
                return;
            ativa?.Cancel();
            telaAparencia.Visible = false; MarcarPagina(configuracoes, false);
            abas.Visible = false;
            telaSobre.Visible = true;
            telaSobre.BringToFront();
            cabecalhoTrabalho.Definir("Sobre", "Versão e créditos");
            MarcarPagina(sobre, true);
            foreach (var botao in botoes.Values)
                MarcarPagina(botao, false);
            transicao.Iniciar(telaSobre);
        };
        void MostrarPagina(string nome)
        {
            if (NavegacaoBloqueada)
                return;
            telaSobre.Visible = telaAparencia.Visible = false;
            MarcarPagina(configuracoes, false);
            abas.Visible = true;
            MarcarPagina(sobre, false);
            foreach (var (chave, botao) in botoes)
                MarcarPagina(botao, chave == nome);
            if (grupo == nome)
            {
                cabecalhoTrabalho.Definir(nome, abas.SelectedTab?.Text ?? "");
                cabecalhoTrabalho.DefinirConteudo(titulo.Text, descricao.Text);
                return;
            }
            ativa?.Cancel();
            grupo = nome;
            organizandoAbas = true;
            telaComputador.Parent?.Controls.Remove(telaComputador);
            foreach (var anterior in abas.TabPages.Cast<TabPage>().ToArray())
            {
                abas.TabPages.Remove(anterior);
                anterior.Dispose();
            }
            foreach (var area in CatalogoFerramentas.Itens.Where(item => item.Grupo == nome).Select(item => item.Area).Distinct())
                abas.TabPages.Add(new TabPage(area) { BackColor = AltoContraste ? SystemColors.Window : Superficie, ToolTipText = area });
            abas.SelectedIndex = 0;
            organizandoAbas = false;
            if (abas.SelectedTab is { } aba)
            {
                aba.Controls.Add(telaComputador);
                MostrarArea(aba.Text);
                transicao.Iniciar(telaComputador);
            }
        }
        abrirFerramenta = id =>
        {
            if (NavegacaoBloqueada)
                return;
            var ferramenta = CatalogoFerramentas.Itens.Single(item => item.Id == id);
            MostrarPagina(ferramenta.Grupo);
            abas.SelectedTab = abas.TabPages.Cast<TabPage>().Single(aba => aba.Text == ferramenta.Area);
            diagnosticos.SelectedItem = diagnosticos.Items.Cast<Ferramenta>().Single(item => item.Id == FluxoFerramentas.Principal(id));
            ConfigurarFluxo(id);
        };
        menu.BuscaSolicitada += (_, _) =>
        {
            if (NavegacaoBloqueada) return;
            var focoAnterior = ActiveControl;
            using var dialogo = new DialogoBusca(id => Impactos.GetValueOrDefault(id), IconeMenu) { Icon = Icon };
            if (dialogo.ShowDialog(this) == DialogResult.OK && dialogo.Selecionada is { } ferramenta)
            {
                abrirFerramenta(ferramenta.Id);
                diagnosticos.Focus();
            }
            else if (focoAnterior is { IsDisposed: false, Visible: true, Enabled: true }) focoAnterior.Focus();
        };
        MostrarPagina("Visão geral");
        Disposed += (_, _) => transicao.Dispose();
        layout.Controls.Add(areaTrabalho, 1, 0);

        using var identidade = WindowsIdentity.GetCurrent();
        var sessao = new WindowsPrincipal(identidade).IsInRole(WindowsBuiltInRole.Administrator)
            ? "administrador" : "padrão";
        var rodape = new StatusStrip { SizingGrip = false, Dock = DockStyle.Fill };
        if (!AltoContraste)
        {
            rodape.BackColor = Navegacao;
            rodape.ForeColor = Secundario;
        }
        rodape.Items.Add(new ToolStripStatusLabel($"{Environment.MachineName}  |  Sessão {sessao}  |  Uso local"));
        layout.Controls.Add(rodape, 0, 1);
        layout.SetColumnSpan(rodape, 2);
        Controls.Add(layout);
        menu.Adaptar(ClientSize.Width * 96 / DeviceDpi);
        IniciarAparencia();
        consultar.Click += async (_, _) => await ExecutarConsultaAsync();
        entrada.KeyDown += async (_, evento) =>
        {
            if (evento.KeyCode == Keys.Enter)
            {
                evento.SuppressKeyPress = true;
                await ExecutarConsultaAsync();
            }
        };
        cancelar.Click += (_, _) =>
        {
            if (administrativaEmAndamento && ferramentaAtual is { } f && ElevacaoConsulta.PermiteParar(f.Id))
            {
                if (f.Id == "sfcrepair" && !ConfirmarParadaReparo()) return;
                if (ativa is null) return; // A operação pode ter concluído durante a confirmação.
                cancelar.Enabled = false;
                estado.Text = "Parada solicitada · aguardando encerramento do SFC e limpeza…";
            }
            ativa?.Cancel();
        };
        salvar.Click += (_, _) => SalvarResultado();
        FormClosing += (_, evento) =>
        {
            if (administrativaEmAndamento || preparandoIntervencao || intervencaoEmAndamento)
            {
                evento.Cancel = true;
                DialogoTema.Mostrar(this, "Aguarde a operação em andamento terminar.", "Operação em andamento",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            varredura?.Cancel();
            if (ativa is null)
                return;
            evento.Cancel = true;
            fecharAposConsulta = true;
            ativa.Cancel();
        };
    }

    private TableLayoutPanel CriarTelaComputador()
    {
        var pagina = CriarPagina();
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var cabecalho = new FlowLayoutPanel { AutoSize = true, Visible = false, FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill };
        cabecalho.Controls.Add(titulo);
        cabecalho.Controls.Add(descricao);
        pagina.Controls.Add(cabecalho, 0, 0);
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
        var parametros = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        var comandos = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
        var rodape = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var campoFerramenta = CampoRotulado("Ferramenta", diagnosticos);
        var campoEntrada = CampoRotulado("Destino ou código", entrada);
        var campoVolume = CampoRotulado("Volume", volumesAdmin);
        var campoFiltro = CampoRotulado("Filtrar resultados", filtro);
        volumesAdmin.Items.AddRange(DriveInfo.GetDrives().Where(disco => disco.DriveType == DriveType.Fixed)
            .Select(disco => disco.Name.TrimEnd('\\')).Cast<object>().ToArray());
        if (volumesAdmin.Items.Count > 0)
            volumesAdmin.SelectedIndex = 0;
        atualizarFerramenta = () =>
        {
            resultado = null;
            filtro.Clear();
            AtualizarGrade();
            var id = ferramentaAtual?.Id;
            titulo.Text = id == "monitor" ? "Desempenho" : grupo == "Visão geral" ? "Informações do computador" : abas.SelectedTab?.Text ?? grupo;
            if (id == "monitor") descricao.Text = "Uso de CPU e memória atualizado a cada segundo.";
            else if (id == "velocidade") descricao.Text = "Download, upload e latência HTTPS até o servidor de teste.";
            else descricao.Text = grupo == "Hardware" ? "Consulte os componentes e os discos. Ações com UAC pedem autorização ao executar." : grupo == "Rede" ? "Consulte a configuração ou teste a conectividade." : descricao.Text;
            cabecalhoTrabalho.DefinirConteudo(titulo.Text, descricao.Text);
            comandos.Visible = id != "monitor";
            pagina.AutoScroll = true;
            var especial = id is "monitor" or "mapaocupacao";
            rodape.Visible = !especial;
            painelTabela.Visible = !especial;
            painelVelocidade.Visible = id == "velocidade";
            if (id == "velocidade") painelVelocidade.Limpar();
            if (telaMonitor is not null) telaMonitor.Visible = id == "monitor";
            if (telaMapa is not null) telaMapa.Visible = id == "mapaocupacao";
            consultar.Visible = !especial;
            cancelar.Visible = !especial && cancelar.Enabled;
            campoFiltro.Visible = !especial;
            salvar.Visible = copiarResultado.Visible = !especial && resultado is not null;
            estado.Visible = feedbackCopia.Visible = !especial;
            if (id == "monitor") telaMonitor?.BringToFront();
            else if (id == "mapaocupacao") telaMapa?.BringToFront();
            else painelTabela.BringToFront();
            consultar.Text = id switch
            {
                "sistema" => "&Consultar computador",
                "ping" => "Testar &ping",
                "velocidade" => "Testar &velocidade",
                "gphtml" => "Exportar &HTML…",
                "gpcomputerhtml" => "Exportar &HTML (UAC)…",
                "evtx" => "Exportar &EVTX…",
                "ultimo" => "&Rever último resultado",
                _ when id is not null && Impactos.ContainsKey(id) => "Revisar &ação…",
                _ => id is not null && ElevacaoConsulta.EhAdministrativa(id) ? "&Consultar (UAC)" : "&Consultar"
            };
            entrada.Clear();
            entrada.Visible = id is "ping" or "codigofalha";
            volumesAdmin.Visible = id is "chkdskread" or "chkdskfull";
            campoEntrada.Visible = id is "ping" or "codigofalha";
            campoVolume.Visible = id is "chkdskread" or "chkdskfull";
            ((Label)campoEntrada.Controls[0]).Text = id == "ping" ? "Destino" : "Código de erro";
            entrada.PlaceholderText = id == "ping" ? "Ex.: 127.0.0.1 ou exemplo.com" : "Ex.: 5 ou 0x80070005";
            entrada.AccessibleName = id == "ping" ? "Destino do ping" : "Código de erro";
            instrucoes.Text = id switch
            {
                "ping" => "Informe um host, endereço IP ou URL. Serão feitas quatro tentativas ICMP.",
                "velocidade" => "Cloudflare · cerca de 20–35 s · até 1 GiB de dados. Mede a vazão útil; Wi-Fi, VPN e outros acessos podem limitar o resultado.",
                "codigofalha" => "Informe o código decimal ou hexadecimal com prefixo 0x. A pesquisa identifica os significados possíveis.",
                "falhas" => "Clique em um código destacado para ver a explicação. No teclado, selecione a linha e pressione Enter.",
                "gphtml" => "Gera um relatório HTML das políticas do usuário desta sessão.",
                "evtx" => "Exporta Application, System e Setup das últimas 24 horas com manifesto SHA-256.",
                "ultimo" => "Recupera o último resultado disponível desta sessão.",
                "chkdskread" => "Escolha o volume. A verificação consulta o sistema de arquivos sem corrigir erros.",
                "sfcverify" => "Executa SFC /verifyonly para verificar arquivos protegidos.",
                "dismcheck" => "Consulta o estado de integridade registrado para a imagem do Windows.",
                "dismscan" => "Executa uma verificação da imagem do Windows com ScanHealth.",
                "gpcomputerhtml" => "Gera um HTML das políticas do computador na pasta que você escolher.",
                "filaimpressao" => "Em Ação, escolha diagnosticar, reiniciar o spooler ou limpar uma fila local. Alterações exigem revisão e UAC.",
                "diskhealth" => "Lê os indicadores dos discos com acesso administrativo. O Windows solicitará UAC ao consultar.",
                _ when id is not null && Impactos.TryGetValue(id, out var impacto) => impacto,
                _ => ""
            };
            instrucoes.Visible = instrucoes.Text.Length > 0;
            estado.Text = "Pronto para consultar.";
        };
        diagnosticos.SelectedIndexChanged += (_, _) => ConfigurarFluxo();
        filtro.TextChanged += (_, _) => AgendarFiltro();
        atrasoFiltro.Tick += (_, _) => AtualizarGrade();
        Disposed += (_, _) => atrasoFiltro.Dispose();
        if (!AltoContraste)
        {
            diagnosticos.BackColor = Superficie;
            diagnosticos.ForeColor = Texto;
            filtro.BackColor = Superficie;
            filtro.ForeColor = Texto;
            entrada.BackColor = Superficie;
            entrada.ForeColor = Texto;
            volumesAdmin.BackColor = Superficie;
            volumesAdmin.ForeColor = Texto;
        }
        AplicarCorBotao(consultar, true);
        AplicarCorBotao(cancelar, false);
        acoes.Controls.Add(campoFerramenta);
        consultar.Margin = cancelar.Margin = new Padding(3, 26, 8, 3);
        acoes.Controls.Add(consultar);
        acoes.Controls.Add(cancelar);
        cancelar.EnabledChanged += (_, _) => cancelar.Visible = cancelar.Enabled;
        CriarCamposFluxo(parametros, comandos);
        parametros.Controls.Add(campoEntrada);
        parametros.Controls.Add(campoVolume);
        parametros.Controls.Add(campoFiltro);
        comandos.Controls.Add(acoes, 0, 0);
        comandos.Controls.Add(parametros, 0, 1);
        comandos.Controls.Add(instrucoes, 0, 2);
        pagina.SizeChanged += (_, _) =>
        {
            var largura = Math.Max(180, pagina.ClientSize.Width - pagina.Padding.Horizontal - 12);
            titulo.MaximumSize = descricao.MaximumSize = instrucoes.MaximumSize = new Size(largura, 0);
            resumoDiscos.MaximumSize = new Size(largura - 8, 0);
            selecionadoFluxo.MaximumSize = new Size(largura - 8, 0);
        };
        pagina.Controls.Add(comandos, 0, 1);

        EstilizarGrade(grade);
        grade.ConfigurarAtalho(() => AbrirCodigo(grade.CurrentRow));
        grade.CellContentClick += (_, evento) =>
        {
            if (evento.RowIndex >= 0 && evento.ColumnIndex == 2)
                AbrirCodigo(grade.Rows[evento.RowIndex]);
            if (evento.RowIndex >= 0 && evento.ColumnIndex == 4)
                CopiarValor(grade.Rows[evento.RowIndex]);
        };
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Seção", FillWeight = 25, MinimumWidth = 70 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Campo", FillWeight = 35, MinimumWidth = 85 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valor", FillWeight = 55, MinimumWidth = 110 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Indicador", FillWeight = 30, MinimumWidth = 145, Visible = false });
        grade.CellPainting += DesenharIndicadorDisco;
        grade.RowStateChanged += (_, evento) =>
        {
            if ((evento.StateChanged & DataGridViewElementStates.Selected) == 0) return;
            foreach (var link in evento.Row.Cells.OfType<DataGridViewLinkCell>())
                link.LinkColor = link.ActiveLinkColor = link.VisitedLinkColor = evento.Row.Selected ? Estilo.Paleta.NoDestaque : Estilo.Paleta.Link;
        };
        ConfigurarCopia();
        painelTabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        painelTabela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painelTabela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painelTabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        resumoDiscos.AccessibleName = "Resumo dos discos";
        resumoDiscos.Padding = new Padding(8);
        Estilo.Marcar(resumoDiscos, PapelVisual.Superficie);
        resumoDiscos.Visible = false;
        painelTabela.Controls.Add(painelVelocidade, 0, 0);
        painelTabela.Controls.Add(resumoDiscos, 0, 1);
        painelTabela.Controls.Add(grade, 0, 2);
        conteudoResultados.Controls.Add(painelTabela);
        conteudoResultados.SizeChanged += (_, _) =>
        {
            // Reservar espaço para os resultados, inclusive na altura mínima da janela.
            var altura = painelVelocidade.AlturaPreferida(conteudoResultados.ClientSize.Width);
            painelVelocidade.MinimumSize = new Size(0, altura);
            painelVelocidade.Height = altura;
        };
        pagina.Controls.Add(conteudoResultados, 0, 2);
        estado.Margin = new Padding(3, 8, 3, 0);
        AplicarCorBotao(salvar, false);
        rodape.Controls.Add(salvar);
        rodape.Controls.Add(copiarResultado);
        rodape.Controls.Add(estado);
        rodape.Controls.Add(feedbackCopia);
        pagina.Controls.Add(rodape, 0, 3);
        var ajustandoAltura = false;
        pagina.Layout += (_, _) =>
        {
            if (ajustandoAltura) return;
            ajustandoAltura = true;
            try
            {
                // Reservar uma área legível; a janela baixa rola em vez de esmagar a tabela.
                var minimoResultados = (ferramentaAtual?.Id is "monitor" or "mapaocupacao" or "velocidade" ? 360 : 180) * pagina.DeviceDpi / 96;
                if (ferramentaAtual?.Id == "monitor" && telaMonitor is not null)
                    minimoResultados = Math.Max(minimoResultados, telaMonitor.MinimumSize.Height);
                if (ferramentaAtual?.Id == "velocidade")
                    minimoResultados = Math.Max(minimoResultados, painelVelocidade.MinimumSize.Height + 140 * pagina.DeviceDpi / 96);
                var minimo = pagina.Padding.Vertical + (cabecalho.Visible ? cabecalho.Height + cabecalho.Margin.Vertical : 0) +
                    (comandos.Visible ? comandos.Height + comandos.Margin.Vertical : 0) + (rodape.Visible ? rodape.Height + rodape.Margin.Vertical : 0) +
                    conteudoResultados.Margin.Vertical + minimoResultados;
                if (pagina.AutoScrollMinSize.Height != minimo) pagina.AutoScrollMinSize = new Size(0, minimo);
            }
            finally { ajustandoAltura = false; }
        };
        pagina.AutoScroll = true;
        return pagina;
    }

    private void MostrarArea(string area)
    {
        ativa?.Cancel();
        cabecalhoTrabalho.Definir(grupo, area);
        titulo.Text = grupo == "Visão geral" ? "Informações do computador" : area;
        descricao.Text = grupo switch
        {
            "Hardware" => "Consulte os componentes e os discos. Ações com UAC pedem autorização ao executar.",
            "Rede" => "Consulte a configuração ou teste a conectividade.",
            "Windows" => "Investigue eventos, atualizações e a integridade do Windows.",
            "Impressão" => "Diagnostique as filas ou revise uma ação de impressão.",
            "Contas e políticas" => "Consulte políticas, revise perfis ou configure o login automático.",
            "Relatórios" => "Revise resultados e exporte registros do Windows.",
            _ => "Consulte o computador e use a busca para encontrar uma ferramenta."
        };
        diagnosticos.Items.Clear();
        diagnosticos.Items.AddRange(FluxoFerramentas.Listar(grupo, area));
        diagnosticos.SelectedItem = diagnosticos.Items.Cast<Ferramenta>().FirstOrDefault(f => f.Id == "monitor") ?? diagnosticos.Items[0];
    }

    private bool NavegacaoBloqueada => administrativaEmAndamento || preparandoIntervencao || intervencaoEmAndamento;

    private static FlowLayoutPanel CampoRotulado(string nome, Control campo)
    {
        var grupo = new FlowLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown, WrapContents = false,
            Margin = new Padding(0, 0, 12, 4)
        };
        grupo.Controls.Add(CriarRotulo(nome, 9, false));
        grupo.Controls.Add(campo);
        return grupo;
    }
    private static void EstilizarGrade(DataGridView grade)
    {
        grade.BackgroundColor = AltoContraste ? SystemColors.Window : Superficie;
        grade.BorderStyle = BorderStyle.None;
        grade.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grade.DefaultCellStyle.Padding = new Padding(6, 5, 6, 5);
        grade.RowTemplate.MinimumHeight = 38;
        grade.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        Estilo.Aplicar(grade);
    }

    private TableLayoutPanel CriarTelaSobre(string versao)
    {
        var pagina = CriarPagina();
        pagina.Controls.Add(CriarRotulo("Sobre o Suporte TI", 20, true));
        pagina.Controls.Add(CriarRotulo($"Suporte TI Gráfico · versão {versao}", 12, true));
        pagina.Controls.Add(CriarRotulo("Desenvolvido por Gustavo Brandão Gimenez.", 10, false));
        pagina.Controls.Add(CriarRotulo("Prévia portátil da edição pública para Windows.", 10, false));
        pagina.Controls.Add(CriarRotulo("Referência funcional: Suporte TI para terminal 0.1.22.", 10, false));
        return pagina;
    }

    private static TableLayoutPanel CriarPagina()
    {
        var pagina = new TableLayoutPanel { ColumnCount = 1, Dock = DockStyle.Fill, Padding = new Padding(16) };
        pagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return pagina;
    }

    private static Label CriarRotulo(string texto, int tamanho, bool destaque)
    {
        var rotulo=new Label {Text=texto, AutoSize=true, Margin=new Padding(3,2,3,6),
            Font=new Font("Segoe UI",tamanho,destaque?FontStyle.Bold:FontStyle.Regular)};
        Estilo.Marcar(rotulo,destaque?PapelVisual.Normal:PapelVisual.Secundario);
        return rotulo;
    }
    private static void MarcarPagina(Button botao, bool ativo)
    {
        var anterior = botao.BackColor;
        Estilo.Marcar(botao, ativo ? PapelVisual.Ativo : PapelVisual.Navegacao);
        if (botao is BotaoModerno moderno) moderno.AnimarSelecao(anterior);
    }
    private static void AplicarCorBotao(Button botao, bool destaque) => Estilo.Marcar(botao, destaque ? PapelVisual.Primario : PapelVisual.Normal);
    private static string IconeMenu(string nome) => nome switch
    {
        "Visão geral" => "\uE80F", "Hardware" => "\uE770", "Rede" => "\uE839", "Windows" => "win",
        "Impressão" => "\uE749", "Contas e políticas" => "\uE77B", _ => "\uE9F9"
    };
    private async Task ExecutarConsultaAsync(string familia = "")
    {
        if (ativa is not null || preparandoIntervencao || ferramentaAtual is not Ferramenta escolhido)
            return;
        if (escolhido.Id is "monitor" or "mapaocupacao")
            return;
        if (escolhido.Id == "ultimo")
        {
            if (ultimoResultado is { } ultimo)
            {
                (resultado, nomeResultado, inicioResultado, fimResultado) = ultimo;
                AtualizarGrade();
                estado.Text = $"{nomeResultado} · {fimResultado:g}";
            }
            else
                estado.Text = "Nenhum resultado nesta sessão.";
            return;
        }
        if (escolhido.Id is "ping" or "codigofalha" && string.IsNullOrWhiteSpace(entrada.Text))
        {
            estado.Text = escolhido.Id == "ping" ? "Informe o destino do ping." : "Informe o código de erro.";
            entrada.Focus();
            return;
        }
        var parametro = entrada.Text.Trim();
        if (escolhido.Id is "gphtml" or "evtx" or "gpcomputerhtml")
        {
            var pasta = EscolherPasta("Escolha onde salvar os arquivos da exportação.");
            if (pasta is null)
                return;
            parametro = pasta;
        }
        var volume = volumesAdmin.SelectedItem as string ?? "";
        if (escolhido.Id is "chkdskread" or "chkdskfull" && volume.Length == 0)
        {
            estado.Text = "Nenhum volume fixo local disponível.";
            return;
        }
        var intervencao = Impactos.ContainsKey(escolhido.Id);
        ParametrosIntervencao? dados = null;
        if (intervencao)
        {
            var preparacao = await PrepararIntervencaoAsync(escolhido, volume);
            if (!preparacao.Confirmada)
                return;
            dados = preparacao.Dados;
        }
        using var cancelamento = new CancellationTokenSource();
        var grupoInicial = grupo;
        var inicio = DateTimeOffset.Now;
        ativa = cancelamento;
        administrativaEmAndamento = ElevacaoConsulta.EhAdministrativa(escolhido.Id);
        intervencaoEmAndamento = intervencao;
        resultado = null;
        AtualizarGrade();
        diagnosticos.Enabled = false;
        HabilitarCamposFluxo(false);
        entrada.Enabled = false;
        consultar.Enabled = false;
        volumesAdmin.Enabled = false;
        cancelar.Text = escolhido.Id == "sfcrepair" ? "Parar SFC…" : ElevacaoConsulta.PermiteParar(escolhido.Id) ? "Parar verificação" : "Cancelar";
        cancelar.Enabled = ElevacaoConsulta.PermiteParar(escolhido.Id) || (!administrativaEmAndamento && !intervencaoEmAndamento);
        estado.Text = administrativaEmAndamento ? ElevacaoConsulta.PermiteParar(escolhido.Id)
            ? escolhido.Id == "sfcrepair" ? "Reparo SFC · aguarde o resultado; parar exige confirmação e deixa o reparo incompleto."
                : "Aguardando autorização ou resultado · parar encerra apenas esta verificação, sem reparo."
            : "Aguardando autorização ou resultado · esta operação não permite parada pelo aplicativo."
            : "Consultando o Windows…";
        if (escolhido.Id == "velocidade") painelVelocidade.Iniciar();
        try
        {
            Resposta resposta;
            if (escolhido.Id == "velocidade")
            {
                var progresso = new Progress<ProgressoVelocidade>(p =>
                {
                    if (IsDisposed || cancelamento.IsCancellationRequested || ativa != cancelamento || resultado is not null) return;
                    painelVelocidade.Atualizar(p);
                    estado.Text = $"{p.Etapa} · {p.Segundos:N0} s · medindo a conexão…";
                });
                resposta = await TesteVelocidade.ExecutarAsync(cancelamento.Token, progresso);
                painelVelocidade.Concluir(resposta.Estado == "concluido" ? "Teste concluído" : "Teste concluído com avisos", resposta);
            }
            else if (administrativaEmAndamento)
            {
                var retorno = await ElevacaoConsulta.ConsultarAsync(escolhido.Id, volume, cancelamento.Token, dados, Estilo.Escuro ? "Escuro" : "Claro");
                resposta = retorno.Resultado;
                if (retorno.Html is { } html)
                {
                    var salvo = await Task.Run(() => LogConsulta.SalvarHtmlComputador(parametro, html, DateTimeOffset.Now));
                    resposta = resposta with { Secoes = [.. resposta.Secoes, new Secao("Exportação", [new Campo("HTML do computador", salvo)], null)] };
                }
            }
            else
                resposta = await consulta.ConsultarAsync(escolhido.Id, cancelamento.Token, parametro, familia);
            if (!ElevacaoConsulta.PermiteParar(escolhido.Id)) cancelamento.Token.ThrowIfCancellationRequested();
            if (IsDisposed)
                return;
            resultado = resposta;
            nomeResultado = escolhido.Nome;
            inicioResultado = inicio;
            fimResultado = DateTimeOffset.Now;
            ultimoResultado = (resposta, nomeResultado, inicioResultado, fimResultado);
            AtualizarGrade();
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed && grupo == grupoInicial)
                estado.Text = administrativaEmAndamento ? "Solicitação de UAC cancelada." : "Consulta cancelada.";
        }
        catch (Exception erro)
        {
            if (!IsDisposed && grupo == grupoInicial && !cancelamento.IsCancellationRequested)
            {
                resultado = new Resposta(1, escolhido.Id, "indisponivel", [new Secao("Consulta", [], erro.Message)]);
                if (escolhido.Id == "velocidade") painelVelocidade.Concluir("Não foi possível concluir o teste", resultado);
                nomeResultado = escolhido.Nome;
                inicioResultado = inicio;
                fimResultado = DateTimeOffset.Now;
                ultimoResultado = (resultado, nomeResultado, inicioResultado, fimResultado);
                AtualizarGrade();
                estado.Text = erro is TimeoutException ? "Limite de tempo atingido." : "A consulta falhou.";
            }
        }
        finally
        {
            if (escolhido.Id == "velocidade" && !IsDisposed && (resultado is null || cancelamento.IsCancellationRequested))
                painelVelocidade.Concluir(cancelamento.IsCancellationRequested ? "Teste cancelado" : "Não foi possível concluir o teste");
            ativa = null;
            administrativaEmAndamento = false;
            intervencaoEmAndamento = false;
            if (fecharAposConsulta)
                Close();
            else if (!IsDisposed)
            {
                consultar.Enabled = true;
                diagnosticos.Enabled = true;
                HabilitarCamposFluxo(true);
                entrada.Enabled = true;
                volumesAdmin.Enabled = true;
                cancelar.Enabled = false;
            }
        }
    }

    private void SalvarResultado()
    {
        if (resultado is null)
            return;
        var pasta = EscolherPasta("Escolha a pasta para salvar o log. O resultado inclui todos os dados da consulta.");
        if (pasta is null)
            return;
        try
        {
            var salvo = LogConsulta.Salvar(pasta, nomeResultado, resultado, inicioResultado, fimResultado);
            DialogoTema.Mostrar(this, salvo, "Log salvo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception erro) when (erro is IOException or UnauthorizedAccessException or ArgumentException)
        {
            DialogoTema.Mostrar(this, erro.Message, "Não foi possível salvar o log", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string? EscolherPasta(string descricao)
    {
        var documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var sugerido = string.IsNullOrEmpty(documentos) ? "" : Path.Combine(documentos, "SuporteTI", "Resultados");
        using var dialogo = new FolderBrowserDialog
        {
            Description = descricao,
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(sugerido) ? sugerido : documentos
        };
        return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.SelectedPath : null;
    }

    private Action<string>? abrirFerramenta;

    private bool AbrirCodigo(DataGridViewRow? linha)
    {
        if (linha?.Tag is not Campo codigo || ativa is not null)
            return false;
        if (codigo.Familia is not ("Win32" or "HRESULT" or "Excecoes" or "Paradas"))
            return false;
        abrirFerramenta?.Invoke("codigofalha");
        entrada.Text = codigo.Valor;
        _ = ExecutarConsultaAsync(codigo.Familia);
        return true;
    }

    private sealed class GradeDiagnostico : DataGridView
    {
        public GradeDiagnostico() => DoubleBuffered = true;
        private Func<bool>? abrirCodigo;
        private Action? copiar;

        public void ConfigurarCopia(Action acao) => copiar = acao;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C) && copiar is not null)
            {
                copiar();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ConfigurarAtalho(Func<bool> acao) => abrirCodigo = acao;

        protected override bool ProcessDialogKey(Keys keyData) =>
            (keyData == Keys.Enter && abrirCodigo?.Invoke() == true) || base.ProcessDialogKey(keyData);

        protected override bool ProcessDataGridViewKey(KeyEventArgs e) =>
            (e.KeyData == Keys.Enter && abrirCodigo?.Invoke() == true) || base.ProcessDataGridViewKey(e);
    }
}
