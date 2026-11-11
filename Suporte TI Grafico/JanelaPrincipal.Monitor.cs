using System.Drawing.Drawing2D;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private TableLayoutPanel CriarTelaMonitor()
    {
        var pagina = CriarPagina();
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.Controls.Add(CriarRotulo("Monitor ao vivo", 20, true), 0, 0);
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var iniciar = new BotaoModerno { Text = "&Iniciar monitor", AutoSize = true };
        AplicarCorBotao(iniciar, true);
        acoes.Controls.Add(iniciar);
        var historico = new CheckBox { Text = "Mostrar &histórico de 60 amostras", Checked = true, AutoSize = true, Margin = new Padding(12, 8, 3, 3) };
        acoes.Controls.Add(historico);
        pagina.Controls.Add(acoes, 0, 1);
        var cpu = new GraficoUso("CPU");
        var ram = new GraficoUso("Memória RAM");
        var cartoes = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 2, Margin = Padding.Empty };
        cartoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cartoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cartoes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cartoes.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        cartoes.Controls.Add(cpu, 0, 0); cartoes.Controls.Add(ram, 1, 0);
        pagina.Controls.Add(cartoes, 0, 2);
        void OrganizarCartoes()
        {
            var ladoALado = cartoes.ClientSize.Width >= 720 * pagina.DeviceDpi / 96;
            var altura = (historico.Checked ? 284 : 192) * pagina.DeviceDpi / 96;
            cartoes.SuspendLayout();
            cartoes.SetCellPosition(ram, new TableLayoutPanelCellPosition(ladoALado ? 1 : 0, ladoALado ? 0 : 1));
            cartoes.SetColumnSpan(cpu, ladoALado ? 1 : 2);
            cartoes.SetColumnSpan(ram, ladoALado ? 1 : 2);
            cartoes.RowStyles[0].SizeType = SizeType.Absolute; cartoes.RowStyles[0].Height = altura;
            cartoes.RowStyles[1].Height = ladoALado ? 0 : altura;
            cartoes.Height = altura * (ladoALado ? 1 : 2);
            cartoes.ResumeLayout();
        }
        cartoes.SizeChanged += (_, _) => OrganizarCartoes();
        pagina.DpiChangedAfterParent += (_, _) => OrganizarCartoes();
        cpu.ExibirHistorico(true); ram.ExibirHistorico(true);
        historico.CheckedChanged += (_, _) =>
        {
            cpu.ExibirHistorico(historico.Checked);
            ram.ExibirHistorico(historico.Checked);
            OrganizarCartoes();
            pagina.PerformLayout();
            conteudoResultados.Parent?.PerformLayout();
        };
        var status = CriarRotulo("Pronto · coleta a cada segundo · estatísticas da janela de até 60 amostras.", 9, false);
        Estilo.Marcar(status, PapelVisual.Secundario);
        pagina.Controls.Add(status, 0, 3);
        pagina.Layout += (_, _) =>
        {
            var minimo = pagina.Padding.Vertical + pagina.Controls[0].Height + pagina.Controls[0].Margin.Vertical +
                acoes.Height + acoes.Margin.Vertical + cartoes.Height + status.Height + status.Margin.Vertical;
            if (pagina.MinimumSize.Height != minimo) pagina.MinimumSize = new Size(0, minimo);
        };

        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        var leitor = new MonitorRecursos();
        void Parar()
        {
            timer.Stop();
            cpu.PausarAnimacao(); ram.PausarAnimacao();
            iniciar.Text = "&Iniciar monitor";
            status.Text = "Pausado · leituras e estatísticas preservadas · iniciar abre uma nova sessão.";
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
            status.Text = "Coletando · 1 amostra/s · estatísticas de até 60 amostras; lacunas não contam como zero.";
            timer.Start();
        };
        timer.Tick += (_, _) =>
        {
            var (usoCpu, usoRam, total, usada) = leitor.Ler();
            cpu.Adicionar(usoCpu, "Uso total do processador\nEscala fixa de 0–100%");
            ram.Adicionar(usoRam, usoRam is not null ? $"{EmGiB(usada)} usada · {EmGiB(total - usada)} disponível\n{EmGiB(total)} total" : "Memória física indisponível");
        };
        pagina.VisibleChanged += (_, _) => { if (!pagina.Visible && timer.Enabled) Parar(); };
        pagina.Disposed += (_, _) => timer.Dispose();
        return pagina;
    }

    private static string EmGiB(ulong bytes) => $"{bytes / 1073741824.0:N1} GiB";

    private sealed class GraficoUso : Cartao
    {
        private readonly SerieMonitor serie = new();
        private readonly Label leitura;
        private readonly Label detalhe;
        private readonly Label estatisticas;
        private readonly Panel trama;
        private Color cor => nome == "CPU" ? Estilo.Paleta.Cpu : Estilo.Paleta.Ram;
        private readonly string nome;
        private readonly System.Windows.Forms.Timer animacao = new() { Interval = 16 };
        private readonly System.Diagnostics.Stopwatch relogio = new();
        private double? anteriorValor;
        private bool mostrarHistorico;

        public void ExibirHistorico(bool exibir)
        {
            mostrarHistorico = exibir;
            MinimumSize = new Size(0, (exibir ? 272 : 180) * DeviceDpi / 96);
            trama.Invalidate();
        }

        public GraficoUso(string nome)
        {
            this.nome = nome;
            Estilo.Marcar(this, PapelVisual.Superficie);
            Dock = DockStyle.Fill;
            MinimumSize = new Size(0, 180);
            Margin = new Padding(3, 6, 3, 3);
            Padding = new Padding(12, 4, 12, 6);
            BackColor = AltoContraste ? SystemColors.Window : Superficie;
            var cabecalho = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            cabecalho.Controls.Add(CriarRotulo(nome, 13, true));
            leitura = CriarRotulo("—", 27, true);
            leitura.AccessibleName = nome + ": aguardando amostra";
            cabecalho.Controls.Add(leitura);
            detalhe = CriarRotulo("Aguardando amostra", 9, false);
            Estilo.Marcar(detalhe, PapelVisual.Secundario);
            cabecalho.Controls.Add(detalhe);
            estatisticas = CriarRotulo("Média —  ·  Pico —  ·  0/0 válidas", 9, false);
            estatisticas.Dock = DockStyle.Bottom;
            Estilo.Marcar(estatisticas, PapelVisual.Secundario);
            SizeChanged += (_, _) =>
            {
                var largura = Math.Max(100, ClientSize.Width - Padding.Horizontal - 8);
                detalhe.MaximumSize = leitura.MaximumSize = estatisticas.MaximumSize = new Size(largura, 0);
            };
            trama = new AreaDesenho { Dock = DockStyle.Fill };
            trama.Paint += (_, evento) => Desenhar(evento.Graphics);
            trama.Resize += (_, _) => trama.Invalidate();
            Controls.Add(trama);
            Controls.Add(estatisticas);
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
            serie.Limpar();
            PausarAnimacao(); anteriorValor = null;
            leitura.Text = "—"; detalhe.Text = "Aguardando amostra";
            estatisticas.Text = "Média —  ·  Pico —  ·  0/0 válidas";
            leitura.AccessibleName = nome + ": aguardando amostra";
            trama.Invalidate();
        }

        public void Adicionar(double? valor, string resumo)
        {
            valor = MedidasVisuais.Percentual(valor);
            if (valor is null) resumo = "Indisponível";
            anteriorValor = serie.Atual;
            serie.Adicionar(valor);
            leitura.Text = valor is double n ? $"{n:N0}%" : "—";
            detalhe.Text = valor is null ? "Leitura indisponível" : resumo;
            leitura.AccessibleName = $"{nome}: {(valor is null ? "indisponível" : leitura.Text)}. {detalhe.Text}";
            var estatistica = serie.Resumo;
            estatisticas.Text = $"Média {(estatistica.Media is double media ? $"{media:N0}%" : "—")}  ·  Pico {(estatistica.Pico is double pico ? $"{pico:N0}%" : "—")}  ·  {estatistica.Validas}/{serie.Quantidade} válidas";
            if (Visible && Movimento.Permitido) { relogio.Restart(); animacao.Start(); }
            trama.Invalidate();
        }

        private void Desenhar(Graphics desenho)
        {
            var px = DeviceDpi / 96f;
            var barra = new RectangleF(6 * px, 8 * px, Math.Max(1, trama.Width - 12 * px), 8 * px);
            using (var fundo = new SolidBrush(Estilo.Paleta.Borda)) desenho.FillRectangle(fundo, barra);
            var atual = serie.Atual;
            if (atual is double valorBarra)
            {
                var fBarra = animacao.Enabled ? Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 240) : 1;
                var visual = anteriorValor is double de ? de + (valorBarra - de) * fBarra : valorBarra;
                using var preenchimento = new SolidBrush(AltoContraste ? SystemColors.Highlight : cor);
                desenho.FillRectangle(preenchimento, barra.X, barra.Y, barra.Width * (float)(visual / 100), barra.Height);
            }
            foreach (var percentual in new[] { 0, 50, 100 })
            {
                var texto = percentual + "%";
                var largura = TextRenderer.MeasureText(texto, Font).Width;
                var x = barra.Left + barra.Width * percentual / 100 - largura * percentual / 100;
                TextRenderer.DrawText(desenho, texto, Font, new Point((int)x, (int)barra.Bottom + 3), ForeColor);
            }
            if (!mostrarHistorico) return;
            var margemVertical = Font.Height / 2 + 2;
            var area = Rectangle.FromLTRB(TextRenderer.MeasureText("100%", Font).Width + 8, (int)(barra.Bottom + Font.Height + 14 * px) + margemVertical,
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
            var indice = 60 - serie.Quantidade;
            var f = animacao.Enabled ? Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 240) : 1;
            foreach (var valor in serie.Pontos)
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
