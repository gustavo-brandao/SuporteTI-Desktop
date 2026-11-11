using System.Globalization;

namespace SuporteTI.Grafico;

internal static class MedidasVisuais
{
    public static double? Percentual(double? valor) => valor is double n && double.IsFinite(n) && n >= 0 && n <= 100 ? n : null;
    public static double? Taxa(double? valor) => valor is double n && double.IsFinite(n) && n >= 0 ? n : null;

    // A escala só cresce durante a execução; uma queda de vazão não muda o significado do arco.
    public static double Escala(double atual, double? taxa)
    {
        var escala = double.IsFinite(atual) && atual >= 100 ? atual : 100;
        if (Taxa(taxa) is not double n || n <= escala) return escala;
        foreach (var limite in new double[] { 250, 500, 1000, 2500, 5000, 10000 })
            if (n <= limite) return Math.Max(escala, limite);
        return Math.Max(escala, n);
    }

    public static double? LerTaxa(string? texto) => texto is not null &&
        double.TryParse(texto.Replace("Mbps", "").Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var n) ? Taxa(n) : null;
}
