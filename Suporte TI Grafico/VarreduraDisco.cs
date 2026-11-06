using System.Diagnostics;

namespace SuporteTI.Grafico;

internal sealed record ItemDisco(string Nome, long Bytes, ItemDisco[]? Filhos);
internal sealed record ResultadoDisco(ItemDisco Raiz, int Ignorados, string? Limite);

internal static class VarreduraDisco
{
    // A politica de leitura segue VarreduraOcupacao da edicao de terminal.
    public static ResultadoDisco Ler(string caminho, CancellationToken token, int maxItens = 350_000, int maxSegundos = 90)
    {
        var tempo = Stopwatch.StartNew();
        var itens = 0;
        var ignorados = 0;
        string? limite = null;
        bool AtingiuLimite()
        {
            token.ThrowIfCancellationRequested();
            limite = itens >= maxItens ? "limite de itens atingido"
                : tempo.Elapsed.TotalSeconds >= maxSegundos ? "limite de tempo atingido" : null;
            return limite is not null;
        }

        ItemDisco LerPasta(DirectoryInfo pasta, int profundidade)
        {
            var filhos = new List<ItemDisco>();
            try
            {
                if (profundidade >= 64 || pasta.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    ignorados++;
                else
                {
                    foreach (var entrada in pasta.EnumerateFileSystemInfos())
                    {
                        if (AtingiuLimite())
                            break;
                        itens++;
                        try
                        {
                            if (entrada.Attributes.HasFlag(FileAttributes.ReparsePoint))
                                ignorados++;
                            else
                                filhos.Add(entrada is DirectoryInfo subpasta ? LerPasta(subpasta, profundidade + 1)
                                    : new ItemDisco(entrada.Name, ((FileInfo)entrada).Length, null));
                        }
                        catch (Exception erro) when (erro is IOException or UnauthorizedAccessException) { ignorados++; }
                        if (limite is not null)
                            break;
                    }
                }
            }
            catch (Exception erro) when (erro is IOException or UnauthorizedAccessException) { ignorados++; }
            return new ItemDisco(pasta.Name, filhos.Sum(item => item.Bytes),
                filhos.OrderByDescending(item => item.Bytes).ThenBy(item => item.Nome, StringComparer.CurrentCultureIgnoreCase).ToArray());
        }

        token.ThrowIfCancellationRequested();
        var raiz = LerPasta(new DirectoryInfo(caminho), 0);
        token.ThrowIfCancellationRequested();
        return new ResultadoDisco(raiz, ignorados, limite);
    }
}
