using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal static class FormaModerna
{
    public static GraphicsPath Caminho(Rectangle area, int raio)
    {
        var p = new GraphicsPath();
        var d = Math.Min(raio * 2, Math.Min(area.Width, area.Height));
        if (d <= 0) return p;
        p.AddArc(area.X, area.Y, d, d, 180, 90);
        p.AddArc(area.Right-d, area.Y, d, d, 270, 90);
        p.AddArc(area.Right-d, area.Bottom-d, d, d, 0, 90);
        p.AddArc(area.X, area.Bottom-d, d, d, 90, 90); p.CloseFigure();
        return p;
    }
}

internal class BotaoModerno : Button
{
    private bool hover, pressionado;
    private readonly System.Windows.Forms.Timer movimento = new() { Interval = 16 };
    private readonly System.Diagnostics.Stopwatch relogio = new();
    private Color origem;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string? Simbolo { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool SomenteIcone { get; set; }
    public BotaoModerno()
    {
        DoubleBuffered = true; Padding = new Padding(10, 4, 10, 4); MinimumSize = new Size(0, 34);
        movimento.Tick += (_, _) => { if (relogio.ElapsedMilliseconds >= 160 || !Visible || !Movimento.Permitido) movimento.Stop(); Invalidate(); };
    }
    public void AnimarSelecao(Color anterior)
    {
        movimento.Stop();
        if (anterior == BackColor || !Visible || !Movimento.Permitido) { Invalidate(); return; }
        origem = anterior; relogio.Restart(); movimento.Start(); Invalidate();
    }
    protected override void OnMouseEnter(EventArgs e) { hover=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover=pressionado=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressionado=true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressionado=false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var p=Estilo.Paleta;
        e.Graphics.Clear(Parent?.BackColor ?? p.Fundo);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        var area=Rectangle.Inflate(ClientRectangle,-1,-1);
        using var caminho=FormaModerna.Caminho(area, Math.Max(3,6*DeviceDpi/96));
        var cor=BackColor;
        if (movimento.Enabled && Movimento.Permitido)
        {
            var f = Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 160);
            cor = Color.FromArgb((int)(origem.R + (cor.R - origem.R) * f), (int)(origem.G + (cor.G - origem.G) * f), (int)(origem.B + (cor.B - origem.B) * f));
        }
        if (hover && Enabled && !Estilo.AltoContraste) cor=ControlPaint.Light(cor, pressionado ? .04f : .08f);
        // A cor do texto acompanha o contraste do fundo intermediário durante o realce.
        static double Canal(byte n) { var s = n / 255.0; return s <= .04045 ? s / 12.92 : Math.Pow((s + .055) / 1.055, 2.4); }
        var luminancia = .2126 * Canal(cor.R) + .7152 * Canal(cor.G) + .0722 * Canal(cor.B);
        var corTexto = movimento.Enabled && Movimento.Permitido ? luminancia > .179 ? Color.Black : Color.White : ForeColor;
        using var fundo=new SolidBrush(cor); e.Graphics.FillPath(fundo,caminho);
        if (Estilo.PapelDe(this) is not (PapelVisual.Navegacao or PapelVisual.Ativo))
        { using var borda=new Pen(p.Borda); e.Graphics.DrawPath(borda,caminho); }
        int Px(int valor)=>Math.Max(1,valor*DeviceDpi/96);
        if (Estilo.PapelDe(this) == PapelVisual.Ativo)
        {
            using var marca = new Pen(Estilo.AltoContraste ? p.NoDestaque : p.Destaque, Px(3)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            e.Graphics.DrawLine(marca, area.Left + Px(5), area.Top + Px(12), area.Left + Px(5), area.Bottom - Px(12));
        }
        var texto=Rectangle.Inflate(area,-Px(10),-Px(2));
        if (Simbolo is not null)
        {
            if (SomenteIcone) texto = new Rectangle((Width-Px(24))/2, texto.Y, Px(24), texto.Height);
            if(Simbolo=="win")
            {
                using var tinta=new SolidBrush(Enabled?corTexto:p.Secundario);
                var y=texto.Y+(texto.Height-Px(14))/2;
                for(var i=0;i<4;i++) e.Graphics.FillRectangle(tinta,texto.X+Px(4)+(i%2)*Px(8),y+(i/2)*Px(8),Px(6),Px(6));
            }
            else
            {
                using var icone=new Font("Segoe MDL2 Assets",11);
                TextRenderer.DrawText(e.Graphics,Simbolo,icone,new Rectangle(texto.X,texto.Y,Px(24),texto.Height),Enabled?corTexto:p.Secundario,
                    TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter);
            }
            texto.X+=Px(30); texto.Width-=Px(30);
        }
        if (!SomenteIcone) TextRenderer.DrawText(e.Graphics,Text,Font,texto,Enabled?corTexto:p.Secundario,
            TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|(TextAlign==ContentAlignment.MiddleLeft?TextFormatFlags.Left:TextFormatFlags.HorizontalCenter)|
            (ShowKeyboardCues?0:TextFormatFlags.HidePrefix));
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(area,-Px(4),-Px(4)),ForeColor,BackColor);
    }
    protected override void Dispose(bool disposing) { if (disposing) movimento.Dispose(); base.Dispose(disposing); }
}

