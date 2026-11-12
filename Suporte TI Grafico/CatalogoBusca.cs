using System.Globalization;
using System.Text.RegularExpressions;

namespace SuporteTI.Grafico;

internal static class CatalogoBusca
{
    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions Opcoes = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
    private static readonly Dictionary<string, string> Sinonimos = new()
    {
        ["desempenho"] = "ram processador memoria processos cpu",
        ["monitor"] = "ram processador memoria cpu tempo real monitoramento",
        ["rede"] = "wifi ethernet internet adaptador ip",
        ["velocidade"] = "internet download upload banda latencia",
        ["discos"] = "armazenamento espaco volumes",
        ["mapaocupacao"] = "armazenamento espaco pastas arquivos",
        ["diskhealth"] = "smart armazenamento saude",
        ["codigofalha"] = "erro codigo hresult ntstatus win32",
        ["filaimpressao"] = "impressora fila spooler",
        ["autologon"] = "login automatico autologon"
    };

    private static string[] Palavras(string texto) => Regex.Matches(texto, @"[\p{L}\p{N}]+")
        .Select(m => m.Value).ToArray();

    public static Ferramenta[] Buscar(string? texto, string? grupo = null)
    {
        var consulta = (texto ?? "").Trim();
        var termos = Palavras(consulta);
        return CatalogoFerramentas.Itens.Where(f => grupo is null || f.Grupo == grupo)
            .Where(f =>
            {
                var palavras = Palavras($"{f.Id} {f.Nome} {f.Grupo} {f.Area} {Sinonimos.GetValueOrDefault(f.Id)}");
                return termos.All(t => palavras.Any(p => Comparador.IsPrefix(p, t, Opcoes)));
            })
            .OrderBy(f => consulta.Length == 0 ? 0 : Comparador.Compare(f.Id, consulta, Opcoes) == 0 ? 0 :
                Comparador.Compare(f.Nome, consulta, Opcoes) == 0 ? 1 : Comparador.IsPrefix(f.Nome, consulta, Opcoes) ? 2 : 3)
            .ToArray();
    }

    public static string Resumo(Ferramenta f) => f.Id switch
    {
        "sistema" => "Consulte identificação, Windows, BIOS, CPU e memória do computador.",
        "monitor" => "Acompanhe CPU e memória em tempo real, com histórico e resumos da sessão.",
        "desempenho" => "Faça uma consulta pontual de CPU, memória e processos.",
        "velocidade" => "Meça download, upload e latência HTTPS até a Cloudflare. Pode consumir até 1 GiB.",
        "ping" => "Teste a resposta ICMP de um destino e confira perda e tempos de resposta.",
        "rede" => "Consulte adaptadores, endereços e configuração de rede.",
        "mapaocupacao" => "Explore a ocupação de pastas e arquivos, com análise cancelável.",
        "discos" or "diskhealth" => "Consulte volumes e indicadores de saúde disponíveis, preservando avisos de leitura parcial.",
        "codigofalha" => "Pesquise um código de erro sem confundir sua família com o ID de um evento.",
        "falhas" or "eventos" => "Leia eventos do Windows e abra a explicação dos códigos identificados.",
        "sfcverify" => "Verifique arquivos protegidos com SFC /verifyonly, sem executar reparo.",
        "dismcheck" or "dismscan" => "Verifique a imagem do Windows com o modo de consulta selecionado.",
        "chkdskread" => "Consulte um volume local com o modo de leitura do CHKDSK.",
        "filaimpressao" => "Consulte filas e trabalhos antes de escolher uma manutenção.",
        "ultimo" => "Reabra o último resultado desta sessão. Não há histórico persistente.",
        "autologon" => "Consulte o estado do login automático, sem ler a senha armazenada.",
        _ => $"Prepare {f.Nome.ToLowerInvariant()} e revise os parâmetros no formulário da ferramenta."
    };
}
