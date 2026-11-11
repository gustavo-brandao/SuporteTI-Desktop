using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed class PainelVelocidade : Cartao
{
    private readonly Label etapa = new() { AutoSize = true, Text = "Teste de conexão" };
    private readonly Label vazao = new() { AutoSize = true, Text = "— Mbps", Font = new Font("Segoe UI", 24, FontStyle.Bold), AccessibleName = "Vazão medida" };
    private readonly Label detalhe = new() { AutoSize = true, Text = "Download e upload · amostras reais em paralelo" };
    private readonly Area trama = new() { Dock = DockStyle.Fill };
    private readonly Queue<double?> pontos = new();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 33 };
    private bool ativo;
    private double fracao, angulo;
    private string direcao = "";

    public PainelVelocidade()
    {
        Dock = DockStyle.Fill; Height = 192; MinimumSize = new Size(0, 192); Margin = new Padding(0, 0, 0, 10);
        Estilo.Marcar(this, PapelVisual.Superficie);
        var texto = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        texto.Controls.Add(etapa); texto.Controls.Add(vazao); texto.Controls.Add(detalhe);
        Controls.Add(trama); Controls.Add(texto);
        SizeChanged += (_, _) =>
        {
            var largura = Math.Max(100, ClientSize.Width - Padding.Horizontal - 8);
            etapa.MaximumSize = detalhe.MaximumSize = new Size(largura, 0);
        };
        trama.Paint += (_, e) => Desenhar(e.Graphics);
        timer.Tick += (_, _) => { if (!Movimento.Permitido || !Visible) { timer.Stop(); trama.Invalidate(); return; } angulo = (angulo + 9) % 360; trama.Invalidate(); };
        VisibleChanged += (_, _) => { if (!Visible) timer.Stop(); else if (ativo && Movimento.Permitido) timer.Start(); };
    }

    public void Limpar()
    {
        ativo = false; timer.Stop(); fracao = 0; pontos.Clear(); direcao = "";
        etapa.Text = "Pronto para testar"; vazao.Text = "— Mbps";
        vazao.AccessibleName = "Vazão medida";
        detalhe.Text = "Até 10 s por direção · até 1 GiB no total"; trama.Invalidate();
    }
    public void Iniciar()
    {
        Limpar(); ativo = true; etapa.Text = "Conectando ao servidor…";
        if (Movimento.Permitido && Visible) timer.Start();
    }
    public void Atualizar(ProgressoVelocidade progresso)
    {
        if (!ativo) return;
        if (direcao != progresso.Etapa) { pontos.Clear(); direcao = progresso.Etapa; }
        etapa.Text = progresso.Etapa + (progresso.Etapa == "Upload" ? " · medindo blocos confirmados" : " · medindo");
        vazao.Text = progresso.Mbps is double taxa ? $"{taxa:N1} Mbps" : "— Mbps";
        vazao.AccessibleName = "Vazão medida: " + vazao.Text;
        detalhe.Text = $"{progresso.Segundos:N0} s decorridos · {progresso.Bytes / 1048576.0:N1} MiB transferidos";
        fracao = progresso.Fracao;
        if (pontos.Count == 80) pontos.Dequeue();
        pontos.Enqueue(progresso.Mbps); trama.Invalidate();
    }
    public void Concluir(string mensagem, Resposta? resultado = null)
    {
        ativo = false; timer.Stop(); etapa.Text = mensagem;
        var download = TesteVelocidade.ObterTaxa(resultado, "Download");
        var upload = TesteVelocidade.ObterTaxa(resultado, "Upload");
        vazao.Text = upload is not null ? "↑ " + upload : download is not null ? "↓ " + download : "— Mbps";
        detalhe.Text = resultado is null ? "Medição interrompida. Inicie um novo teste para obter um resultado completo." :
            $"↓ Download: {download ?? "indisponível"}    ↑ Upload: {upload ?? "indisponível"}";
        if (resultado is not null) fracao = 1;
        vazao.AccessibleName = "Medição finalizada: " + detalhe.Text;
        trama.Invalidate();
    }
    private void Desenhar(Graphics g)
    {
        var p = Estilo.Paleta;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var w = trama.Width; var h = trama.Height;
        if (w < 30 || h < 20) return;
        using var baseLinha = new Pen(p.Borda, 3);
        using var indicador = new Pen(p.Destaque, 3);
        g.DrawLine(baseLinha, 2, h - 6, w - 2, h - 6);
        if (fracao > 0) g.DrawLine(indicador, 2, h - 6, 2 + (w - 4) * (float)fracao, h - 6);
        if (ativo)
        {
            if (Movimento.Permitido) g.DrawArc(indicador, w - 22, 3, 16, 16, (float)angulo, 250);
            else { using var tinta = new SolidBrush(p.Destaque); g.FillEllipse(tinta, w - 17, 7, 6, 6); }
        }
        var valores = pontos.ToArray();
        var max = Math.Max(1, valores.Max() ?? 1);
        PointF? anterior = null;
        using var linha = new Pen(p.Cpu, 2);
        for (var i = 0; i < valores.Length; i++)
        {
            if (valores[i] is not double valor) { anterior = null; continue; }
            var ponto = new PointF(3 + (w - 34) * i / 79f, h - 16 - (float)(Math.Max(1, h - 24) * valor / max));
            if (anterior is PointF a) g.DrawLine(linha, a, ponto);
            anterior = ponto;
        }
    }
    protected override void Dispose(bool disposing) { if (disposing) timer.Dispose(); base.Dispose(disposing); }
    private sealed class Area : Panel { public Area() => DoubleBuffered = true; }
}
