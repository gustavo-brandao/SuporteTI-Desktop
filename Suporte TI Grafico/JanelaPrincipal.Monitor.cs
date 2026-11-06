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
        var iniciar = new Button { Text = "&Iniciar monitor", AutoSize = true };
        AplicarCorBotao(iniciar, true);
        acoes.Controls.Add(iniciar);
        acoes.Controls.Add(CriarRotulo("Amostra por segundo · até 60 pontos por gráfico", 10, false));
        pagina.Controls.Add(acoes, 0, 1);
        var cpu = new GraficoUso("CPU", Color.FromArgb(119, 186, 255));
        var ram = new GraficoUso("Memória RAM", Color.FromArgb(101, 219, 195));
        pagina.Controls.Add(cpu, 0, 2);
        pagina.Controls.Add(ram, 0, 3);
        var status = CriarRotulo("Monitor pronto. Inicie para medir o uso.", 10, false);
        pagina.Controls.Add(status, 0, 4);

        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        var leitor = new MonitorRecursos();
        void Parar()
        {
            timer.Stop();
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

    private sealed class GraficoUso : Panel
    {
        private readonly Queue<double?> pontos = new();
        private readonly Label leitura;
        private readonly Panel trama;
        private readonly Color cor;
        private readonly string nome;

        public GraficoUso(string nome, Color cor)
        {
            this.nome = nome;
            this.cor = AltoContraste ? SystemColors.WindowText : cor;
            Dock = DockStyle.Fill;
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
        }

        public void Limpar()
        {
            pontos.Clear();
            leitura.Text = "Aguardando amostra";
            leitura.AccessibleName = nome + ": aguardando amostra";
            trama.Invalidate();
        }

        public void Adicionar(double? valor, string resumo)
        {
            if (pontos.Count == 60)
                pontos.Dequeue();
            pontos.Enqueue(valor);
            leitura.Text = resumo;
            leitura.AccessibleName = $"{nome}: {resumo}";
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
            PointF? anterior = null;
            var indice = 60 - pontos.Count;
            foreach (var valor in pontos)
            {
                if (valor is double percentual)
                {
                    var ponto = new PointF(area.Left + area.Width * indice / 59f, area.Bottom - (float)(area.Height * percentual / 100));
                    if (anterior is PointF ultimo)
                        desenho.DrawLine(linha, ultimo, ponto);
                    else
                        desenho.DrawEllipse(linha, ponto.X - 1, ponto.Y - 1, 2, 2);
                    anterior = ponto;
                }
                else
                    anterior = null;
                indice++;
            }
        }
    }

    private sealed class AreaDesenho : Panel
    {
        public AreaDesenho() => DoubleBuffered = true;
    }
}
