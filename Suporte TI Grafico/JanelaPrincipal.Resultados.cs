namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private readonly System.Windows.Forms.Timer atrasoFiltro = new() { Interval = 200 };

    private void AgendarFiltro()
    {
        atrasoFiltro.Stop();
        if (resultado is null) return;
        if (filtro.Text.Length == 0) AtualizarGrade();
        else
        {
            estado.Text = "Filtrando resultados…";
            atrasoFiltro.Start();
        }
    }

    private void AtualizarGrade()
    {
        atrasoFiltro.Stop();
        grade.Columns[3].Visible = resultado?.Operacao is "discos" or "diskhealth";
        salvar.Enabled = resultado is not null;
        copiarResultado.Enabled = resultado is not null;
        salvar.Visible = copiarResultado.Visible = resultado is not null;
        var mostrarResumo = resultado?.Operacao is "discos" or "diskhealth";
        resumoDiscos.Visible = mostrarResumo;
        resumoDiscos.Text = mostrarResumo ? ResumoDiscos.Criar(resultado!) : "";
        feedbackCopia.Text = "";
        grade.SuspendLayout();
        var tamanhoAutomatico = grade.AutoSizeRowsMode;
        grade.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        try
        {
            grade.Rows.Clear();
            if (resultado is null) return;
            var linhas = new List<DataGridViewRow>();
            foreach (var item in LinhasResultado.Filtrar(resultado, filtro.Text))
            {
                var campo = item.Campo;
                var indicador = campo.Percentual is double percentual ? (object)percentual :
                    campo.Metrica is "ocupacao" or "desgaste" ? "Não informado" : "";
                var linha = (DataGridViewRow)grade.RowTemplate.Clone();
                linha.CreateCells(grade, item.Secao, campo.Nome, campo.Valor, indicador);
                linha.Cells[2].ToolTipText = campo.Valor;
                linha.Cells[3].Tag = campo.Metrica;
                linha.Cells[3].ToolTipText = campo.Metrica == "ocupacao" ? "Percentual de espaço ocupado no volume" :
                    "Percentual usado da vida útil estimada. A saúde também depende dos demais indicadores.";
                if (campo.Familia is "Win32" or "HRESULT" or "Excecoes" or "Paradas")
                {
                    linha.Tag = campo;
                    var cor = Estilo.Paleta.Link;
                    linha.Cells[2] = new DataGridViewLinkCell
                    {
                        Value = campo.Valor, LinkColor = cor, ActiveLinkColor = cor, TrackVisitedState = false,
                        ToolTipText = "Consultar o significado deste código"
                    };
                }
                linhas.Add(linha);
            }
            if (linhas.Count > 0) grade.Rows.AddRange(linhas.ToArray());
            grade.ClearSelection();
        }
        finally
        {
            grade.AutoSizeRowsMode = tamanhoAutomatico;
            grade.ResumeLayout();
        }
        estado.Text = resultado.Estado switch
        {
            "concluido" when grade.Rows.Count == 0 => "Nenhum resultado corresponde ao filtro.",
            "concluido" => "Consulta concluída.",
            "parcial" => "Consulta parcial. Confira os avisos na lista.",
            _ => "Dados indisponíveis. Confira os avisos na lista."
        };
    }

    private static void DesenharIndicadorDisco(object? remetente, DataGridViewCellPaintingEventArgs evento)
    {
        if (evento.RowIndex < 0 || evento.ColumnIndex != 3 || evento.Value is not double percentual ||
            evento.Graphics is null || evento.CellStyle is not { } estilo)
            return;
        evento.PaintBackground(evento.ClipBounds, true);
        var area = Rectangle.Inflate(evento.CellBounds, -8, -5);
        if (area.Width > 0 && area.Height > 0)
        {
            var metrica = (remetente as DataGridView)?.Rows[evento.RowIndex].Cells[3].Tag as string;
            var nome = metrica == "ocupacao" ? "Ocupação" : metrica == "desgaste" ? "Desgaste" : "Indicador";
            var trilha = new Rectangle(area.X, area.Bottom - 7, area.Width, 7);
            using var fundo = new SolidBrush(AltoContraste ? SystemColors.ControlDark : Borda);
            using var usado = new SolidBrush(AltoContraste ? SystemColors.Highlight :
                percentual >= (metrica == "desgaste" ? 80 : 90) ? Estilo.Paleta.Alerta : Estilo.Paleta.Cpu);
            evento.Graphics.FillRectangle(fundo, trilha);
            evento.Graphics.FillRectangle(usado, trilha.X, trilha.Y, (int)(trilha.Width * Math.Clamp(percentual, 0, 100) / 100), trilha.Height);
            var rotulo = new Rectangle(area.X, area.Y, area.Width, Math.Max(0, area.Height - 9));
            var corTexto = (evento.State & DataGridViewElementStates.Selected) != 0
                ? estilo.SelectionForeColor : estilo.ForeColor;
            TextRenderer.DrawText(evento.Graphics, $"{nome} {percentual:N0}%", estilo.Font, rotulo, corTexto,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        evento.Handled = true;
    }

}
