namespace SuporteTI.Grafico;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Estilo.Inicializar();
        using var icone = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        using var janela = new JanelaPrincipal(new ConsultaPowerShell()) { Icon = icone };
        Application.Run(janela);
    }
}
