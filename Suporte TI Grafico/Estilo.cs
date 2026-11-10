using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuporteTI.Grafico;

internal enum PapelVisual { Normal, Secundario, Superficie, Navegacao, Ativo, Primario }

internal static class Estilo
{
    private sealed class Papel { public Papel() { } public PapelVisual Valor; }
    private static readonly ConditionalWeakTable<Control, Papel> Papeis = new();
    private static bool inicializado;
    private static bool sistemaEscuro;
    private static SystemColorMode? modoAplicado;
    private static PaletaTema paleta = PaletaTema.Escuro;
    public static ModoTema Escolha { get; private set; } = ModoTema.Escuro;
    public static string? Aviso { get; private set; }
    public static bool AltoContraste => SystemInformation.HighContrast;
    public static bool Escuro => PreferenciasAparencia.UsarEscuro(Escolha, sistemaEscuro);
    public static PaletaTema Paleta => paleta;

    public static void Inicializar()
    {
        if (inicializado) return;
        inicializado = true;
        (Escolha, Aviso) = PreferenciasAparencia.Ler(PreferenciasAparencia.CaminhoPadrao);
        AtualizarModoNativo();
    }

    public static void Escolher(ModoTema tema)
    {
        Escolha = tema;
        try { PreferenciasAparencia.Salvar(PreferenciasAparencia.CaminhoPadrao, tema); Aviso = null; }
        catch (Exception erro) when (erro is IOException or UnauthorizedAccessException)
        { Aviso = "Tema aplicado nesta sessão. Não foi possível salvar a preferência."; }
        AtualizarJanelas();
    }

    public static void AtualizarJanelas()
    {
        AtualizarModoNativo();
        foreach (var janela in Application.OpenForms.Cast<Form>().ToArray()) Aplicar(janela);
    }

    private static void AtualizarModoNativo()
    {
        sistemaEscuro=SistemaEscuro();
        var modo=!AltoContraste && Escuro && OperatingSystem.IsWindowsVersionAtLeast(10,0,22000)?SystemColorMode.Dark:SystemColorMode.Classic;
        if(modoAplicado!=modo) {Application.SetColorMode(modo);modoAplicado=modo;}
        paleta=AltoContraste?new(SystemColors.Window,SystemColors.Control,SystemColors.Window,SystemColors.Window,SystemColors.WindowText,
            SystemColors.Highlight,SystemColors.HighlightText,SystemColors.WindowText,SystemColors.WindowText,SystemColors.HotTrack,
            SystemColors.Highlight,SystemColors.Highlight,SystemColors.WindowText):Escuro?PaletaTema.Escuro:PaletaTema.Claro;
    }

    private static bool SistemaEscuro()
    {
        if (Escolha != ModoTema.Sistema) return Escolha == ModoTema.Escuro;
        try { return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int valor && valor == 0; }
        catch (Exception erro) when (erro is System.Security.SecurityException or IOException or UnauthorizedAccessException) { return false; }
    }

    public static void Marcar(Control controle, PapelVisual papel)
    {
        Papeis.GetOrCreateValue(controle).Valor = papel;
        Aplicar(controle);
    }

    public static PapelVisual PapelDe(Control controle) => Papeis.TryGetValue(controle, out var papel) ? papel.Valor : PapelVisual.Normal;

