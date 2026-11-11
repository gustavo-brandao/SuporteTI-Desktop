using System.Drawing;

namespace SuporteTI.Grafico;

internal sealed record PaletaTema(Color Fundo, Color Navegacao, Color Superficie, Color Alternada, Color Borda,
    Color Destaque, Color NoDestaque, Color Texto, Color Secundario, Color Link, Color Cpu, Color Ram, Color Alerta)
{
    private static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
    public Color NavegacaoAtiva => Color.FromArgb((Navegacao.R*7+Destaque.R)/8, (Navegacao.G*7+Destaque.G)/8, (Navegacao.B*7+Destaque.B)/8);
    public static PaletaTema Escuro { get; } = new(C(18,19,23), C(15,16,20), C(25,27,32), C(32,35,42), C(49,53,62),
        C(132,165,255), C(17,25,44), C(238,240,245), C(164,173,190), C(159,191,255), C(132,165,255), C(99,204,173), C(244,183,113));
    public static PaletaTema Claro { get; } = new(C(240,241,244), C(232,234,239), Color.White, C(247,248,250), C(209,213,221),
        C(49,83,169), Color.White, C(30,33,42), C(82,91,108), C(40,77,166), C(49,83,169), C(0,108,83), C(153,68,12));
}
