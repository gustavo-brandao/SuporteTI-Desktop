using System.Reflection;
using System.Security.Principal;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal : Form
{
    private static readonly bool AltoContraste = SystemInformation.HighContrast;
    private static readonly Color Fundo = Color.FromArgb(12, 21, 44);
    private static readonly Color Navegacao = Color.FromArgb(20, 34, 71);
    private static readonly Color Superficie = Color.FromArgb(23, 38, 73);
    private static readonly Color Alternada = Color.FromArgb(29, 47, 86);
    private static readonly Color Borda = Color.FromArgb(57, 81, 131);
    private static readonly Color Destaque = Color.FromArgb(47, 88, 174);
    private static readonly Color Texto = Color.FromArgb(242, 246, 255);
    private static readonly Color Secundario = Color.FromArgb(182, 203, 241);
    private readonly ConsultaPowerShell consulta;
    private readonly Button consultar = new() { Text = "&Consultar computador", AutoSize = true };
    private readonly Button cancelar = new() { Text = "Ca&ncelar", AutoSize = true, Enabled = false };
    private readonly Button salvar = new() { Text = "Salvar &log…", AutoSize = true, Enabled = false };
    private readonly ComboBox diagnosticos = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, DropDownWidth = 440, AccessibleName = "Escolher diagnóstico" };
    private readonly TextBox filtro = new() { PlaceholderText = "Filtrar resultados…", Width = 175, AccessibleName = "Filtrar resultados" };
    private readonly TextBox entrada = new() { Width = 240, MaxLength = 512 };
    private readonly ComboBox volumesAdmin = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 75, AccessibleName = "Volume local para CHKDSK" };
    private readonly Label titulo = CriarRotulo("Informações do computador", 20, true);
    private readonly Label descricao = CriarRotulo("", 10, false);
    private readonly Label instrucoes = CriarRotulo("", 10, false);
    private Resposta? resultado;
    private DateTimeOffset inicioResultado, fimResultado;
    private string nomeResultado = "";
    private (Resposta Dados, string Nome, DateTimeOffset Inicio, DateTimeOffset Fim)? ultimoResultado;
    private string grupo = "";
    private readonly TabControl abas = new() { Dock = DockStyle.Fill, AccessibleName = "Áreas de ferramentas" };
    private readonly Panel conteudoResultados = new() { Dock = DockStyle.Fill };
    private readonly TableLayoutPanel painelTabela = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
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
        ClientSize = new Size(1000, 650);
        MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AltoContraste ? SystemColors.Window : Fundo;
        ForeColor = AltoContraste ? SystemColors.WindowText : Texto;

        var layout = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, Dock = DockStyle.Fill };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 206));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true,
            Padding = new Padding(16, 20, 16, 16),
            BackColor = AltoContraste ? SystemColors.Control : Navegacao
        };
        menu.Controls.Add(CriarRotulo("Suporte TI", 16, true));
        menu.Controls.Add(CriarRotulo("EDIÇÃO GRÁFICA", 9, false));
        var busca = new TextBox { Width = 168, PlaceholderText = "Buscar ferramenta…", AccessibleName = "Buscar ferramenta" };
        var encontrados = new ListBox { Width = 168, Height = 140, HorizontalScrollbar = true, Visible = false, AccessibleName = "Ferramentas encontradas" };
        var avisoBusca = CriarRotulo("Nenhuma ferramenta encontrada.", 9, false);
        avisoBusca.MaximumSize = new Size(168, 0);
        avisoBusca.Visible = false;
        menu.Controls.Add(busca);
        menu.Controls.Add(encontrados);
        menu.Controls.Add(avisoBusca);
        var botoes = new Dictionary<string, Button>();
        foreach (var nome in CatalogoFerramentas.Grupos)
        {
            var botao = new Button { Text = nome, Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
            botoes.Add(nome, botao);
            menu.Controls.Add(botao);
        }
        var sobre = new Button { Text = "&Sobre", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        menu.Controls.Add(sobre);
        menu.Controls.Add(CriarRotulo(versao, 9, false));
        layout.Controls.Add(menu, 0, 0);

        var painel = new Panel { Dock = DockStyle.Fill };
        var telaComputador = CriarTelaComputador();
        telaMonitor = CriarTelaMonitor();
        telaMapa = CriarTelaMapa();
        conteudoResultados.Controls.Add(telaMonitor);
        conteudoResultados.Controls.Add(telaMapa);
        var telaSobre = CriarTelaSobre(versao);
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
        };
        sobre.Click += (_, _) =>
        {
            if (NavegacaoBloqueada)
                return;
            ativa?.Cancel();
            abas.Visible = false;
            telaSobre.Visible = true;
            telaSobre.BringToFront();
            MarcarPagina(sobre, true);
            foreach (var botao in botoes.Values)
                MarcarPagina(botao, false);
        };
        void MostrarPagina(string nome)
        {
            if (NavegacaoBloqueada)
                return;
            telaSobre.Visible = false;
            abas.Visible = true;
            MarcarPagina(sobre, false);
            foreach (var (chave, botao) in botoes)
                MarcarPagina(botao, chave == nome);
            if (grupo == nome)
                return;
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
                abas.TabPages.Add(new TabPage(area) { BackColor = AltoContraste ? SystemColors.Window : Fundo });
            abas.SelectedIndex = 0;
            organizandoAbas = false;
            if (abas.SelectedTab is { } aba)
            {
                aba.Controls.Add(telaComputador);
                MostrarArea(aba.Text);
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
        void AbrirBusca()
        {
            if (encontrados.SelectedItem is not Ferramenta ferramenta || NavegacaoBloqueada)
                return;
            abrirFerramenta(ferramenta.Id);
            busca.Clear();
            diagnosticos.Focus();
        }
        busca.TextChanged += (_, _) =>
        {
            encontrados.Items.Clear();
            var procurando = !string.IsNullOrWhiteSpace(busca.Text);
            if (procurando)
                encontrados.Items.AddRange(CatalogoFerramentas.Buscar(busca.Text));
            encontrados.Visible = encontrados.Items.Count > 0;
            avisoBusca.Visible = procurando && encontrados.Items.Count == 0;
        };
        encontrados.DoubleClick += (_, _) => AbrirBusca();
        encontrados.KeyDown += (_, evento) =>
        {
            if (evento.KeyCode == Keys.Enter)
            {
                evento.SuppressKeyPress = true;
                AbrirBusca();
            }
        };
        busca.KeyDown += (_, evento) =>
        {
            if (evento.KeyCode == Keys.Down && encontrados.Items.Count > 0)
            {
                encontrados.SelectedIndex = 0;
                encontrados.Focus();
                evento.SuppressKeyPress = true;
            }
            if (evento.KeyCode == Keys.Enter && encontrados.Items.Count == 1)
            {
                encontrados.SelectedIndex = 0;
                AbrirBusca();
                evento.SuppressKeyPress = true;
            }
        };
        MostrarPagina("Visão geral");
        layout.Controls.Add(painel, 1, 0);

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
        consultar.Click += async (_, _) => await ExecutarConsultaAsync();
        entrada.KeyDown += async (_, evento) =>
        {
            if (evento.KeyCode == Keys.Enter)
            {
                evento.SuppressKeyPress = true;
                await ExecutarConsultaAsync();
            }
        };
        cancelar.Click += (_, _) => ativa?.Cancel();
        salvar.Click += (_, _) => SalvarResultado();
        FormClosing += (_, evento) =>
        {
            if (administrativaEmAndamento || preparandoIntervencao || intervencaoEmAndamento)
            {
                evento.Cancel = true;
                MessageBox.Show(this, "Aguarde a operação em andamento terminar.", "Operação em andamento",
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
        var cabecalho = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill };
        cabecalho.Controls.Add(titulo);
        cabecalho.Controls.Add(descricao);
        pagina.Controls.Add(cabecalho, 0, 0);
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
        var parametros = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
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
            var especial = id is "monitor" or "mapaocupacao";
            painelTabela.Visible = !especial;
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
                "velocidade" => "O teste usa até 12 MiB de dados com a Cloudflare para estimar a velocidade.",
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
        var comandos = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
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
        ConfigurarCopia();
        painelTabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        painelTabela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painelTabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        resumoDiscos.AccessibleName = "Resumo dos discos";
        resumoDiscos.Padding = new Padding(8);
        resumoDiscos.BackColor = AltoContraste ? SystemColors.Window : Superficie;
        resumoDiscos.Visible = false;
        painelTabela.Controls.Add(resumoDiscos, 0, 0);
        painelTabela.Controls.Add(grade, 0, 1);
        conteudoResultados.Controls.Add(painelTabela);
        pagina.Controls.Add(conteudoResultados, 0, 2);
        estado.Margin = new Padding(3, 8, 3, 0);
        AplicarCorBotao(salvar, false);
        var rodape = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        rodape.Controls.Add(salvar);
        rodape.Controls.Add(copiarResultado);
        rodape.Controls.Add(estado);
        rodape.Controls.Add(feedbackCopia);
        pagina.Controls.Add(rodape, 0, 3);
        return pagina;
    }

    private void MostrarArea(string area)
    {
        ativa?.Cancel();
        titulo.Text = grupo == "Visão geral" ? "Informações do computador" : grupo + " · " + area;
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
        diagnosticos.SelectedIndex = 0;
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
        grade.BorderStyle = BorderStyle.FixedSingle;
        grade.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grade.DefaultCellStyle.Padding = new Padding(6, 5, 6, 5);
        grade.RowTemplate.MinimumHeight = 44;
        grade.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        if (!AltoContraste)
        {
            grade.EnableHeadersVisualStyles = false;
            grade.GridColor = Borda;
            grade.DefaultCellStyle.BackColor = Superficie;
            grade.DefaultCellStyle.ForeColor = Texto;
            grade.DefaultCellStyle.SelectionBackColor = Destaque;
            grade.DefaultCellStyle.SelectionForeColor = Texto;
            grade.AlternatingRowsDefaultCellStyle.BackColor = Alternada;
            grade.ColumnHeadersDefaultCellStyle.BackColor = Navegacao;
            grade.ColumnHeadersDefaultCellStyle.ForeColor = Secundario;
            grade.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navegacao;
            grade.ColumnHeadersDefaultCellStyle.SelectionForeColor = Secundario;
        }
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
        var pagina = new TableLayoutPanel { ColumnCount = 1, Dock = DockStyle.Fill, Padding = new Padding(18) };
        pagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return pagina;
    }

    private static Label CriarRotulo(string texto, int tamanho, bool destaque) => new()
    {
        Text = texto, AutoSize = true, Margin = new Padding(3, 2, 3, 6),
        Font = new Font("Segoe UI", tamanho, destaque ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = AltoContraste ? SystemColors.WindowText : destaque ? Texto : Secundario
    };

    private static void MarcarPagina(Button botao, bool ativo)
    {
        if (AltoContraste)
        {
            botao.BackColor = ativo ? SystemColors.Highlight : SystemColors.Control;
            botao.ForeColor = ativo ? SystemColors.HighlightText : SystemColors.ControlText;
        }
        else
            AplicarCorBotao(botao, ativo);
    }

    private static void AplicarCorBotao(Button botao, bool destaque)
    {
        if (AltoContraste)
            return;
        botao.UseVisualStyleBackColor = false;
        botao.FlatStyle = FlatStyle.Flat;
        botao.FlatAppearance.BorderColor = Borda;
        botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(53, 94, 178);
        botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(37, 72, 147);
        botao.BackColor = destaque ? Destaque : Superficie;
        botao.ForeColor = Texto;
    }

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
        cancelar.Enabled = !administrativaEmAndamento && !intervencaoEmAndamento;
        estado.Text = administrativaEmAndamento ? "Aguardando autorização ou resultado do Windows…" : "Consultando o Windows…";
        try
        {
            Resposta resposta;
            if (administrativaEmAndamento)
            {
                var retorno = await ElevacaoConsulta.ConsultarAsync(escolhido.Id, volume, cancelamento.Token, dados);
                resposta = retorno.Resultado;
                if (retorno.Html is { } html)
                {
                    var salvo = await Task.Run(() => LogConsulta.SalvarHtmlComputador(parametro, html, DateTimeOffset.Now));
                    resposta = resposta with { Secoes = [.. resposta.Secoes, new Secao("Exportação", [new Campo("HTML do computador", salvo)], null)] };
                }
            }
            else
                resposta = await consulta.ConsultarAsync(escolhido.Id, cancelamento.Token, parametro, familia);
            cancelamento.Token.ThrowIfCancellationRequested();
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
            MessageBox.Show(this, salvo, "Log salvo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception erro) when (erro is IOException or UnauthorizedAccessException or ArgumentException)
        {
            MessageBox.Show(this, erro.Message, "Não foi possível salvar o log", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
