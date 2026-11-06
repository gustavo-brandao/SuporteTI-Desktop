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
    private readonly ComboBox diagnosticos = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210, AccessibleName = "Escolher diagnóstico" };
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
    private sealed record Diagnostico(string Id, string Nome)
    {
        public override string ToString() => Nome;
    }
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
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
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
        var computador = new Button { Text = "C&omputador", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var monitor = new Button { Text = "&Monitor ao vivo", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var mapa = new Button { Text = "Mapa de &disco", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var rede = new Button { Text = "&Rede", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var triagem = new Button { Text = "&Falhas do Windows", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var atendimento = new Button { Text = "&Atendimento", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var relatorios = new Button { Text = "Re&latórios", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var administrativo = new Button { Text = "Ad&ministrativo", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        var sobre = new Button { Text = "&Sobre", Width = 168, Height = 38, TextAlign = ContentAlignment.MiddleLeft };
        menu.Controls.Add(CriarRotulo("Suporte TI", 16, true));
        menu.Controls.Add(CriarRotulo("EDIÇÃO GRÁFICA", 9, false));
        if (!AltoContraste)
            menu.Controls.Add(new Panel { Width = 34, Height = 3, BackColor = Color.FromArgb(107, 166, 255), Margin = new Padding(3, 0, 3, 12) });
        menu.Controls.Add(computador);
        menu.Controls.Add(monitor);
        menu.Controls.Add(mapa);
        menu.Controls.Add(rede);
        menu.Controls.Add(triagem);
        menu.Controls.Add(atendimento);
        menu.Controls.Add(relatorios);
        menu.Controls.Add(administrativo);
        menu.Controls.Add(sobre);
        layout.Controls.Add(menu, 0, 0);

        var painel = new Panel { Dock = DockStyle.Fill };
        var telaComputador = CriarTelaComputador();
        var telaMonitor = CriarTelaMonitor();
        var telaMapa = CriarTelaMapa();
        var telaSobre = CriarTelaSobre(versao);
        var paginas = new (Button Botao, Control Tela, string? Grupo)[] {
            (computador, telaComputador, "Computador"), (monitor, telaMonitor, null), (mapa, telaMapa, null),
            (rede, telaComputador, "Rede"), (triagem, telaComputador, "Falhas do Windows"),
            (atendimento, telaComputador, "Atendimento"), (relatorios, telaComputador, "Relatórios"),
            (administrativo, telaComputador, "Administrativo"), (sobre, telaSobre, null)
        };
        foreach (var tela in paginas.Select(pagina => pagina.Tela).Distinct())
            painel.Controls.Add(tela);
        foreach (var pagina in paginas)
            pagina.Botao.Click += (_, _) => MostrarPagina(pagina);
        abrirTriagem = () => MostrarPagina((triagem, telaComputador, "Falhas do Windows"));
        void MostrarPagina((Button Botao, Control Tela, string? Grupo) ativa)
        {
            if ((administrativaEmAndamento || preparandoIntervencao || intervencaoEmAndamento) && ativa.Grupo != grupo)
                return;
            if (ativa.Grupo is not null)
                MostrarGrupo(ativa.Grupo);
            foreach (Control tela in painel.Controls)
                tela.Visible = tela == ativa.Tela;
            foreach (var pagina in paginas)
                MarcarPagina(pagina.Botao, pagina.Botao == ativa.Botao);
        }
        MostrarPagina(paginas[0]);
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
        volumesAdmin.Items.AddRange(DriveInfo.GetDrives().Where(disco => disco.DriveType == DriveType.Fixed)
            .Select(disco => disco.Name.TrimEnd('\\')).Cast<object>().ToArray());
        if (volumesAdmin.Items.Count > 0)
            volumesAdmin.SelectedIndex = 0;
        diagnosticos.SelectedIndexChanged += (_, _) =>
        {
            resultado = null;
            filtro.Clear();
            AtualizarGrade();
            var id = (diagnosticos.SelectedItem as Diagnostico)?.Id;
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
                "filaimpressao" => "Para agir, escolha Reiniciar spooler ou Limpar fila de impressão na lista acima.",
                _ when id is not null && Impactos.TryGetValue(id, out var impacto) => impacto,
                _ => ""
            };
            instrucoes.Visible = instrucoes.Text.Length > 0;
            estado.Text = "Pronto para consultar.";
        };
        filtro.TextChanged += (_, _) => { if (resultado is not null) AtualizarGrade(); };
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
        acoes.Controls.Add(diagnosticos);
        acoes.Controls.Add(entrada);
        acoes.Controls.Add(volumesAdmin);
        acoes.Controls.Add(consultar);
        acoes.Controls.Add(cancelar);
        acoes.Controls.Add(filtro);
        var comandos = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
        comandos.Controls.Add(acoes, 0, 0);
        comandos.Controls.Add(instrucoes, 0, 1);
        pagina.Controls.Add(comandos, 0, 1);

        EstilizarGrade(grade);
        grade.ConfigurarAtalho(() => AbrirCodigo(grade.CurrentRow));
        grade.CellContentClick += (_, evento) =>
        {
            if (evento.RowIndex >= 0 && evento.ColumnIndex == 2)
                AbrirCodigo(grade.Rows[evento.RowIndex]);
        };
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Seção", FillWeight = 25, MinimumWidth = 70 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Campo", FillWeight = 35, MinimumWidth = 85 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valor", FillWeight = 55, MinimumWidth = 110 });
        grade.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Gráfico", FillWeight = 25, MinimumWidth = 100, Visible = false });
        grade.CellPainting += DesenharIndicadorDisco;
        pagina.Controls.Add(grade, 0, 2);
        estado.Margin = new Padding(3, 8, 3, 0);
        AplicarCorBotao(salvar, false);
        var rodape = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        rodape.Controls.Add(salvar);
        rodape.Controls.Add(estado);
        pagina.Controls.Add(rodape, 0, 3);
        return pagina;
    }

    private void MostrarGrupo(string nome)
    {
        if (grupo == nome)
            return;
        ativa?.Cancel();
        grupo = nome;
        titulo.Text = nome == "Computador" ? "Informações do computador" : nome;
        descricao.Text = nome switch
        {
            "Rede" => "Consulte a configuração ou teste a conectividade.",
            "Falhas do Windows" => "Investigue eventos recentes do Windows ou consulte um código de erro.",
            "Atendimento" => "Diagnostique atualização, impressão e políticas do usuário.",
            "Relatórios" => "Revise resultados e exporte registros do Windows.",
            "Administrativo" => "O Windows pode pedir UAC para a consulta escolhida.",
            _ => "Escolha um diagnóstico local e filtre os resultados."
        };
        diagnosticos.Items.Clear();
        Diagnostico[] opcoes = nome switch
        {
            "Rede" => [new("rede", "Adaptadores e endereços"), new("dominio", "Domínio e grupo de trabalho"),
                new("ping", "Ping ICMP"), new("velocidade", "Velocidade da internet"),
                new("netdhcp", "Ativar IPv4 automático (DHCP)"), new("netstatic", "Configurar IPv4 fixo")],
            "Falhas do Windows" => [new("falhas", "Falhas recentes"), new("codigofalha", "Consultar código de erro")],
            "Atendimento" => [new("update", "Windows Update"), new("filaimpressao", "Diagnóstico de impressão"),
                new("spooler", "Reiniciar spooler"), new("spoolerclear", "Limpar fila de impressão"),
                new("cache", "Recriar cache do Windows Update"),
                new("gpusuario", "Políticas do usuário (GPO)"), new("gphtml", "Exportar GPO do usuário em HTML"),
                new("gpupdateuser", "Atualizar GPO do usuário"), new("gpupdatecomputer", "Atualizar GPO do computador")],
            "Relatórios" => [new("ultimo", "Último resultado"), new("eventos", "Eventos recentes"), new("evtx", "Exportar eventos em EVTX")],
            "Administrativo" => [new("diskhealth", "Saúde dos discos"), new("gpcomputador", "Políticas do computador (GPO)"),
                new("gpcomputerhtml", "Exportar GPO do computador"), new("sfcverify", "SFC · verificar arquivos"),
                new("dismcheck", "DISM · CheckHealth"), new("dismscan", "DISM · ScanHealth"), new("chkdskread", "CHKDSK · consultar volume"),
                new("sfcrepair", "SFC · reparar arquivos"), new("dismrestore", "DISM · reparar imagem"),
                new("chkdskfull", "CHKDSK · reparo completo"), new("perfillocal", "Revisar perfis locais duplicados"),
                new("autologonenable", "Configurar login automático"), new("autologondisable", "Desativar login automático")],
            _ => [new("sistema", "Resumo do sistema"), new("desempenho", "CPU, memória e processos"),
                new("discos", "Volumes e saúde dos discos"), new("bateria", "Saúde da bateria"),
                new("dispositivos", "Problemas de dispositivos"), new("servicos", "Serviços locais"),
                new("programas", "Programas instalados"), new("autologon", "Estado do login automático"),
                new("chromecache", "Limpar cache do Chrome")]
        };
        diagnosticos.Items.AddRange(opcoes);
        diagnosticos.SelectedIndex = 0;
    }

    private static void EstilizarGrade(DataGridView grade)
    {
        grade.BackgroundColor = AltoContraste ? SystemColors.Window : Superficie;
        grade.BorderStyle = BorderStyle.FixedSingle;
        grade.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
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
        Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, tamanho, destaque ? FontStyle.Bold : FontStyle.Regular),
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
        if (ativa is not null || preparandoIntervencao || diagnosticos.SelectedItem is not Diagnostico escolhido)
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
                entrada.Enabled = true;
                volumesAdmin.Enabled = true;
                cancelar.Enabled = false;
            }
        }
    }

    private void AtualizarGrade()
    {
        grade.Rows.Clear();
        grade.Columns[3].Visible = resultado?.Operacao is "discos" or "diskhealth";
        salvar.Enabled = resultado is not null;
        if (resultado is null)
            return;
        var termo = filtro.Text.Trim();
        foreach (var secao in resultado.Secoes)
        {
            foreach (var campo in secao.Campos)
                if (secao.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ||
                    campo.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ||
                    campo.Valor.Contains(termo, StringComparison.CurrentCultureIgnoreCase))
                {
                    var indicador = campo.Percentual is double percentual ? (object)percentual : "";
                    var linha = grade.Rows[grade.Rows.Add(secao.Nome, campo.Nome, campo.Valor, indicador)];
                    if (campo.Familia is "Win32" or "HRESULT" or "Excecoes" or "Paradas")
                    {
                        linha.Tag = campo;
                        var cor = AltoContraste ? SystemColors.HotTrack : Color.FromArgb(152, 204, 255);
                        linha.Cells[2] = new DataGridViewLinkCell
                        {
                            Value = campo.Valor, LinkColor = cor, ActiveLinkColor = cor, TrackVisitedState = false,
                            ToolTipText = "Consultar o significado deste código"
                        };
                    }
                }
            if (!string.IsNullOrWhiteSpace(secao.Aviso))
                grade.Rows.Add(secao.Nome, "Aviso", secao.Aviso);
        }
        grade.ClearSelection();
        estado.Text = resultado.Estado switch
        {
            "concluido" when grade.Rows.Count == 0 => "Nenhum resultado corresponde ao filtro.",
            "concluido" => "Consulta concluída.",
            "parcial" => "Consulta parcial. Confira os avisos na lista.",
            _ => "Dados indisponíveis. Confira os avisos na lista."
        };
    }

    private static void DesenharIndicadorDisco(object? remetente, DataGridViewCellPaintingEventArgs evento)
    {
        if (evento.RowIndex < 0 || evento.ColumnIndex != 3 || evento.Value is not double percentual ||
            evento.Graphics is null || evento.CellStyle is not { } estilo)
            return;
        evento.PaintBackground(evento.ClipBounds, true);
        var area = Rectangle.Inflate(evento.CellBounds, -8, -4);
        if (area.Width > 0 && area.Height > 0)
        {
            var larguraTexto = TextRenderer.MeasureText($"{percentual:N0}%", estilo.Font).Width;
            var trilha = new Rectangle(area.X, area.Y + (area.Height - 12) / 2, Math.Max(0, area.Width - larguraTexto - 4), 12);
            using var fundo = new SolidBrush(AltoContraste ? SystemColors.ControlDark : Borda);
            using var usado = new SolidBrush(AltoContraste ? SystemColors.Highlight :
                percentual >= 90 ? Color.FromArgb(224, 136, 116) : Color.FromArgb(98, 166, 255));
            evento.Graphics.FillRectangle(fundo, trilha);
            evento.Graphics.FillRectangle(usado, trilha.X, trilha.Y, (int)(trilha.Width * Math.Clamp(percentual, 0, 100) / 100), trilha.Height);
            var rotulo = new Rectangle(trilha.Right + 4, area.Y, larguraTexto, area.Height);
            var corTexto = (evento.State & DataGridViewElementStates.Selected) != 0
                ? estilo.SelectionForeColor : estilo.ForeColor;
            TextRenderer.DrawText(evento.Graphics, $"{percentual:N0}%", estilo.Font, rotulo, corTexto,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }
        evento.Handled = true;
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

    private Action? abrirTriagem;

    private bool AbrirCodigo(DataGridViewRow? linha)
    {
        if (linha?.Tag is not Campo codigo || ativa is not null)
            return false;
        if (codigo.Familia is not ("Win32" or "HRESULT" or "Excecoes" or "Paradas"))
            return false;
        abrirTriagem?.Invoke();
        diagnosticos.SelectedIndex = 1;
        entrada.Text = codigo.Valor;
        _ = ExecutarConsultaAsync(codigo.Familia);
        return true;
    }

    private sealed class GradeDiagnostico : DataGridView
    {
        private Func<bool>? abrirCodigo;

        public void ConfigurarAtalho(Func<bool> acao) => abrirCodigo = acao;

        protected override bool ProcessDialogKey(Keys keyData) =>
            (keyData == Keys.Enter && abrirCodigo?.Invoke() == true) || base.ProcessDialogKey(keyData);

        protected override bool ProcessDataGridViewKey(KeyEventArgs e) =>
            (e.KeyData == Keys.Enter && abrirCodigo?.Invoke() == true) || base.ProcessDataGridViewKey(e);
    }
}