    public static void Aplicar(Control controle)
    {
        var p = Paleta;
        var papel = PapelDe(controle);
        var fundo = papel is PapelVisual.Navegacao or PapelVisual.Ativo ? p.Navegacao :
            papel == PapelVisual.Superficie || controle is Cartao ? p.Superficie : controle.Parent?.BackColor ?? p.Fundo;
        if (controle is Form or TabPage) fundo = p.Fundo;
        if (controle is TextBoxBase or ComboBox or ListBox or DataGridView) fundo=p.Superficie;
        if (controle is ToolStrip) fundo=p.Navegacao;
        if (controle is Button) fundo=papel is PapelVisual.Primario or PapelVisual.Ativo?p.Destaque:
            papel==PapelVisual.Navegacao?p.Navegacao:p.Superficie;
        controle.BackColor = fundo;
        controle.ForeColor = papel == PapelVisual.Secundario ? p.Secundario : p.Texto;
        switch (controle)
        {
            case Button botao:
                botao.UseVisualStyleBackColor = false;
                botao.FlatStyle = FlatStyle.Flat;
                botao.FlatAppearance.BorderSize = 0;
                botao.FlatAppearance.BorderColor = p.Borda;
                botao.BackColor = papel == PapelVisual.Primario || papel == PapelVisual.Ativo ? p.Destaque :
                    papel == PapelVisual.Navegacao ? p.Navegacao : p.Superficie;
                botao.ForeColor = papel is PapelVisual.Primario or PapelVisual.Ativo ? p.NoDestaque : p.Texto;
                break;
            case TextBoxBase or ComboBox or ListBox:
                controle.BackColor = p.Superficie; controle.ForeColor = p.Texto;
                break;
            case DataGridView grade:
                grade.BackgroundColor = p.Superficie; grade.GridColor = p.Borda;
                grade.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;
                grade.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
                grade.EnableHeadersVisualStyles = false; grade.BorderStyle = BorderStyle.None;
                grade.DefaultCellStyle.BackColor = p.Superficie;
                grade.DefaultCellStyle.ForeColor = p.Texto;
                grade.DefaultCellStyle.SelectionBackColor = p.Destaque;
                grade.DefaultCellStyle.SelectionForeColor = p.NoDestaque;
                grade.AlternatingRowsDefaultCellStyle.BackColor = p.Alternada;
                grade.ColumnHeadersDefaultCellStyle.BackColor = p.Navegacao;
                grade.ColumnHeadersDefaultCellStyle.ForeColor = p.Secundario;
                grade.ColumnHeadersDefaultCellStyle.SelectionBackColor = p.Navegacao;
                grade.ColumnHeadersDefaultCellStyle.SelectionForeColor = p.Texto;
                foreach (DataGridViewRow linha in grade.Rows)
                    foreach (var link in linha.Cells.OfType<DataGridViewLinkCell>())
                    { link.LinkColor = link.ActiveLinkColor = link.VisitedLinkColor = linha.Selected ? p.NoDestaque : p.Link; }
                break;
            case ToolStrip faixa:
                faixa.BackColor = p.Navegacao; faixa.ForeColor = p.Texto;
                if (AltoContraste) faixa.RenderMode = ToolStripRenderMode.System;
                else faixa.Renderer = new ToolStripProfessionalRenderer(new CoresMenu());
                foreach (ToolStripItem item in faixa.Items) { item.BackColor = p.Navegacao; item.ForeColor = p.Texto; }
                break;
        }
        if (controle.ContextMenuStrip is { } contexto) Aplicar(contexto);
        foreach (Control filho in controle.Controls) Aplicar(filho);
        if (controle is Form formulario && formulario.IsHandleCreated)
        {
            var escuro = !AltoContraste && Escuro ? 1 : 0;
            _ = DwmSetWindowAttribute(formulario.Handle, 20, ref escuro, 4);
            var cantos = 2;
            _ = DwmSetWindowAttribute(formulario.Handle, 33, ref cantos, 4);
        }
        controle.Invalidate();
    }

    private sealed class CoresMenu : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Paleta.Superficie;
        public override Color ImageMarginGradientBegin => Paleta.Superficie;
        public override Color ImageMarginGradientMiddle => Paleta.Superficie;
        public override Color ImageMarginGradientEnd => Paleta.Superficie;
        public override Color MenuItemSelected => Paleta.Alternada;
        public override Color MenuItemBorder => Paleta.Destaque;
        public override Color MenuBorder => Paleta.Borda;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr janela, int atributo, ref int valor, int tamanho);
}
