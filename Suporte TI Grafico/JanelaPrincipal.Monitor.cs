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
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var iniciar = new BotaoModerno { Text = "&Iniciar monitor", AutoSize = true };
        AplicarCorBotao(iniciar, true);
        acoes.Controls.Add(iniciar);
        var historico = new CheckBox { Text = "Ver &histórico", AccessibleName = "Mostrar histórico de 60 amostras", AutoSize = true, Margin = new Padding(12, 8, 3, 3) };
        acoes.Controls.Add(historico);
        var processos = new BotaoModerno { Text = "CPU e processos", AccessibleName = "Abrir consulta de CPU e processos", AutoSize = true };
        processos.Click += (_, _) => abrirFerramenta?.Invoke("desempenho");
        acoes.Controls.Add(processos);
        pagina.Controls.Add(acoes, 0, 0);
        var cpu = new GraficoUso("CPU");
        var ram = new GraficoUso("Memória RAM");
        var cartoes = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 2, Margin = Padding.Empty };
        cartoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cartoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cartoes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        cartoes.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        cartoes.Controls.Add(cpu, 0, 0); cartoes.Controls.Add(ram, 1, 0);
        pagina.Controls.Add(cartoes, 0, 1);
        void OrganizarCartoes()
        {
            var altura = (historico.Checked ? 284 : 192) * pagina.DeviceDpi / 96;
            cartoes.SuspendLayout();
            cartoes.SetCellPosition(ram, new TableLayoutPanelCellPosition(0, 1));
            cartoes.SetColumnSpan(cpu, 2);
            cartoes.SetColumnSpan(ram, 2);
            cartoes.RowStyles[0].SizeType = SizeType.Absolute; cartoes.RowStyles[0].Height = altura;
            cartoes.RowStyles[1].Height = altura;
            cartoes.Height = altura * 2;
            cartoes.ResumeLayout();
        }
        cartoes.SizeChanged += (_, _) => OrganizarCartoes();
        pagina.DpiChangedAfterParent += (_, _) => OrganizarCartoes();
        cpu.ExibirHistorico(false); ram.ExibirHistorico(false);
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
        pagina.Controls.Add(status, 0, 2);
        pagina.Layout += (_, _) =>
        {
            var minimo = pagina.Padding.Vertical + acoes.Height + acoes.Margin.Vertical +
                cartoes.Height + status.Height + status.Margin.Vertical;
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
            cpu.Adicionar(usoCpu, usoCpu is double percentual ? $"Em uso {percentual:N0}% · Ocioso {100-percentual:N0}%" : "Leitura indisponível");
            ram.Adicionar(usoRam, usoRam is not null ? $"{EmGiB(usada)} usada · {EmGiB(total - usada)} disponível · {EmGiB(total)} total" : "Memória física indisponível");
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
            var cabecalho = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 3, RowCount = 2 };
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cabecalho.RowStyles.Add(new RowStyle(SizeType.AutoSize)); cabecalho.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var icone = new AreaDesenho { Size = new Size(36, 36), Anchor = AnchorStyles.Left };
            icone.Paint += (_, e) => DesenharIcone(e.Graphics, icone.ClientRectangle);
            cabecalho.Controls.Add(icone, 0, 0);
            var rotulo = CriarRotulo(nome, 15, true); rotulo.Anchor = AnchorStyles.Left;
            cabecalho.Controls.Add(rotulo, 1, 0);
            leitura = CriarRotulo("—", 27, true);
            leitura.Anchor = AnchorStyles.Right;
            leitura.AccessibleName = nome + ": aguardando amostra";
            cabecalho.Controls.Add(leitura, 2, 0);
            detalhe = CriarRotulo("Aguardando amostra", 9, false);
            detalhe.MinimumSize = new Size(0, detalhe.Font.Height * 2);
            Estilo.Marcar(detalhe, PapelVisual.Secundario);
            cabecalho.Controls.Add(detalhe, 0, 1); cabecalho.SetColumnSpan(detalhe, 3);
            estatisticas = CriarRotulo("Média —  ·  Pico —  ·  0/0 válidas", 9, false);
            estatisticas.Dock = DockStyle.Bottom;
            Estilo.Marcar(estatisticas, PapelVisual.Secundario);
            SizeChanged += (_, _) =>
            {
                var largura = Math.Max(100, ClientSize.Width - Padding.Horizontal - 8);
                detalhe.MaximumSize = leitura.MaximumSize = estatisticas.MaximumSize = new Size(largura, 0);
            };
            DpiChangedAfterParent += (_, _) => detalhe.MinimumSize = new Size(0, detalhe.Font.Height * 2);
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
            var barra = new RectangleF(6 * px, 8 * px, Math.Max(1, trama.Width - 12 * px), 18 * px);
            desenho.SmoothingMode = SmoothingMode.AntiAlias;
            using var trilho = FormaModerna.Caminho(Rectangle.Round(barra), (int)(6*px));
            using (var fundo = new SolidBrush(Estilo.Paleta.Borda)) desenho.FillPath(fundo, trilho);
            var atual = serie.Atual;
            if (atual is double valorBarra)
            {
                var fBarra = animacao.Enabled ? Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 240) : 1;
                var visual = anteriorValor is double de ? de + (valorBarra - de) * fBarra : valorBarra;
                using var preenchimento = new SolidBrush(AltoContraste ? SystemColors.Highlight : cor);
                var estado = desenho.Save(); desenho.SetClip(trilho);
                desenho.FillRectangle(preenchimento, barra.X, barra.Y, barra.Width * (float)(visual / 100), barra.Height);
                desenho.Restore(estado);
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
        private void DesenharIcone(Graphics g, Rectangle bounds)
        {
            var px = DeviceDpi / 96f;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var forma = FormaModerna.Caminho(Rectangle.Inflate(bounds, -1, -1), (int)(7*px));
            using var fundo = new SolidBrush(Estilo.AltoContraste ? Estilo.Paleta.Superficie : Color.FromArgb(30, cor));
            g.FillPath(fundo, forma);
            using var linha = new Pen(cor, 1.5f*px);
            var centro = new PointF(bounds.Width/2f, bounds.Height/2f);
            if (nome == "CPU")
            {
                var chip = new RectangleF(centro.X-7*px, centro.Y-7*px, 14*px, 14*px);
                g.DrawRectangle(linha, chip.X, chip.Y, chip.Width, chip.Height);
                for (var i=-1;i<=1;i++)
                {
                    var d=i*4*px;
                    g.DrawLine(linha, centro.X+d, chip.Top-4*px, centro.X+d, chip.Top);
                    g.DrawLine(linha, centro.X+d, chip.Bottom, centro.X+d, chip.Bottom+4*px);
                    g.DrawLine(linha, chip.Left-4*px, centro.Y+d, chip.Left, centro.Y+d);
                    g.DrawLine(linha, chip.Right, centro.Y+d, chip.Right+4*px, centro.Y+d);
                }
            }
            else
            {
                g.DrawRectangle(linha, centro.X-12*px, centro.Y-6*px, 24*px, 12*px);
                for(var i=-1;i<=1;i++) g.DrawRectangle(linha, centro.X+(i*7-2)*px, centro.Y-3*px, 4*px, 6*px);
                for(var i=-2;i<=2;i++) g.DrawLine(linha, centro.X+i*4*px, centro.Y+6*px, centro.X+i*4*px, centro.Y+9*px);
            }
        }
        protected override void Dispose(bool disposing) { if (disposing) animacao.Dispose(); base.Dispose(disposing); }
    }

    private sealed class AreaDesenho : Panel
    {
        public AreaDesenho() => DoubleBuffered = true;
    }
}
