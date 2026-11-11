namespace SuporteTI.Grafico;

/// <summary>Navegação por assunto. Recolher não altera a página, a consulta ou seus parâmetros.</summary>
internal sealed class NavegacaoLateral : TableLayoutPanel
{
    private readonly Panel cabecalho = new() { Dock = DockStyle.Fill };
    private readonly Label marca = new() { Text = "Suporte TI", AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
    private readonly Label edicao = new() { Text = "VERSÃO DESKTOP", AutoSize = true, Font = new Font("Segoe UI", 8) };
    private readonly BotaoModerno alternar = new() { Text = "Alternar navegação", AccessibleName = "Alternar navegação", Simbolo = "\uE700", SomenteIcone = true };
    private readonly BotaoModerno abrirBusca = new() { Text = "Buscar ferramenta", AccessibleName = "Abrir busca de ferramentas", Simbolo = "\uE721", SomenteIcone = true };
    private readonly FlowLayoutPanel pesquisa = new() { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
    private readonly FlowLayoutPanel destinos = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
    private readonly FlowLayoutPanel utilidades = new() { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
    private readonly Label versao;
    private readonly ToolTip dicas = new();
    private bool? faixaCompacta;
    private bool compacta;
    public event EventHandler? LarguraAlterada;
    public TextBox Busca { get; } = new() { PlaceholderText = "Buscar ferramenta…", AccessibleName = "Buscar ferramenta" };
    public ListBox Encontrados { get; } = new() { Height = 140, HorizontalScrollbar = true, Visible = false, AccessibleName = "Ferramentas encontradas" };
    public Label AvisoBusca { get; } = new() { Text = "Nenhuma ferramenta encontrada.", AutoSize = true, Visible = false };
    public Dictionary<string, Button> Botoes { get; } = new();
    public BotaoModerno Sobre { get; } = new() { Text = "&Sobre", AccessibleName = "Sobre", Simbolo = "\uE946", TextAlign = ContentAlignment.MiddleLeft };
    public BotaoModerno Configuracoes { get; } = new() { Text = "Configurações", AccessibleName = "Configurações", Simbolo = "\uE713", TextAlign = ContentAlignment.MiddleLeft };
    public int LarguraPreferida => Px(compacta ? 80 : 240);

    public NavegacaoLateral(string numeroVersao, Func<string, string> icone)
    {
        Dock = DockStyle.Fill; ColumnCount = 1; RowCount = 4; Margin = Padding.Empty;
        AccessibleName = "Navegação principal"; cabecalho.AccessibleName = "Identidade e menu";
        pesquisa.AccessibleName = "Busca de ferramentas"; destinos.AccessibleName = "Grupos de ferramentas";
        utilidades.AccessibleName = "Sobre e configurações";
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(cabecalho, 0, 0); Controls.Add(pesquisa, 0, 1);
        Controls.Add(destinos, 0, 2); Controls.Add(utilidades, 0, 3);
        cabecalho.Margin = pesquisa.Margin = destinos.Margin = utilidades.Margin = Padding.Empty;
        cabecalho.Controls.Add(marca); cabecalho.Controls.Add(edicao); cabecalho.Controls.Add(alternar);
        pesquisa.Controls.Add(Busca); pesquisa.Controls.Add(Encontrados); pesquisa.Controls.Add(AvisoBusca); pesquisa.Controls.Add(abrirBusca);
        foreach (var nome in CatalogoFerramentas.Grupos)
        {
            var botao = new BotaoModerno { Text = nome, AccessibleName = nome, Simbolo = icone(nome), TextAlign = ContentAlignment.MiddleLeft };
            Botoes.Add(nome, botao); destinos.Controls.Add(botao); Estilo.Marcar(botao, PapelVisual.Navegacao);
        }
        utilidades.Controls.Add(Sobre); utilidades.Controls.Add(Configuracoes);
        versao = new Label { Text = numeroVersao, AutoSize = true };
        utilidades.Controls.Add(versao);
        foreach (var botao in Botoes.Values.Concat<Button>([Sobre, Configuracoes]))
            dicas.SetToolTip(botao, botao.AccessibleName);
        dicas.SetToolTip(abrirBusca, "Buscar ferramenta (Ctrl+K)");
        dicas.SetToolTip(Busca, "Buscar por nome ou assunto (Ctrl+K)");
        alternar.Click += (_, _) => DefinirCompacta(!compacta);
        abrirBusca.Click += (_, _) => FocarBusca();
        SizeChanged += (_, _) => Organizar();
        DpiChangedAfterParent += (_, _) => { Organizar(); LarguraAlterada?.Invoke(this, EventArgs.Empty); };
        Estilo.Marcar(this, PapelVisual.Navegacao);
        Estilo.Marcar(edicao, PapelVisual.Secundario); Estilo.Marcar(versao, PapelVisual.Secundario);
        Estilo.Marcar(AvisoBusca, PapelVisual.Secundario);
        foreach (var botao in new[] { alternar, abrirBusca, Sobre, Configuracoes }) Estilo.Marcar(botao, PapelVisual.Navegacao);
        Organizar();
    }

    public void Adaptar(int larguraEfetiva)
    {
        var novaFaixa = larguraEfetiva < 1000;
        // A escolha manual permanece enquanto a janela estiver na mesma faixa.
        if (faixaCompacta == novaFaixa) return;
        faixaCompacta = novaFaixa;
        DefinirCompacta(novaFaixa);
    }

    public void FocarBusca()
    {
        DefinirCompacta(false);
        Busca.Focus(); Busca.SelectAll();
    }

    private void DefinirCompacta(bool valor)
    {
        if (compacta == valor) return;
        if (valor && (Busca.ContainsFocus || Encontrados.ContainsFocus)) alternar.Focus();
        compacta = valor; Organizar(); LarguraAlterada?.Invoke(this, EventArgs.Empty);
    }

    private int Px(int n) => (int)Math.Round(n * DeviceDpi / 96.0);
    private void Organizar()
    {
        SuspendLayout(); pesquisa.SuspendLayout(); destinos.SuspendLayout(); utilidades.SuspendLayout();
        try
        {
            var largura = Math.Max(Px(44), Width - Px(24));
            marca.Visible = edicao.Visible = versao.Visible = !compacta;
            Busca.Visible = !compacta; Encontrados.Visible = !compacta && Encontrados.Items.Count > 0;
            AvisoBusca.Visible = !compacta && Busca.Text.Trim().Length > 0 && Encontrados.Items.Count == 0;
            abrirBusca.Visible = compacta;
            RowStyles[0].Height = Px(compacta ? 62 : 78);
            marca.Location = new Point(Px(16), Px(12)); edicao.Location = new Point(Px(18), Px(47));
            alternar.SetBounds(compacta ? Px(14) : Math.Max(Px(178), Width-Px(50)), Px(14), Px(36), Px(36));
            alternar.AccessibleDescription = compacta ? "Menu recolhido. Expandir para mostrar os nomes." : "Menu expandido. Recolher para ampliar o conteúdo.";
            dicas.SetToolTip(alternar, compacta ? "Expandir menu" : "Recolher menu");
            pesquisa.Padding = new Padding(Px(12), 0, Px(12), Px(12));
            destinos.Padding = new Padding(Px(compacta?8:12), Px(4), Px(compacta?8:12), 0);
            utilidades.Padding = new Padding(Px(12), Px(8), Px(12), Px(12));
            Busca.Width = Encontrados.Width = largura; AvisoBusca.MaximumSize = new Size(largura, 0);
            Busca.Margin = Encontrados.Margin = AvisoBusca.Margin = new Padding(0, 0, 0, Px(6));
            abrirBusca.Size = new Size(largura, Px(40)); abrirBusca.Margin = Padding.Empty;
            foreach (var botao in Botoes.Values.Concat<Button>([Sobre, Configuracoes]).OfType<BotaoModerno>())
            {
                botao.SomenteIcone = compacta;
                var larguraBotao = Botoes.Values.Contains(botao) ? Math.Max(Px(44), Width-destinos.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth) : largura;
                botao.Size = new Size(larguraBotao, Px(44)); botao.Margin = new Padding(0, 0, 0, Px(4));
            }
            versao.Margin = new Padding(Px(10), Px(8), 0, 0);
        }
        finally { utilidades.ResumeLayout(); destinos.ResumeLayout(); pesquisa.ResumeLayout(); ResumeLayout(); }
    }

    protected override void Dispose(bool disposing) { if (disposing) dicas.Dispose(); base.Dispose(disposing); }
}
