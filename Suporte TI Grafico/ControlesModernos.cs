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
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string? Simbolo { get; set; }
    public BotaoModerno() { DoubleBuffered = true; Padding = new Padding(10, 4, 10, 4); MinimumSize = new Size(0, 34); }
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
        if (hover && Enabled && !Estilo.AltoContraste) cor=ControlPaint.Light(cor, pressionado ? .04f : .08f);
        using var fundo=new SolidBrush(cor); e.Graphics.FillPath(fundo,caminho);
        if (Estilo.PapelDe(this) is not (PapelVisual.Navegacao or PapelVisual.Ativo))
        { using var borda=new Pen(p.Borda); e.Graphics.DrawPath(borda,caminho); }
        int Px(int valor)=>Math.Max(1,valor*DeviceDpi/96);
        var texto=Rectangle.Inflate(area,-Px(10),-Px(2));
        if (Simbolo is not null)
        {
            if(Simbolo=="win")
            {
                using var tinta=new SolidBrush(Enabled?ForeColor:p.Secundario);
                var y=texto.Y+(texto.Height-Px(14))/2;
                for(var i=0;i<4;i++) e.Graphics.FillRectangle(tinta,texto.X+Px(4)+(i%2)*Px(8),y+(i/2)*Px(8),Px(6),Px(6));
            }
            else
            {
                using var icone=new Font("Segoe MDL2 Assets",11);
                TextRenderer.DrawText(e.Graphics,Simbolo,icone,new Rectangle(texto.X,texto.Y,Px(24),texto.Height),Enabled?ForeColor:p.Secundario,
                    TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter);
            }
            texto.X+=Px(30); texto.Width-=Px(30);
        }
        TextRenderer.DrawText(e.Graphics,Text,Font,texto,Enabled?ForeColor:p.Secundario,
            TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|(TextAlign==ContentAlignment.MiddleLeft?TextFormatFlags.Left:TextFormatFlags.HorizontalCenter)|
            (ShowKeyboardCues?0:TextFormatFlags.HidePrefix));
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(area,-Px(4),-Px(4)),ForeColor,BackColor);
    }
}

internal sealed class SeletorModerno : ComboBox
{
    public SeletorModerno() { FlatStyle=FlatStyle.Flat; }
}

internal sealed class AbasModernas : TabControl
{
    public AbasModernas()
    {
        DrawMode=TabDrawMode.OwnerDrawFixed; Padding=new Point(14,9);
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Estilo.Paleta.Fundo);
        for(var i=0;i<TabCount;i++) OnDrawItem(new DrawItemEventArgs(e.Graphics,Font,GetTabRect(i),i,
            i==SelectedIndex?DrawItemState.Selected:DrawItemState.None));
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var p=Estilo.Paleta; var ativo=e.Index==SelectedIndex;
        using var fundo=new SolidBrush(p.Fundo); e.Graphics.FillRectangle(fundo,e.Bounds);
        var area=Rectangle.Inflate(e.Bounds,-2,-2);
        using var forma=FormaModerna.Caminho(area,6);
        using var tinta=new SolidBrush(ativo?p.Superficie:p.Fundo); e.Graphics.FillPath(tinta,forma);
        TextRenderer.DrawText(e.Graphics,TabPages[e.Index].Text,Font,area,ativo?p.Texto:p.Secundario,
            TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
        if(ativo) {using var linha=new Pen(p.Destaque,3); e.Graphics.DrawLine(linha,area.Left+10,area.Bottom-1,area.Right-10,area.Bottom-1);}
        if(Focused&&ShowFocusCues&&ativo) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(area,-4,-4),p.Texto,p.Superficie);
    }
}

internal class Cartao : Panel
{
    public Cartao() { DoubleBuffered=true; Padding=new Padding(12); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var forma=FormaModerna.Caminho(Rectangle.Inflate(ClientRectangle,-1,-1),8*DeviceDpi/96);
        using var linha=new Pen(Estilo.Paleta.Borda); e.Graphics.DrawPath(linha,forma);
    }
}
