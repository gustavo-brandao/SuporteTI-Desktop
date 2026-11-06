using System.Runtime.InteropServices;

namespace SuporteTI.Grafico;

internal sealed class MonitorRecursos
{
    private (ulong Ocioso, ulong Kernel, ulong Usuario)? anterior;

    public (double? Cpu, double? Ram, ulong Total, ulong Usada) Ler()
    {
        double? cpu = null;
        if (GetSystemTimes(out var ocioso, out var kernel, out var usuario))
        {
            if (anterior is { } ultimo)
                cpu = CalcularCpu(ultimo, (ocioso, kernel, usuario));
            anterior = (ocioso, kernel, usuario);
        }
        else
            anterior = null;

        var memoria = new Memoria { Tamanho = (uint)Marshal.SizeOf<Memoria>() };
        if (!GlobalMemoryStatusEx(ref memoria) || memoria.TotalFisica == 0 || memoria.DisponivelFisica > memoria.TotalFisica)
            return (cpu, null, 0, 0);
        var usada = memoria.TotalFisica - memoria.DisponivelFisica;
        return (cpu, 100.0 * usada / memoria.TotalFisica, memoria.TotalFisica, usada);
    }

    internal static double? CalcularCpu((ulong Ocioso, ulong Kernel, ulong Usuario) anterior,
        (ulong Ocioso, ulong Kernel, ulong Usuario) atual)
    {
        if (atual.Ocioso < anterior.Ocioso || atual.Kernel < anterior.Kernel || atual.Usuario < anterior.Usuario)
            return null;
        var total = (double)(atual.Kernel - anterior.Kernel) + (atual.Usuario - anterior.Usuario);
        var ocioso = atual.Ocioso - anterior.Ocioso;
        return total > 0 && ocioso <= total ? 100 * (1 - ocioso / total) : null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Memoria
    {
        public uint Tamanho, Carga;
        public ulong TotalFisica, DisponivelFisica, TotalPaginacao, DisponivelPaginacao,
            TotalVirtual, DisponivelVirtual, DisponivelEstendida;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out ulong ocioso, out ulong kernel, out ulong usuario);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref Memoria memoria);
}
