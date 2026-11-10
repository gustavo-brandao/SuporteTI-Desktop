namespace SuporteTI.Grafico;

internal sealed record OpcaoFluxo(string Id, string Acao, string Escopo = "");

internal sealed record FluxoFerramenta(string Nome, string RotuloAcao, OpcaoFluxo[] Opcoes)
{
    public string Id => Opcoes[0].Id;
    public string[] Escopos => Opcoes.Select(opcao => opcao.Escopo).Distinct().ToArray();
    public OpcaoFluxo[] Acoes(string escopo) => Opcoes.Where(opcao => opcao.Escopo == escopo).ToArray();
    public OpcaoFluxo Resolver(string escopo, string acao) =>
        Opcoes.Single(opcao => opcao.Escopo == escopo && opcao.Acao == acao);
}

internal static class FluxoFerramentas
{
    public static IReadOnlyList<FluxoFerramenta> Itens { get; } = Array.AsReadOnly(new FluxoFerramenta[]
    {
        new("Políticas de grupo (GPO)", "Ação", [
            new("gpusuario", "Consultar políticas", "Usuário da sessão"),
            new("gphtml", "Exportar HTML", "Usuário da sessão"),
            new("gpupdateuser", "Atualizar políticas", "Usuário da sessão"),
            new("gpcomputador", "Consultar políticas", "Computador"),
            new("gpcomputerhtml", "Exportar HTML", "Computador"),
            new("gpupdatecomputer", "Atualizar políticas", "Computador")]),
        new("Configurar IPv4", "Modo", [
            new("netdhcp", "Automático (DHCP)"), new("netstatic", "Manual (IPv4 fixo)")]),
        new("Ler eventos do Windows", "Leitura", [
            new("falhas", "Falhas recentes"), new("eventos", "Eventos recentes")]),
        new("Impressoras e filas", "Ação", [
            new("filaimpressao", "Diagnosticar impressão"),
            new("spooler", "Reiniciar spooler (UAC)"),
            new("spoolerclear", "Limpar fila local (UAC)")])
    });

    public static FluxoFerramenta? Obter(string id) => Itens.SingleOrDefault(fluxo => fluxo.Opcoes.Any(opcao => opcao.Id == id));
    public static string Principal(string id) => Obter(id)?.Id ?? id;

    public static Ferramenta[] Listar(string grupo, string area) => CatalogoFerramentas.Itens
        .Where(item => item.Grupo == grupo && item.Area == area && Principal(item.Id) == item.Id)
        .Select(item => Obter(item.Id) is { } fluxo ? item with { Nome = fluxo.Nome } : item).ToArray();
}
