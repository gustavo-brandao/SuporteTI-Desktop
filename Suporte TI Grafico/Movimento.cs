using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SuporteTI.Grafico;

internal static class Movimento
{
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Consultar(uint acao, uint parametro, [MarshalAs(UnmanagedType.Bool)] out bool valor, uint flags);

    // Apenas leitura. Nunca altera a preferência de acessibilidade do Windows.
    public static bool Permitido => !Estilo.AltoContraste && !SystemInformation.TerminalServerSession &&
        Consultar(0x1042, 0, out var habilitado, 0) && habilitado;
    public static double Suavizar(double fracao) => 1 - Math.Pow(1 - Math.Clamp(fracao, 0, 1), 3);
}

internal sealed class TransicaoEntrada : IDisposable
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Stopwatch relogio = new();
    private Control? alvo;
    private Padding original;
    public TransicaoEntrada() => timer.Tick += (_, _) => Atualizar();
    public void Iniciar(Control controle)
    {
        Parar();
        if (!controle.Visible || !Movimento.Permitido) return;
        alvo = controle; original = controle.Padding;
        relogio.Restart(); timer.Start(); Atualizar();
    }
    private void Atualizar()
    {
        if (alvo is null || alvo.IsDisposed || !alvo.Visible || !Movimento.Permitido || relogio.ElapsedMilliseconds >= 180)
        { Parar(); return; }
        var deslocamento = (int)Math.Round(10 * alvo.DeviceDpi / 96.0 * (1 - Movimento.Suavizar(relogio.Elapsed.TotalMilliseconds / 180)));
        alvo.Padding = new Padding(original.Left, original.Top + deslocamento, original.Right, original.Bottom);
    }
    private void Parar()
    {
        timer.Stop();
        if (alvo is { IsDisposed: false }) alvo.Padding = original;
        alvo = null;
    }
    public void Dispose() { Parar(); timer.Dispose(); }
}
