namespace SuporteTI.Grafico;

/// <summary>Contexto global da área, separado das guias e do formulário da operação.</summary>
internal sealed class CabecalhoTrabalho : Panel
{
    private readonly Label area = new() { AutoEllipsis = true, Font = new Font("Segoe UI", 22, FontStyle.Bold) };
    private readonly Label contexto = new() { AutoEllipsis = true, Font = new Font("Segoe UI", 9) };
    private readonly Label caminho = new() { AutoEllipsis = true, Font = new Font("Segoe UI", 8) };
    private readonly Label catalogo = new() { AutoSize = true, Text = $"{CatalogoFerramentas.Itens.Count} ferramentas", Font = new Font("Segoe UI", 9) };

    public CabecalhoTrabalho()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        Margin = Padding.Empty;
        AccessibleName = "Contexto da área de trabalho";
        Controls.Add(area); Controls.Add(contexto); Controls.Add(catalogo); Controls.Add(caminho);
        Estilo.Marcar(contexto, PapelVisual.Secundario);
        Estilo.Marcar(catalogo, PapelVisual.Secundario);
        Estilo.Marcar(caminho, PapelVisual.Secundario);
        Definir("Visão geral", "Computador");
    }

    public void Definir(string grupo, string guia)
    {
        area.Text = grupo is "Sobre" or "Configurações" ? grupo : guia;
        contexto.Text = guia;
        caminho.Text = grupo + " / " + guia;
        caminho.AccessibleName = "Grupo atual: " + grupo + "; área atual: " + guia;
    }

    public void DefinirConteudo(string titulo, string descricao) { area.Text = titulo; contexto.Text = descricao; }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        int Px(int n) => (int)Math.Round(n * DeviceDpi / 96.0);
        Height = Px(96);
        MinimumSize = new Size(0, Px(96));
        catalogo.Visible = Width >= Px(560);
        catalogo.Location = new Point(Math.Max(Px(16), Width - catalogo.PreferredWidth - Px(8)), Px(36));
        var largura = Math.Max(Px(80), Width - Px(16) - (catalogo.Visible ? catalogo.PreferredWidth + Px(28) : 0));
        caminho.SetBounds(Px(4), Px(3), largura, Px(18));
        area.SetBounds(Px(4), Px(21), largura, Px(38));
        contexto.SetBounds(Px(4), Px(65), Math.Max(Px(80), Width-Px(16)), Px(24));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var linha = new Pen(Estilo.Paleta.Borda);
        e.Graphics.DrawLine(linha, 0, Height - 1, Width, Height - 1);
    }
}
