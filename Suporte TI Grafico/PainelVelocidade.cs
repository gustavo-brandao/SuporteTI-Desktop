using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed class PainelVelocidade : Cartao
{
    private readonly Label etapa = new() { AutoSize = true, Text = "Teste de conexão" };
    private readonly Label vazao = new() { AutoSize = true, Text = "— Mbps", Font = new Font("Segoe UI", 24, FontStyle.Bold), AccessibleName = "Vazão medida" };
    private readonly Label detalhe = new() { AutoSize = true, Text = "Download e upload · amostras reais em paralelo" };
    private readonly Area trama = new() { Dock = DockStyle.Fill };
    private bool ativo;
    private double fracao, escala = 100;
    private double? taxaAtual;

    public PainelVelocidade()
    {
        Dock = DockStyle.Fill; Height = 280; MinimumSize = new Size(0, 250); Margin = new Padding(0, 0, 0, 10);
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
        trama.Resize += (_, _) => trama.Invalidate();
        trama.AccessibleName = "Velocímetro em Mbps; escala adaptável indicada no arco";
    }

    public void Limpar()
    {
        ativo = false; fracao = 0; escala = 100; taxaAtual = null;
        etapa.Text = "Pronto para testar"; vazao.Text = "— Mbps";
        vazao.AccessibleName = "Vazão medida";
        detalhe.Text = "Até 10 s por direção · até 1 GiB no total"; trama.Invalidate();
    }
    public void Iniciar()
    {
        Limpar(); ativo = true; etapa.Text = "Conectando ao servidor…";
    }
    public void Atualizar(ProgressoVelocidade progresso)
    {
        if (!ativo) return;
        taxaAtual = MedidasVisuais.Taxa(progresso.Mbps);
        escala = MedidasVisuais.Escala(escala, taxaAtual);
        etapa.Text = progresso.Etapa + (progresso.Etapa == "Upload" ? " · medindo blocos confirmados" : " · medindo");
        vazao.Text = taxaAtual is double taxa ? $"{taxa:N1} Mbps" : "— Mbps";
        vazao.AccessibleName = "Vazão medida: " + vazao.Text;
        detalhe.Text = $"{progresso.Segundos:N0} s decorridos · {progresso.Bytes / 1048576.0:N1} MiB transferidos";
        fracao = Math.Clamp(double.IsFinite(progresso.Fracao) ? progresso.Fracao : 0, 0, 1);
        trama.Invalidate();
    }
    public void Concluir(string mensagem, Resposta? resultado = null)
    {
        ativo = false; etapa.Text = mensagem;
        var download = TesteVelocidade.ObterTaxa(resultado, "Download");
        var upload = TesteVelocidade.ObterTaxa(resultado, "Upload");
        vazao.Text = upload is not null ? "↑ " + upload : download is not null ? "↓ " + download : "— Mbps";
        taxaAtual = MedidasVisuais.LerTaxa(upload ?? download);
        escala = MedidasVisuais.Escala(escala, taxaAtual);
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
        var px = DeviceDpi / 96f;
        var raio = Math.Min((w - 110 * px) / 2, h - 42 * px);
        if (raio < 12) return;
        var centro = new PointF(w / 2f, raio + 8 * px);
        var arco = new RectangleF(centro.X - raio, centro.Y - raio, raio * 2, raio * 2);
        using var trilho = new Pen(p.Borda, 9 * px) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var preenchido = new Pen(p.Destaque, 9 * px) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(trilho, arco, 180, 180);
        if (taxaAtual is double taxa)
        {
            var fracaoTaxa = (float)Math.Clamp(taxa / escala, 0, 1);
            if (fracaoTaxa > 0) g.DrawArc(preenchido, arco, 180, 180 * fracaoTaxa);
            var radianos = Math.PI * (1 + fracaoTaxa);
            using var agulha = new Pen(p.Texto, 2 * px);
            g.DrawLine(agulha, centro, new PointF(centro.X + (float)Math.Cos(radianos) * raio * .78f,
                centro.Y + (float)Math.Sin(radianos) * raio * .78f));
            using var eixo = new SolidBrush(p.Texto);
            g.FillEllipse(eixo, centro.X - 4 * px, centro.Y - 4 * px, 8 * px, 8 * px);
        }
        void Legenda(string texto, float x, float y)
        {
            var tamanho = TextRenderer.MeasureText(texto, Font);
            TextRenderer.DrawText(g, texto, Font, new Point((int)(x - tamanho.Width / 2f), (int)y), p.Texto);
        }
        Legenda("0", centro.X - raio - 14 * px, centro.Y - Font.Height / 2f);
        Legenda($"{escala:N0}", centro.X + raio + 30 * px, centro.Y - Font.Height / 2f);
        Legenda($"Escala 0–{escala:N0} Mbps", centro.X, centro.Y + 7 * px);
    }
    private sealed class Area : Panel { public Area() => DoubleBuffered = true; }
}
