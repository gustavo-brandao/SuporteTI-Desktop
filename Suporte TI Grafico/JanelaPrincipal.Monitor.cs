using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private TableLayoutPanel CriarTelaMonitor()
    {
        var pagina = CriarPagina();
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pagina.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.Controls.Add(CriarRotulo("Monitor ao vivo", 20, true), 0, 0);
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var iniciar = new BotaoModerno { Text = "&Iniciar monitor", AutoSize = true };
        AplicarCorBotao(iniciar, true);
        acoes.Controls.Add(iniciar);
        acoes.Controls.Add(CriarRotulo("Amostra por segundo · até 60 pontos por gráfico", 10, false));
        pagina.Controls.Add(acoes, 0, 1);
        var cpu = new GraficoUso("CPU");
        var ram = new GraficoUso("Memória RAM");
        pagina.Controls.Add(cpu, 0, 2);
        pagina.Controls.Add(ram, 0, 3);
        var status = CriarRotulo("Monitor pronto. Inicie para medir o uso.", 10, false);
        pagina.Controls.Add(status, 0, 4);

        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        var leitor = new MonitorRecursos();
        void Parar()
        {
            timer.Stop();
            cpu.PausarAnimacao(); ram.PausarAnimacao();
            iniciar.Text = "&Iniciar monitor";
            status.Text = "Monitor pausado. O histórico permanece visível.";
        }
        iniciar.Click += (_, _) =>
        {
            if (timer.Enabled)
            {
                Parar();
                return;
            }
            leitor = new MonitorRecursos();
            leitor.Ler();
            cpu.Limpar();
            ram.Limpar();
            iniciar.Text = "&Pausar monitor";
            status.Text = "Monitor em execução.";
            timer.Start();
        };
        timer.Tick += (_, _) =>
        {
            var (usoCpu, usoRam, total, usada) = leitor.Ler();
            cpu.Adicionar(usoCpu, usoCpu is double cpuAtual ? $"{cpuAtual:N0}%" : "Indisponível");
            ram.Adicionar(usoRam, usoRam is double ramAtual ? $"{ramAtual:N0}% · {EmGiB(usada)} de {EmGiB(total)}" : "Indisponível");
        };
        pagina.VisibleChanged += (_, _) => { if (!pagina.Visible && timer.Enabled) Parar(); };
        pagina.Disposed += (_, _) => timer.Dispose();
        return pagina;
    }

    private static string EmGiB(ulong bytes) => $"{bytes / 1073741824.0:N1} GiB";

    private sealed class GraficoUso : Cartao
    {
        private readonly Queue<double?> pontos = new();
        private readonly Label leitura;
        private readonly Panel trama;
        private Color cor => nome == "CPU" ? Estilo.Paleta.Cpu : Estilo.Paleta.Ram;
        private readonly string nome;
        private readonly System.Windows.Forms.Timer animacao = new() { Interval = 16 };
        private readonly System.Diagnostics.Stopwatch relogio = new();
        private double? anteriorValor;

        public GraficoUso(string nome)
        {
            this.nome = nome;
            Estilo.Marcar(this, PapelVisual.Superficie);
            Dock = DockStyle.Fill;
            MinimumSize = new Size(0, 128);
            Margin = new Padding(3, 6, 3, 3);
            Padding = new Padding(12, 4, 12, 6);
            BackColor = AltoContraste ? SystemColors.Window : Superficie;
            var cabecalho = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
            cabecalho.Controls.Add(CriarRotulo(nome, 13, true));
            leitura = CriarRotulo("Aguardando amostra", 11, false);
            leitura.AccessibleName = nome + ": aguardando amostra";
            cabecalho.Controls.Add(leitura);
            trama = new AreaDesenho { Dock = DockStyle.Fill };
            trama.Paint += (_, evento) => Desenhar(evento.Graphics);
            trama.Resize += (_, _) => trama.Invalidate();
            Controls.Add(trama);
            Controls.Add(cabecalho);
            animacao.Tick += (_, _) =>
            {
                if (!Visible || !Movimento.Permitido || relogio.ElapsedMilliseconds >= 240) animacao.Stop();
                trama.Invalidate();
            };
            VisibleChanged += (_, _) => { if (!Visible) PausarAnimacao(); };
        }

        public void PausarAnimacao() { animacao.Stop(); trama.Invalidate(); }

        public void Limpar()
        {
            pontos.Clear();
            PausarAnimacao(); anteriorValor = null;
            leitura.Text = "Aguardando amostra";
            leitura.AccessibleName = nome + ": aguardando amostra";
            trama.Invalidate();
        }

        public void Adicionar(double? valor, string resumo)
        {
            anteriorValor = pontos.LastOrDefault();
            if (pontos.Count == 60)
                pontos.Dequeue();
            pontos.Enqueue(valor);
            leitura.Text = resumo;
            leitura.AccessibleName = $"{nome}: {resumo}";
            if (Visible && Movimento.Permitido) { relogio.Restart(); animacao.Start(); }
            trama.Invalidate();
        }

        private void Desenhar(Graphics desenho)
        {
            var margemVertical = Font.Height / 2 + 2;
            var area = Rectangle.FromLTRB(TextRenderer.MeasureText("100%", Font).Width + 8, margemVertical,
                trama.Width - 8, trama.Height - margemVertical);
            if (area.Width <= 0 || area.Height <= 0)
                return;
            using var grade = new Pen(AltoContraste ? SystemColors.WindowText : Borda);
            using var linha = new Pen(cor, 2);
            foreach (var valor in new[] { 0, 50, 100 })
            {
                var y = area.Bottom - area.Height * valor / 100;
                desenho.DrawLine(grade, area.Left, y, area.Right, y);
                TextRenderer.DrawText(desenho, $"{valor}%", Font, new Point(0, y - Font.Height / 2),
                    AltoContraste ? SystemColors.WindowText : Secundario);
            }
            desenho.SmoothingMode = SmoothingMode.AntiAlias;
            desenho.SetClip(area);
            PointF? anterior = null;
            var indice = 60 - pontos.Count;
            var f = animacao.Enabled ? Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 240) : 1;
            foreach (var valor in pontos)
            {
                if (valor is double percentual)
                {
                    var ultimo = indice == 59;
                    var visual = ultimo && anteriorValor is double de ? de + (percentual - de) * f : percentual;
                    var x = indice + (ultimo ? 0 : 1-f);
                    var ponto = new PointF(area.Left + (float)(area.Width * x / 59), area.Bottom - (float)(area.Height * visual / 100));
                    if (anterior is PointF precedente)
                    {
                        using var sombra = new SolidBrush(Color.FromArgb(AltoContraste ? 0 : 30, cor));
                        desenho.FillPolygon(sombra, [precedente, ponto, new(ponto.X, area.Bottom), new(precedente.X, area.Bottom)]);
                        desenho.DrawLine(linha, precedente, ponto);
                    }
                    else
                        desenho.DrawEllipse(linha, ponto.X - 1, ponto.Y - 1, 2, 2);
                    if (ultimo) { using var ponta = new SolidBrush(cor); desenho.FillEllipse(ponta, ponto.X-3, ponto.Y-3, 6, 6); }
                    anterior = ponto;
                }
                else
                    anterior = null;
                indice++;
            }
            desenho.ResetClip();
        }
        protected override void Dispose(bool disposing) { if (disposing) animacao.Dispose(); base.Dispose(disposing); }
    }

    private sealed class AreaDesenho : Panel
    {
        public AreaDesenho() => DoubleBuffered = true;
    }
}
