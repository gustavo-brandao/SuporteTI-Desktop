namespace SuporteTI.Grafico;

/// <summary>Janela da sessão: lacunas ocupam tempo, mas não entram nas estatísticas.</summary>
internal sealed class SerieMonitor
{
    private readonly Queue<double?> pontos = new();
    public IReadOnlyCollection<double?> Pontos => pontos.ToArray();
    public double? Atual => pontos.LastOrDefault();
    public int Quantidade => pontos.Count;
    public (int Validas, double? Media, double? Pico) Resumo
    {
        get
        {
            var validas = pontos.OfType<double>().ToArray();
            return validas.Length == 0 ? (0, null, null) : (validas.Length, validas.Average(), validas.Max());
        }
    }
    public void Adicionar(double? valor)
    {
        if (pontos.Count == 60) pontos.Dequeue();
        pontos.Enqueue(MedidasVisuais.Percentual(valor));
    }
    public void Limpar() => pontos.Clear();
}
