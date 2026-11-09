using System.Globalization;

namespace SuporteTI.Grafico;

internal sealed record Ferramenta(string Id, string Nome, string Grupo, string Area)
{
    public override string ToString() => ElevacaoConsulta.EhAdministrativa(Id) ? Nome + " (UAC)" : Nome;
}

internal static class CatalogoFerramentas
{
    public static IReadOnlyList<string> Grupos { get; } = Array.AsReadOnly(new[]
        { "Visão geral", "Hardware", "Rede", "Windows", "Impressão", "Contas e políticas", "Relatórios" });

    public static IReadOnlyList<Ferramenta> Itens { get; } = Array.AsReadOnly(new Ferramenta[]
    {
        new("sistema", "Resumo do sistema", "Visão geral", "Computador"),
        new("desempenho", "CPU, memória e processos", "Hardware", "Desempenho"),
        new("monitor", "Monitor ao vivo", "Hardware", "Desempenho"),
        new("diskhealth", "Saúde dos discos", "Hardware", "Discos"),
        new("discos", "Volumes e saúde dos discos", "Hardware", "Discos"),
        new("mapaocupacao", "Mapa de disco", "Hardware", "Discos"),
        new("chkdskread", "CHKDSK · consultar volume", "Hardware", "Discos"),
        new("chkdskfull", "CHKDSK · reparo completo", "Hardware", "Discos"),
        new("bateria", "Saúde da bateria", "Hardware", "Bateria"),
        new("dispositivos", "Problemas de dispositivos", "Hardware", "Dispositivos"),
        new("servicos", "Serviços locais", "Hardware", "Serviços"),
        new("programas", "Programas instalados", "Hardware", "Programas"),
        new("chromecache", "Limpar cache do Chrome", "Hardware", "Programas"),
        new("rede", "Adaptadores e endereços", "Rede", "Conexão"),
        new("dominio", "Domínio e grupo de trabalho", "Rede", "Conexão"),
        new("ping", "Ping ICMP", "Rede", "Conectividade"),
        new("velocidade", "Velocidade da internet", "Rede", "Conectividade"),
        new("netdhcp", "Ativar IPv4 automático (DHCP)", "Rede", "Configuração IPv4"),
        new("netstatic", "Configurar IPv4 fixo", "Rede", "Configuração IPv4"),
        new("falhas", "Falhas recentes", "Windows", "Eventos e códigos"),
        new("codigofalha", "Consultar código de erro", "Windows", "Eventos e códigos"),
        new("eventos", "Eventos recentes", "Windows", "Eventos e códigos"),
        new("update", "Diagnóstico do Windows Update", "Windows", "Windows Update"),
        new("cache", "Recriar cache do Windows Update", "Windows", "Windows Update"),
        new("sfcverify", "SFC · verificar arquivos", "Windows", "Integridade e reparos"),
        new("dismcheck", "DISM · CheckHealth", "Windows", "Integridade e reparos"),
        new("dismscan", "DISM · ScanHealth", "Windows", "Integridade e reparos"),
        new("sfcrepair", "SFC · reparar arquivos", "Windows", "Integridade e reparos"),
        new("dismrestore", "DISM · reparar imagem", "Windows", "Integridade e reparos"),
        new("filaimpressao", "Diagnóstico de impressão", "Impressão", "Filas e spooler"),
        new("spooler", "Reiniciar spooler", "Impressão", "Filas e spooler"),
        new("spoolerclear", "Limpar fila de impressão", "Impressão", "Filas e spooler"),
        new("gpusuario", "Políticas do usuário (GPO)", "Contas e políticas", "Políticas de grupo"),
        new("gpcomputador", "Políticas do computador (GPO)", "Contas e políticas", "Políticas de grupo"),
        new("gphtml", "Exportar GPO do usuário em HTML", "Contas e políticas", "Políticas de grupo"),
        new("gpcomputerhtml", "Exportar GPO do computador", "Contas e políticas", "Políticas de grupo"),
        new("gpupdateuser", "Atualizar GPO do usuário", "Contas e políticas", "Políticas de grupo"),
        new("gpupdatecomputer", "Atualizar GPO do computador", "Contas e políticas", "Políticas de grupo"),
        new("perfillocal", "Revisar perfis locais duplicados", "Contas e políticas", "Perfis locais"),
        new("autologon", "Estado do login automático", "Contas e políticas", "Login automático"),
        new("autologonenable", "Configurar login automático", "Contas e políticas", "Login automático"),
        new("autologondisable", "Desativar login automático", "Contas e políticas", "Login automático"),
        new("ultimo", "Último resultado", "Relatórios", "Resultados"),
        new("evtx", "Exportar eventos em EVTX", "Relatórios", "Resultados")
    });

    public static Ferramenta[] Buscar(string termo)
    {
        var palavras = termo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return Itens.Where(item => palavras.All(palavra =>
            CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                $"{item.Grupo} {item.Area} {item.Nome} {item.Id}", palavra,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)).ToArray();
    }
}
