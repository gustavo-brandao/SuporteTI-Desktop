namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private CancellationTokenSource? varredura;

    private TableLayoutPanel CriarTelaMapa()
    {
        var pagina = CriarPagina();
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
        pagina.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pagina.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pagina.Controls.Add(CriarRotulo("Mapa de ocupação", 20, true), 0, 0);
        var acoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var unidades = new SeletorModerno { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100, AccessibleName = "Volume local" };
        var analisar = new BotaoModerno { Text = "&Analisar volume", AutoSize = true };
        var interromper = new BotaoModerno { Text = "Ca&ncelar leitura", AutoSize = true, Enabled = false };
        var subir = new BotaoModerno { Text = "&Pasta anterior", AutoSize = true, Enabled = false, AccessibleDescription = "Volta um nível no caminho do mapa." };
        AplicarCorBotao(analisar, true);
        AplicarCorBotao(interromper, false);
        AplicarCorBotao(subir, false);
        if (!AltoContraste)
        {
            unidades.BackColor = Superficie;
            unidades.ForeColor = Texto;
        }
        acoes.Controls.AddRange([CriarRotulo("Volume", 10, false), unidades, analisar, interromper, subir]);
        pagina.Controls.Add(acoes, 0, 1);

        var capacidade = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
        var capacidadeTexto = CriarRotulo("Selecione um volume local.", 10, false);
        var barra = new AreaDesenho { Dock = DockStyle.Top, Height = 9, Margin = new Padding(3, 0, 3, 3) };
        double? ocupacao = null;
        barra.Paint += (_, evento) =>
        {
            if (ocupacao is not double fracao)
                return;
            using var fundo = new SolidBrush(AltoContraste ? SystemColors.ControlDark : Borda);
            using var usado = new SolidBrush(AltoContraste ? SystemColors.Highlight : Estilo.Paleta.Cpu);
            evento.Graphics.FillRectangle(fundo, barra.ClientRectangle);
            evento.Graphics.FillRectangle(usado, 0, 0, (int)(barra.Width * fracao), barra.Height);
        };
        barra.Resize += (_, _) => barra.Invalidate();
        capacidade.Controls.Add(capacidadeTexto, 0, 0);
        capacidade.Controls.Add(barra, 0, 1);
        pagina.Controls.Add(capacidade, 0, 2);

        var caminho = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
        pagina.Controls.Add(caminho, 0, 3);
        var painel = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill };
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var mapa = new AreaDesenho { Dock = DockStyle.Fill, BackColor = AltoContraste ? SystemColors.Window : Superficie };
        var lista = new ListaMapa
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, RowHeadersVisible = false, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells, AccessibleName = "Itens da pasta"
        };
        EstilizarGrade(lista);
        lista.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Item", FillWeight = 55, SortMode = DataGridViewColumnSortMode.NotSortable });
        lista.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tamanho", FillWeight = 28, SortMode = DataGridViewColumnSortMode.NotSortable });
        lista.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% da pasta", FillWeight = 24, SortMode = DataGridViewColumnSortMode.NotSortable });
        painel.Controls.Add(mapa, 0, 0);
        painel.Controls.Add(lista, 1, 0);
        pagina.Controls.Add(painel, 0, 4);
        var rodape = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Fill };
        var status = CriarRotulo("Escolha um volume e inicie a leitura.", 10, false);
        var contexto = CriarRotulo("Na lista, Enter abre a pasta e Backspace sobe um nível.", 10, false);
        rodape.Controls.Add(status, 0, 0);
        rodape.Controls.Add(contexto, 0, 1);
        pagina.Controls.Add(rodape, 0, 5);

        var navegacao = new List<ItemDisco>();
        var blocos = new List<(ItemDisco? Item, Rectangle Area, long Bytes)>();
        ItemDisco? selecao = null;

        void Exibir()
        {
            lista.Rows.Clear();
            selecao = null;
            while (caminho.Controls.Count > 0)
                caminho.Controls[0].Dispose();
            for (var i = 0; i < navegacao.Count; i++)
            {
                var indice = i;
                var botao = new BotaoModerno { Text = navegacao[i].Nome.Replace("&", "&&"), AutoSize = true, AccessibleName = $"Ir para {navegacao[i].Nome}" };
                AplicarCorBotao(botao, i == navegacao.Count - 1);
                botao.Click += (_, _) => Navegar(indice);
                caminho.Controls.Add(botao);
            }
            if (caminho.Controls.Count > 0)
                caminho.ScrollControlIntoView(caminho.Controls[^1]);
            subir.Enabled = navegacao.Count > 1;
            if (navegacao.LastOrDefault() is { Filhos: { } filhos } pasta)
            {
                foreach (var filho in filhos.Take(250))
                {
                    var nome = filho.Filhos is null ? filho.Nome : $"{filho.Nome}  ›";
                    var percentual = pasta.Bytes > 0 ? $"{(double)filho.Bytes / pasta.Bytes:P1}" : "—";
                    lista.Rows[lista.Rows.Add(nome, FormatarBytes(filho.Bytes), percentual)].Tag = filho;
                }
                contexto.Text = $"{FormatarBytes(pasta.Bytes)} lidos · {Math.Min(250, filhos.Length)} de {filhos.Length} itens na lista · Enter abre a pasta";
            }
            else
                contexto.Text = "Na lista, Enter abre a pasta e Backspace sobe um nível.";
            lista.ClearSelection();
            mapa.Invalidate();
        }

        void Navegar(int indice)
        {
            navegacao.RemoveRange(indice + 1, navegacao.Count - indice - 1);
            Exibir();
            if (lista.Rows.Count > 0)
                lista.Focus();
            else
                subir.Focus();
        }

        void Abrir(ItemDisco? item)
        {
            if (item?.Filhos is null)
                return;
            navegacao.Add(item);
            Navegar(navegacao.Count - 1);
        }

        mapa.Paint += (_, evento) =>
        {
            blocos.Clear();
            if (navegacao.LastOrDefault() is not { Bytes: > 0, Filhos: { } filhos } pasta)
            {
                TextRenderer.DrawText(evento.Graphics, navegacao.Count > 0 ? "Sem itens com tamanho para exibir." : "O mapa aparece após a leitura.",
                    Font, mapa.ClientRectangle, AltoContraste ? SystemColors.WindowText : Secundario,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }
            var itens = filhos.Where(item => item.Bytes > 0).Take(30).Select(item => ((ItemDisco?)item, item.Bytes)).ToList();
            var outros = pasta.Bytes - itens.Sum(item => item.Bytes);
            if (outros > 0)
                itens.Add((null, outros));
            DividirMapa(itens, 0, itens.Count, pasta.Bytes, mapa.ClientRectangle, blocos);
            var indice = 0;
            foreach (var bloco in blocos)
            {
                var ativo = bloco.Item is not null && ReferenceEquals(bloco.Item, selecao);
                var cor = AltoContraste ? (ativo ? SystemColors.Highlight : SystemColors.Window)
                    : ativo ? Destaque : bloco.Item is null ? Navegacao : CoresMapa[indice++ % CoresMapa.Length];
                using var pincel = new SolidBrush(cor);
                using var borda = new Pen(AltoContraste ? SystemColors.WindowText : ativo ? Texto : Fundo, ativo ? 2 : 1);
                evento.Graphics.FillRectangle(pincel, bloco.Area);
                evento.Graphics.DrawRectangle(borda, bloco.Area.X, bloco.Area.Y, bloco.Area.Width - 1, bloco.Area.Height - 1);
                if (bloco.Area.Width >= 62 && bloco.Area.Height >= 24)
                {
                    var tinta = AltoContraste ? (ativo ? SystemColors.HighlightText : SystemColors.WindowText) : ativo ? Estilo.Paleta.NoDestaque : Texto;
                    var rotulo = Rectangle.Inflate(bloco.Area, -5, -4);
                    TextRenderer.DrawText(evento.Graphics, bloco.Item?.Nome ?? "Outros", Font, rotulo, tinta,
                        TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
                    if (bloco.Area.Width >= 100 && bloco.Area.Height >= Font.Height * 2 + 10)
                    {
                        rotulo.Offset(0, Font.Height + 2);
                        TextRenderer.DrawText(evento.Graphics, $"{FormatarBytes(bloco.Bytes)} · {(double)bloco.Bytes / pasta.Bytes:P0}",
                            Font, rotulo, tinta, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
                    }
                }
            }
        };
        mapa.Resize += (_, _) => mapa.Invalidate();
        mapa.MouseClick += (_, evento) =>
        {
            var item = blocos.FirstOrDefault(bloco => bloco.Area.Contains(evento.Location)).Item;
            if (item is null)
                return;
            foreach (DataGridViewRow linha in lista.Rows)
                if (ReferenceEquals(linha.Tag, item))
                {
                    lista.CurrentCell = linha.Cells[0];
                    linha.Selected = true;
                    break;
                }
        };
        mapa.MouseDoubleClick += (_, evento) => Abrir(blocos.FirstOrDefault(bloco => bloco.Area.Contains(evento.Location)).Item);
        lista.SelectionChanged += (_, _) =>
        {
            selecao = lista.SelectedRows.Count == 0 ? null : lista.SelectedRows[0].Tag as ItemDisco;
            mapa.Invalidate();
        };
        lista.CellDoubleClick += (_, evento) => { if (evento.RowIndex >= 0) Abrir(lista.Rows[evento.RowIndex].Tag as ItemDisco); };
        lista.ConfigurarAtalho(tecla =>
        {
            if (tecla == Keys.Enter && (selecao ?? lista.CurrentRow?.Tag as ItemDisco) is { Filhos: not null } pasta)
            {
                Abrir(pasta);
                return true;
            }
            if (tecla == Keys.Back && navegacao.Count > 1)
            {
                Navegar(navegacao.Count - 2);
                return true;
            }
            return false;
        });
        subir.Click += (_, _) => Navegar(navegacao.Count - 2);
        interromper.Click += (_, _) => varredura?.Cancel();
        unidades.SelectedIndexChanged += (_, _) =>
        {
            navegacao.Clear();
            Exibir();
            status.Text = "Pronto para analisar. A leitura pode levar até 90 segundos.";
            AtualizarCapacidade();
        };

        void AtualizarCapacidade()
        {
            ocupacao = null;
            try
            {
                var volume = new DriveInfo((string)unidades.SelectedItem!);
                var total = volume.TotalSize;
                var livre = volume.TotalFreeSpace;
                if (total <= 0)
                    throw new IOException();
                ocupacao = Math.Clamp(1 - (double)livre / total, 0, 1);
                capacidadeTexto.Text = $"Volume · {ocupacao:P0} usado · {FormatarBytes(total - livre)} usados · {FormatarBytes(livre)} livres";
            }
            catch (Exception erro) when (erro is IOException or UnauthorizedAccessException)
            {
                capacidadeTexto.Text = "Capacidade do volume indisponível.";
            }
            barra.Invalidate();
        }

        analisar.Click += async (_, _) =>
        {
            if (unidades.SelectedItem is not string volume || varredura is not null)
                return;
            using var cancelamento = new CancellationTokenSource();
            varredura = cancelamento;
            analisar.Enabled = unidades.Enabled = false;
            interromper.Enabled = true;
            status.Text = "Lendo os itens do volume…";
            try
            {
                var leitura = await Task.Run(() => VarreduraDisco.Ler(volume, cancelamento.Token));
                if (IsDisposed)
                    return;
                navegacao.Clear();
                navegacao.Add(leitura.Raiz);
                Exibir();
                AtualizarCapacidade();
                status.Text = leitura.Ignorados > 0 || leitura.Limite is not null
                    ? $"Análise parcial · {leitura.Ignorados:N0} itens ignorados ou inacessíveis{(leitura.Limite is null ? "" : $" · {leitura.Limite}")}."
                    : "Análise concluída · tamanhos lógicos dos arquivos.";
            }
            catch (OperationCanceledException) { if (!IsDisposed) status.Text = "Leitura cancelada."; }
            catch (Exception erro) when (erro is IOException or UnauthorizedAccessException)
            {
                if (!IsDisposed)
                    status.Text = "Não foi possível ler o volume.";
            }
            finally
            {
                varredura = null;
                if (!IsDisposed)
                {
                    analisar.Enabled = unidades.Enabled = true;
                    interromper.Enabled = false;
                }
            }
        };

        foreach (var disco in DriveInfo.GetDrives().Where(disco => disco.DriveType == DriveType.Fixed))
            unidades.Items.Add(disco.Name);
        if (unidades.Items.Count > 0)
            unidades.SelectedIndex = 0;
        else
        {
            analisar.Enabled = false;
            status.Text = "Nenhum volume local disponível.";
        }
        return pagina;
    }

    private static Color[] CoresMapa => Estilo.Escuro ? [
        Color.FromArgb(38, 74, 136), Color.FromArgb(32, 96, 111), Color.FromArgb(73, 63, 133), Color.FromArgb(36, 81, 110)
    ] : [Color.FromArgb(194,220,248), Color.FromArgb(186,233,223), Color.FromArgb(220,210,245), Color.FromArgb(201,225,239)];

    private sealed class ListaMapa : DataGridView
    {
        private Func<Keys, bool>? acaoDeTecla;

        public void ConfigurarAtalho(Func<Keys, bool> acao) => acaoDeTecla = acao;

        protected override bool ProcessDialogKey(Keys keyData) =>
            acaoDeTecla?.Invoke(keyData) == true || base.ProcessDialogKey(keyData);

        protected override bool ProcessDataGridViewKey(KeyEventArgs e) =>
            acaoDeTecla?.Invoke(e.KeyData) == true || base.ProcessDataGridViewKey(e);
    }

    private static string FormatarBytes(long bytes) => bytes >= 1L << 30 ? EmGiB((ulong)bytes)
        : bytes >= 1L << 20 ? $"{bytes / 1048576.0:N1} MiB"
        : bytes >= 1L << 10 ? $"{bytes / 1024.0:N1} KiB" : $"{bytes:N0} B";

    private static void DividirMapa(List<(ItemDisco? Item, long Bytes)> itens, int inicio, int fim, long total,
        Rectangle area, List<(ItemDisco? Item, Rectangle Area, long Bytes)> blocos)
    {
        if (area.Width < 1 || area.Height < 1)
            return;
        if (fim - inicio == 1)
        {
            blocos.Add((itens[inicio].Item, area, total));
            return;
        }
        var meio = inicio + 1;
        var soma = itens[inicio].Bytes;
        while (meio < fim - 1 && soma + itens[meio].Bytes / 2.0 < total / 2.0)
            soma += itens[meio++].Bytes;
        if (area.Width >= area.Height)
        {
            var largura = (int)Math.Round(area.Width * ((double)soma / total));
            DividirMapa(itens, inicio, meio, soma, new Rectangle(area.X, area.Y, largura, area.Height), blocos);
            DividirMapa(itens, meio, fim, total - soma, new Rectangle(area.X + largura, area.Y, area.Width - largura, area.Height), blocos);
        }
        else
        {
            var altura = (int)Math.Round(area.Height * ((double)soma / total));
            DividirMapa(itens, inicio, meio, soma, new Rectangle(area.X, area.Y, area.Width, altura), blocos);
            DividirMapa(itens, meio, fim, total - soma, new Rectangle(area.X, area.Y + altura, area.Width, area.Height - altura), blocos);
        }
    }
}
