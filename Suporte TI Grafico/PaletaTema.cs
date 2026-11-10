using System.Drawing;

namespace SuporteTI.Grafico;

internal sealed record PaletaTema(Color Fundo, Color Navegacao, Color Superficie, Color Alternada, Color Borda,
    Color Destaque, Color NoDestaque, Color Texto, Color Secundario, Color Link, Color Cpu, Color Ram, Color Alerta)
{
    private static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
    public static PaletaTema Escuro { get; } = new(C(32,34,39), C(24,26,31), C(43,46,52), C(48,51,58), C(79,85,96),
        C(112,184,255), C(10,30,50), C(245,247,250), C(191,199,212), C(136,202,255), C(112,184,255), C(111,222,190), C(255,185,130));
    public static PaletaTema Claro { get; } = new(C(242,245,249), C(231,236,243), Color.White, C(245,247,251), C(168,179,194),
        C(12,91,183), Color.White, C(27,35,48), C(72,85,104), C(0,81,171), C(12,91,183), C(0,108,83), C(153,68,12));
}
