using System.Reflection;
using System.Text;

namespace SuporteTI.Grafico;

internal static class LogConsulta
{
    public static string Salvar(string destino, string consulta, Resposta resposta, DateTimeOffset inicio, DateTimeOffset fim)
    {
        var maquina = Environment.MachineName;
        var pasta = CriarPasta(destino, fim);
        var caminho = Path.Combine(pasta, "resultado.txt");
        var versao = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
        using var arquivo = new FileStream(caminho, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var texto = new StreamWriter(arquivo, new UTF8Encoding(true));
        texto.WriteLine($"SUPORTE TI GRÁFICO - v{versao}");
        texto.WriteLine($"Computador: {maquina}");
        texto.WriteLine($"Consulta: {consulta}");
        texto.WriteLine($"Início: {inicio:O}");
        texto.WriteLine($"Fim: {fim:O}");
        texto.WriteLine($"Resultado: {resposta.Estado}");
        foreach (var secao in resposta.Secoes)
        {
            texto.WriteLine();
            texto.WriteLine($"--- {secao.Nome} ---");
            foreach (var campo in secao.Campos)
                texto.WriteLine($"{campo.Nome}: {campo.Valor}");
            if (!string.IsNullOrWhiteSpace(secao.Aviso))
                texto.WriteLine($"Aviso: {secao.Aviso}");
        }
        return caminho;
    }

    public static string SalvarHtmlComputador(string destino, byte[] html, DateTimeOffset fim)
    {
        var caminho = Path.Combine(CriarPasta(destino, fim), "gpresult-computer.html");
        using var arquivo = new FileStream(caminho, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        arquivo.Write(html);
        return caminho;
    }

    private static string CriarPasta(string destino, DateTimeOffset fim)
    {
        if (!Path.IsPathFullyQualified(destino))
            throw new ArgumentException("Escolha uma pasta com caminho absoluto.", nameof(destino));
        var pasta = Path.Combine(destino, Environment.MachineName, $"{fim:yyyy-MM-dd_HHmmss}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(pasta);
        return pasta;
    }
}
