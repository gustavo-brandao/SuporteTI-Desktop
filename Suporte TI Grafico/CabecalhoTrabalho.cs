namespace SuporteTI.Grafico;

/// <summary>Contexto global da área, separado das guias e do formulário da operação.</summary>
internal sealed class CabecalhoTrabalho : Panel
{
    private readonly Label area = new() { AutoEllipsis = true, Font = new Font("Segoe UI", 15, FontStyle.Bold) };
    private readonly Label contexto = new() { AutoEllipsis = true, Font = new Font("Segoe UI", 9) };
    private readonly Label catalogo = new() { AutoSize = true, Text = $"{CatalogoFerramentas.Itens.Count} ferramentas", Font = new Font("Segoe UI", 9) };

    public CabecalhoTrabalho()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        Margin = Padding.Empty;
        AccessibleName = "Contexto da área de trabalho";
        Controls.Add(area); Controls.Add(contexto); Controls.Add(catalogo);
        Estilo.Marcar(contexto, PapelVisual.Secundario);
        Estilo.Marcar(catalogo, PapelVisual.Secundario);
        Definir("Visão geral", "Computador");
    }

    public void Definir(string grupo, string guia)
    {
        area.Text = grupo;
        contexto.Text = guia;
        area.AccessibleName = "Grupo atual: " + grupo;
        contexto.AccessibleName = "Área atual: " + guia;
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        int Px(int n) => (int)Math.Round(n * DeviceDpi / 96.0);
        Height = Px(72);
        MinimumSize = new Size(0, Px(72));
        catalogo.Visible = Width >= Px(560);
        catalogo.Location = new Point(Math.Max(Px(16), Width - catalogo.PreferredWidth - Px(8)), Px(29));
        var largura = Math.Max(Px(80), Width - Px(16) - (catalogo.Visible ? catalogo.PreferredWidth + Px(28) : 0));
        area.SetBounds(Px(4), Px(10), largura, Px(29));
        contexto.SetBounds(Px(4), Px(41), largura, Px(22));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var linha = new Pen(Estilo.Paleta.Borda);
        e.Graphics.DrawLine(linha, 0, Height - 1, Width, Height - 1);
    }
}
