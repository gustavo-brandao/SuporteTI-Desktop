namespace SuporteTI.Grafico;

internal sealed record LinhaResultado(string Secao, Campo Campo, bool Aviso);

internal static class LinhasResultado
{
    public static IEnumerable<LinhaResultado> Filtrar(Resposta resposta, string filtro)
    {
        var termo = filtro.Trim();
        foreach (var secao in resposta.Secoes)
        {
            var secaoCorresponde = secao.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase);
            foreach (var campo in secao.Campos)
                if (secaoCorresponde || campo.Nome.Contains(termo, StringComparison.CurrentCultureIgnoreCase) ||
                    campo.Valor.Contains(termo, StringComparison.CurrentCultureIgnoreCase))
                    yield return new LinhaResultado(secao.Nome, campo, false);
            // Avisos de leitura parcial não desaparecem com o filtro.
            if (!string.IsNullOrWhiteSpace(secao.Aviso))
                yield return new LinhaResultado(secao.Nome, new Campo("Aviso", secao.Aviso), true);
        }
    }
}
