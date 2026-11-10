using System.Runtime.InteropServices;

namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private readonly Button copiarResultado = new BotaoModerno() { Text = "Copiar &resultado", AutoSize = true, Enabled = false };
    private readonly Label feedbackCopia = new() { AutoSize = true };

    private void ConfigurarCopia()
    {
        grade.MultiSelect = true;
        grade.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
        grade.ConfigurarCopia(CopiarLinhas);
        grade.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Copiar", Text = "Copiar", UseColumnTextForButtonValue = true,
            FlatStyle = FlatStyle.Flat,
            Width = 65, AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        copiarResultado.Click += (_, _) => CopiarResultado();
        AplicarCorBotao(copiarResultado, false);
        feedbackCopia.ForeColor = AltoContraste ? SystemColors.WindowText : Secundario;

        var menu = new ContextMenuStrip();
        DataGridViewRow? alvoContexto = null;
        var valor = menu.Items.Add("Copiar valor");
        valor.Click += (_, _) => CopiarTexto(valor.Tag as string ?? "");
        var linhas = menu.Items.Add("Copiar linhas selecionadas", null, (_, _) => CopiarLinhas());
        var completo = menu.Items.Add("Copiar resultado completo", null, (_, _) => CopiarResultado());
        menu.Opening += (_, _) =>
        {
            valor.Enabled = (alvoContexto ?? grade.CurrentRow)?.Selected == true;
            valor.Tag = Convert.ToString((alvoContexto ?? grade.CurrentRow)?.Cells[2].Value);
            linhas.Enabled = grade.SelectedRows.Count > 0;
            completo.Enabled = resultado is not null;
        };
        menu.Closed += (_, _) => alvoContexto = null;
        grade.CellMouseDown += (_, evento) =>
        {
            if (evento.Button != MouseButtons.Right || evento.RowIndex < 0)
                return;
            var linha = grade.Rows[evento.RowIndex];
            alvoContexto = linha;
            if (!linha.Selected)
            {
                grade.ClearSelection();
                grade.CurrentCell = linha.Cells[2];
            }
            linha.Selected = true;
        };
        grade.ContextMenuStrip = menu;
        grade.Disposed += (_, _) => menu.Dispose();
    }

    private void CopiarValor(DataGridViewRow? linha)
    {
        if (linha is not null)
            CopiarTexto(Convert.ToString(linha.Cells[2].Value) ?? "");
    }

    private void CopiarLinhas()
    {
        var linhas = grade.SelectedRows.Cast<DataGridViewRow>().OrderBy(linha => linha.Index)
            .Select(linha => string.Join("\t", Enumerable.Range(0, 3)
                .Select(indice => Convert.ToString(linha.Cells[indice].Value) ?? "")));
        CopiarTexto(string.Join(Environment.NewLine, linhas));
    }

    private void CopiarResultado()
    {
        if (resultado is not null)
            CopiarTexto(LogConsulta.Formatar(nomeResultado, resultado, inicioResultado, fimResultado));
    }

    private void CopiarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return;
        try
        {
            Clipboard.SetText(texto, TextDataFormat.UnicodeText);
            feedbackCopia.Text = "Copiado.";
        }
        catch (ExternalException)
        {
            feedbackCopia.Text = "Área de transferência ocupada. Tente copiar novamente.";
        }
    }
}