internal sealed class SeletorModerno : ComboBox
{
    public SeletorModerno() { FlatStyle=FlatStyle.Flat; }
}

internal sealed class AbasModernas : TabControl
{
    private int hover = -1;
    private RectangleF indicador;
    private RectangleF origemIndicador;
    private readonly System.Windows.Forms.Timer movimento = new() { Interval = 16 };
    private readonly System.Diagnostics.Stopwatch relogio = new();
    public AbasModernas()
    {
        DrawMode=TabDrawMode.OwnerDrawFixed; Padding=new Point(14,10); Multiline = false; ShowToolTips = true;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
        movimento.Tick += (_, _) => { if (relogio.ElapsedMilliseconds >= 180 || !Visible || !Movimento.Permitido) movimento.Stop(); Invalidate(); };
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        var novo = -1;
        for (var i = 0; i < TabCount; i++) if (GetTabRect(i).Contains(e.Location)) { novo = i; break; }
        if (hover != novo) { hover = novo; Invalidate(); }
        base.OnMouseMove(e);
    }
    protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        movimento.Stop();
        if (SelectedIndex >= 0 && indicador.Width > 0 && Visible && Movimento.Permitido)
        { origemIndicador = indicador; relogio.Restart(); movimento.Start(); }
        base.OnSelectedIndexChanged(e); Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Estilo.Paleta.Superficie);
        for(var i=0;i<TabCount;i++) OnDrawItem(new DrawItemEventArgs(e.Graphics,Font,GetTabRect(i),i,
            i==SelectedIndex?DrawItemState.Selected:DrawItemState.None));
        if (SelectedIndex < 0) return;
        var alvo = Rectangle.Inflate(GetTabRect(SelectedIndex), -3, -3);
        var destino = new RectangleF(alvo.Left + 12, alvo.Bottom - 3, Math.Max(1, alvo.Width - 24), 3 * DeviceDpi / 96f);
        var f = movimento.Enabled ? (float)Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 180) : 1;
        indicador = new RectangleF(origemIndicador.X + (destino.X-origemIndicador.X)*f,
            destino.Y, origemIndicador.Width + (destino.Width-origemIndicador.Width)*f, destino.Height);
        using var tinta = new SolidBrush(Estilo.Paleta.Destaque);
        e.Graphics.FillRectangle(tinta, indicador);
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var p=Estilo.Paleta; var ativo=e.Index==SelectedIndex;
        using var fundo=new SolidBrush(p.Superficie); e.Graphics.FillRectangle(fundo,e.Bounds);
        var area=Rectangle.Inflate(e.Bounds,-3,-3);
        using var forma=FormaModerna.Caminho(area,7*DeviceDpi/96);
        using var tinta=new SolidBrush(ativo?(Estilo.AltoContraste?p.Destaque:p.NavegacaoAtiva):e.Index==hover?p.Alternada:p.Superficie); e.Graphics.FillPath(tinta,forma);
        // O TabControl mede as guias com Font. A seleção mantém a mesma fonte e
        // largura; o fundo e o indicador identificam a guia ativa sem cortar o texto.
        TextRenderer.DrawText(e.Graphics,TabPages[e.Index].Text,Font,Rectangle.Inflate(area,-2,-4),ativo&&Estilo.AltoContraste?p.NoDestaque:ativo?p.Texto:p.Secundario,
            TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix);
        if(Focused&&ShowFocusCues&&ativo) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(area,-4,-4),p.Texto,p.Superficie);
    }
    protected override void Dispose(bool disposing) { if (disposing) movimento.Dispose(); base.Dispose(disposing); }
}

internal class Cartao : Panel
{
    public Cartao()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Padding = new Padding(12);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var forma=FormaModerna.Caminho(Rectangle.Inflate(ClientRectangle,-1,-1),8*DeviceDpi/96);
        using var linha=new Pen(Estilo.Paleta.Borda); e.Graphics.DrawPath(linha,forma);
    }
}
