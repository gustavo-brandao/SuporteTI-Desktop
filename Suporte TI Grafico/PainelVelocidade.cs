using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed class PainelVelocidade : Cartao
{
    private readonly Label etapa = new() { AutoSize = true, Text = "Pronto para testar", Font = new Font("Segoe UI", 9, FontStyle.Bold) };
    private readonly Label vazao = new() { AutoSize = true, Text = "— Mbps", Font = new Font("Segoe UI", 27, FontStyle.Bold), AccessibleName = "Vazão medida" };
    private readonly Label detalhe = new() { AutoSize = true, Font = new Font("Segoe UI", 9) };
    private readonly Label rodape = new() { AutoSize = true, Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 8), Text = "Cloudflare · HTTPS · até 1 GiB por teste" };
    private readonly Area trama = new() { Dock = DockStyle.Fill };
    private readonly TableLayoutPanel corpo = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Margin = Padding.Empty };
    private readonly ResumoDirecao download = new("↓ Download");
    private readonly ResumoDirecao upload = new("↑ Upload");
    private readonly EstadoMedicaoVelocidade medicao = new();

    public int AlturaPreferida(int largura) => (largura >= 640 * DeviceDpi / 96 ? 360 : 440) * DeviceDpi / 96;

    public PainelVelocidade()
    {
        Dock = DockStyle.Fill; Height = 360; MinimumSize = new Size(0, 360); Margin = new Padding(0, 0, 0, 10);
        Padding = new Padding(12, 8, 12, 8);
        Estilo.Marcar(this, PapelVisual.Superficie);
        Estilo.Marcar(detalhe, PapelVisual.Secundario); Estilo.Marcar(rodape, PapelVisual.Secundario);
        var texto = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        texto.Controls.Add(etapa); texto.Controls.Add(vazao); texto.Controls.Add(detalhe);
        corpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        corpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        corpo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        corpo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        corpo.Controls.Add(trama, 0, 0); corpo.SetRowSpan(trama, 2);
        corpo.Controls.Add(download, 1, 0); corpo.Controls.Add(upload, 1, 1);
        Controls.Add(corpo); Controls.Add(rodape); Controls.Add(texto);
        SizeChanged += (_, _) =>
        {
            var largura = Math.Max(100, ClientSize.Width - Padding.Horizontal - 8);
            etapa.MaximumSize = detalhe.MaximumSize = rodape.MaximumSize = new Size(largura, 0);
            var amplo = ClientSize.Width >= 640 * DeviceDpi / 96;
            corpo.SuspendLayout();
            corpo.SetRowSpan(trama, amplo ? 2 : 1); corpo.SetColumnSpan(trama, amplo ? 1 : 2);
            corpo.SetCellPosition(download, new TableLayoutPanelCellPosition(amplo ? 1 : 0, amplo ? 0 : 1));
            corpo.SetCellPosition(upload, new TableLayoutPanelCellPosition(1, 1));
            corpo.ColumnStyles[0].Width = amplo ? 62 : 50; corpo.ColumnStyles[1].Width = amplo ? 38 : 50;
            corpo.RowStyles[0].Height = amplo ? 50 : 60; corpo.RowStyles[1].Height = amplo ? 50 : 40;
            corpo.ResumeLayout();
        };
        trama.Paint += (_, e) => Desenhar(e.Graphics);
        trama.Resize += (_, _) => trama.Invalidate();
        trama.AccessibleName = "Velocímetro em Mbps; escala adaptável indicada no arco";
        Limpar();
    }

    public void Limpar()
    {
        medicao.Limpar();
        etapa.Text = "Pronto para testar"; vazao.Text = "— Mbps";
        vazao.AccessibleName = "Vazão medida";
        detalhe.Text = "Latência → Download → Upload · até 10 s por direção";
        AtualizarResumos();
    }
    public void Iniciar()
    {
        Limpar(); medicao.Iniciar(); etapa.Text = "Conectando ao servidor…"; AtualizarResumos();
    }
    public void Atualizar(ProgressoVelocidade progresso)
    {
        if (!medicao.Atualizar(progresso)) return;
        etapa.Text = progresso.Etapa + (progresso.Etapa == "Upload" ? " · medindo blocos confirmados" : " · medindo");
        vazao.Text = medicao.Taxa is double taxa ? $"{taxa:N1} Mbps" : "— Mbps";
        vazao.AccessibleName = "Taxa provisória de " + progresso.Etapa + ": " + vazao.Text;
        detalhe.Text = $"{progresso.Segundos:N0} s decorridos · {progresso.Bytes / 1048576.0:N1} MiB transferidos";
        AtualizarResumos();
    }
    public void Concluir(string mensagem, Resposta? resultado = null)
    {
        medicao.Concluir(resultado); etapa.Text = mensagem;
        vazao.Text = medicao.Upload is not null ? "↑ " + medicao.Upload : medicao.Download is not null ? "↓ " + medicao.Download : "— Mbps";
        detalhe.Text = resultado is null ? "Medição interrompida. Inicie um novo teste para obter um resultado completo." :
            "Resultados finais por direção · detalhes e avisos na tabela abaixo";
        vazao.AccessibleName = "Medição finalizada: " + mensagem + ". " + vazao.Text;
        AtualizarResumos();
    }
    private void AtualizarResumos()
    {
        download.Definir(medicao.Download, medicao.Situacao("Download"));
        upload.Definir(medicao.Upload, medicao.Situacao("Upload"));
        rodape.Text = medicao.PossuiResultado ? $"Latência HTTPS: {medicao.Latencia ?? "indisponível"} · Cloudflare" :
            "Cloudflare · HTTPS · até 1 GiB por teste";
        trama.AccessibleName = $"Velocímetro: {(medicao.Taxa is double n ? $"{n:N1} Mbps" : "sem taxa válida")}. Escala de 0 a {medicao.Escala:N0} Mbps.";
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
        if (medicao.Fracao > 0) g.DrawLine(indicador, 2, h - 6, 2 + (w - 4) * (float)medicao.Fracao, h - 6);
        var px = DeviceDpi / 96f;
        var raio = Math.Min((w - 110 * px) / 2, h - 42 * px);
        if (raio < 12) return;
        var centro = new PointF(w / 2f, raio + 8 * px);
        var arco = new RectangleF(centro.X - raio, centro.Y - raio, raio * 2, raio * 2);
        using var trilho = new Pen(p.Borda, 9 * px) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var preenchido = new Pen(p.Destaque, 9 * px) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(trilho, arco, 180, 180);
        if (medicao.Taxa is double taxa)
        {
            var fracaoTaxa = (float)Math.Clamp(taxa / medicao.Escala, 0, 1);
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
        Legenda($"{medicao.Escala:N0}", centro.X + raio + 30 * px, centro.Y - Font.Height / 2f);
        Legenda($"Escala 0–{medicao.Escala:N0} Mbps", centro.X, centro.Y + 7 * px);
    }
    private sealed class ResumoDirecao : Cartao
    {
        private readonly Label valor = new() { AutoSize = true, Text = "— Mbps", Font = new Font("Segoe UI", 17, FontStyle.Bold) };
        private readonly Label estado = new() { AutoSize = true, Font = new Font("Segoe UI", 8) };
        public ResumoDirecao(string nome)
        {
            Dock = DockStyle.Fill; Padding = new Padding(8, 4, 8, 4); Margin = new Padding(4);
            var coluna = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            var titulo = new Label { Text = nome, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            coluna.Controls.Add(titulo); coluna.Controls.Add(valor); coluna.Controls.Add(estado); Controls.Add(coluna);
            Estilo.Marcar(estado, PapelVisual.Secundario);
            SizeChanged += (_, _) =>
            {
                var largura = Math.Max(60, ClientSize.Width - Padding.Horizontal - 10);
                valor.MaximumSize = estado.MaximumSize = new Size(largura, 0);
            };
        }
        public void Definir(string? leitura, string situacao) { valor.Text = leitura ?? "— Mbps"; estado.Text = situacao; }
    }
    private sealed class Area : Panel { public Area() => DoubleBuffered = true; }
}
