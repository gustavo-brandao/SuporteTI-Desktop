using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed class DialogoBusca : Form
{
    private readonly TextBox busca = new() { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 15),
        PlaceholderText = "Buscar ferramenta, assunto ou ação…", MaxLength = 160, AccessibleName = "Buscar ferramenta", Dock = DockStyle.Fill };
    private readonly ComboBox grupo = new SeletorModerno() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, AccessibleName = "Filtrar busca por grupo" };
    private readonly ListBox lista = new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, DrawMode = DrawMode.OwnerDrawFixed,
        IntegralHeight = false, AccessibleName = "Ferramentas encontradas" };
    private readonly Label quantidade = new() { AutoSize = true, Padding = new Padding(10, 7, 0, 0) };
    private readonly Label nome = new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold) };
    private readonly Label caminho = new() { AutoSize = true, Font = new Font("Segoe UI", 9) };
    private readonly Label permissao = new() { AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(3, 14, 3, 8) };
    private readonly Label resumo = new() { AutoSize = true, Font = new Font("Segoe UI", 10), Margin = new Padding(3, 3, 3, 12) };
    private readonly Label impacto = new() { AutoSize = true, Font = new Font("Segoe UI", 9) };
    private readonly Label vazio = new() { Text = "Nenhuma ferramenta encontrada.\nTente outro termo ou grupo.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
    private readonly FlowLayoutPanel previa = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
        AutoScroll = true, Padding = new Padding(18, 12, 12, 12), AccessibleName = "Prévia da ferramenta" };
    private readonly Panel resultados = new() { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 8, 0) };
    private readonly TableLayoutPanel corpo = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
    private readonly Button detalhes = new BotaoModerno { Text = "Ver detalhes", AutoSize = true };
    private readonly Button abrir = new BotaoModerno { Text = "Abrir formulário ↵", AutoSize = true, AccessibleName = "Abrir ferramenta selecionada" };
    private readonly Func<string, string?> obterImpacto;
    private readonly Func<string, string> icone;
    private bool atualizando, detalhesCompactos;
    public Ferramenta? Selecionada { get; private set; }

    public DialogoBusca(Func<string, string?> obterImpacto, Func<string, string> icone)
    {
        this.obterImpacto = obterImpacto; this.icone = icone;
        Text = "Buscar ferramentas"; AccessibleName = Text;
        Font = new Font("Segoe UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(880, 540); MinimumSize = new Size(640, 440);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false;
        KeyPreview = true; Padding = new Padding(18, 12, 18, 12);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        var entrada = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8, 10, 8, 10) };
        entrada.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34)); entrada.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        entrada.Controls.Add(new Label { Text = "\uE721", Font = new Font("Segoe MDL2 Assets", 17), Dock = DockStyle.Fill }, 0, 0);
        entrada.Controls.Add(busca, 1, 0);
        var filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 4, 0, 12) };
        grupo.Items.Add("Todos os grupos"); grupo.Items.AddRange(CatalogoFerramentas.Grupos.Cast<object>().ToArray()); grupo.SelectedIndex = 0;
        filtros.Controls.Add(grupo); filtros.Controls.Add(detalhes); filtros.Controls.Add(quantidade);
        corpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56)); corpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        corpo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        resultados.Controls.Add(lista); resultados.Controls.Add(vazio);
        previa.Controls.Add(caminho); previa.Controls.Add(nome); previa.Controls.Add(permissao); previa.Controls.Add(resumo); previa.Controls.Add(impacto);
        corpo.Controls.Add(resultados, 0, 0); corpo.Controls.Add(previa, 1, 0);
        var rodape = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 8, 0, 0) };
        rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rodape.Controls.Add(new Label { Text = "↑↓ selecionar · Enter preparar · Esc voltar/fechar", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        rodape.Controls.Add(abrir, 1, 0);
        layout.Controls.Add(entrada, 0, 0); layout.Controls.Add(filtros, 0, 1); layout.Controls.Add(corpo, 0, 2); layout.Controls.Add(rodape, 0, 3);
        Controls.Add(layout);
        Estilo.Marcar(abrir, PapelVisual.Primario);
        Estilo.Marcar(previa, PapelVisual.Superficie);
        foreach (var secundario in new[] { caminho, quantidade, permissao }) Estilo.Marcar(secundario, PapelVisual.Secundario);
        Estilo.Aplicar(this);
        lista.BackColor = Estilo.Paleta.Fundo;
        entrada.Paint += (_, e) => { using var linha = new Pen(Estilo.Paleta.Borda); e.Graphics.DrawLine(linha, 0, entrada.Height-1, entrada.Width, entrada.Height-1); };
        previa.Paint += (_, e) => { using var linha = new Pen(Estilo.Paleta.Borda); e.Graphics.DrawLine(linha, 0, 0, 0, previa.Height); };
        lista.DrawItem += DesenharResultado;
        lista.SelectedIndexChanged += (_, _) => { if (!atualizando) MostrarPrevia(); };
        lista.DoubleClick += (_, _) => Abrir(); abrir.Click += (_, _) => Abrir();
        busca.TextChanged += (_, _) => Atualizar(); grupo.SelectedIndexChanged += (_, _) => Atualizar();
        detalhes.Click += (_, _) => { detalhesCompactos = !detalhesCompactos; Organizar(); };
        SizeChanged += (_, _) => Organizar(); DpiChanged += (_, _) => Organizar();
        previa.SizeChanged += (_, _) =>
        {
            var largura = Math.Max(100, previa.ClientSize.Width - previa.Padding.Horizontal - 24);
            foreach (var label in previa.Controls.OfType<Label>()) label.MaximumSize = new Size(largura, 0);
        };
        Shown += (_, _) => { Atualizar(); busca.Focus(); };
        HandleCreated += (_, _) => { Estilo.Aplicar(this); lista.BackColor = Estilo.Paleta.Fundo; };
        Atualizar();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var area = Owner?.ClientSize ?? Screen.FromControl(this).WorkingArea.Size;
        Width = Math.Clamp(area.Width - 40 * DeviceDpi / 96, MinimumSize.Width, 920 * DeviceDpi / 96);
        Height = Math.Clamp(area.Height - 32 * DeviceDpi / 96, MinimumSize.Height, 620 * DeviceDpi / 96);
        var limites = Screen.FromControl(Owner ?? this).WorkingArea;
        var dono = Owner?.Bounds ?? limites;
        Location = new Point(
            Math.Clamp(dono.Left + (dono.Width - Width) / 2, limites.Left, Math.Max(limites.Left, limites.Right - Width)),
            Math.Clamp(dono.Top + (dono.Height - Height) / 2, limites.Top, Math.Max(limites.Top, limites.Bottom - Height)));
        Organizar();
    }

    private void Organizar()
    {
        var amplo = corpo.ClientSize.Width >= 740 * DeviceDpi / 96;
        corpo.SuspendLayout();
        resultados.Visible = amplo || !detalhesCompactos; previa.Visible = amplo || detalhesCompactos;
        corpo.ColumnStyles[0].Width = amplo ? 56 : detalhesCompactos ? 0 : 100;
        corpo.ColumnStyles[1].Width = amplo ? 44 : detalhesCompactos ? 100 : 0;
        detalhes.Visible = !amplo; detalhes.Text = detalhesCompactos ? "← Resultados" : "Ver detalhes";
        lista.ItemHeight = 62 * DeviceDpi / 96;
        corpo.ResumeLayout();
    }

    private void Atualizar()
    {
        var anterior = (lista.SelectedItem as Ferramenta)?.Id;
        var itens = CatalogoBusca.Buscar(busca.Text, grupo.SelectedIndex <= 0 ? null : grupo.Text);
        atualizando = true; lista.BeginUpdate();
        try
        {
            lista.Items.Clear(); lista.Items.AddRange(itens);
            lista.SelectedIndex = itens.Length == 0 ? -1 : Math.Max(0, Array.FindIndex(itens, f => f.Id == anterior));
        }
        finally { lista.EndUpdate(); atualizando = false; }
        quantidade.Text = $"{itens.Length} ferramenta{(itens.Length == 1 ? "" : "s")}";
        vazio.Visible = itens.Length == 0; lista.Visible = itens.Length > 0;
        MostrarPrevia();
    }

    private void MostrarPrevia()
    {
        var f = lista.SelectedItem as Ferramenta;
        abrir.Enabled = f is not null; detalhes.Enabled = f is not null;
        caminho.Text = f is null ? "" : f.Grupo + " / " + f.Area;
        nome.Text = f?.Nome ?? "Nenhuma correspondência";
        permissao.Text = f is null ? "" : ElevacaoConsulta.EhAdministrativa(f.Id) ? "UAC ao executar" : "Permissões da sessão";
        resumo.Text = f is null ? "Tente outro termo ou remova o filtro de grupo." : CatalogoBusca.Resumo(f);
        impacto.Text = f is null ? "" : obterImpacto(f.Id) is { } aviso ? "Antes de executar\n" + aviso : "Abrir somente prepara o formulário. A execução é uma ação separada.";
        impacto.ForeColor = f is not null && obterImpacto(f.Id) is not null ? Estilo.Paleta.Alerta : Estilo.Paleta.Secundario;
        lista.Invalidate();
    }

    private void Abrir()
    {
        if (lista.SelectedItem is not Ferramenta f) return;
        Selecionada = f; DialogResult = DialogResult.OK; Close();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            if (grupo.DroppedDown) { grupo.DroppedDown = false; return true; }
            if (detalhes.Visible && detalhesCompactos) { detalhesCompactos = false; Organizar(); busca.Focus(); }
            else { DialogResult = DialogResult.Cancel; Close(); }
            return true;
        }
        if (keyData == Keys.Enter) { if (grupo.DroppedDown) return base.ProcessCmdKey(ref msg, keyData); Abrir(); return true; }
        if (keyData is Keys.Down or Keys.Up or Keys.PageDown or Keys.PageUp)
        {
            if (grupo.ContainsFocus) return base.ProcessCmdKey(ref msg, keyData);
            if (lista.Items.Count > 0) lista.SelectedIndex = Math.Clamp(lista.SelectedIndex + (keyData == Keys.Down ? 1 : keyData == Keys.Up ? -1 : keyData == Keys.PageDown ? 5 : -5), 0, lista.Items.Count - 1);
            return true;
        }
        if (keyData == (Keys.Control | Keys.K)) { busca.Focus(); busca.SelectAll(); return true; }
        if (keyData == (Keys.Alt | Keys.Right) || keyData == (Keys.Alt | Keys.Left))
        { detalhesCompactos = keyData == (Keys.Alt | Keys.Right); Organizar(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void DesenharResultado(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || lista.Items[e.Index] is not Ferramenta f) return;
        var p = Estilo.Paleta; var selecionado = (e.State & DrawItemState.Selected) != 0;
        using var baseFundo = new SolidBrush(p.Fundo);
        e.Graphics.FillRectangle(baseFundo, e.Bounds);
        var area = Rectangle.Inflate(e.Bounds, -3, -3);
        using var forma = FormaModerna.Caminho(area, 7 * DeviceDpi / 96);
        using var fundo = new SolidBrush(selecionado ? Estilo.AltoContraste ? p.Destaque : p.NavegacaoAtiva : p.Fundo);
        e.Graphics.FillPath(fundo, forma);
        if (selecionado) { using var borda = new Pen(p.Destaque); e.Graphics.DrawPath(borda, forma); }
        var tinta = selecionado && Estilo.AltoContraste ? p.NoDestaque : p.Texto;
        int Px(int n) => n * DeviceDpi / 96;
        using var fonteIcone = new Font("Segoe MDL2 Assets", 16);
        var simbolo = icone(f.Grupo); if (simbolo == "win") simbolo = "\uE713";
        TextRenderer.DrawText(e.Graphics, simbolo, fonteIcone, new Rectangle(area.X + Px(10), area.Y, Px(30), area.Height), selecionado ? tinta : p.Secundario, TextFormatFlags.VerticalCenter);
        using var fonteNome = new Font("Segoe UI", 10, FontStyle.Bold);
        using var fonteCaminho = new Font("Segoe UI", 8);
        var x = area.X + Px(50); var largura = Math.Max(1, area.Right - x - Px(10));
        TextRenderer.DrawText(e.Graphics, f.Nome, fonteNome, new Rectangle(x, area.Y + Px(6), largura, Px(24)), tinta, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        var contexto = f.Grupo + " · " + f.Area + (ElevacaoConsulta.EhAdministrativa(f.Id) ? " · UAC" : "") + (obterImpacto(f.Id) is not null ? " · manutenção" : "");
        TextRenderer.DrawText(e.Graphics, contexto, fonteCaminho, new Rectangle(x, area.Y + Px(32), largura, Px(18)), selecionado && Estilo.AltoContraste ? p.NoDestaque : p.Secundario, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
    }
}
