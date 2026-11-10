namespace SuporteTI.Grafico;

internal static class ResumoDiscos
{
    public static string Criar(Resposta resposta)
    {
        var campos = resposta.Secoes.SelectMany(secao => secao.Campos).ToArray();
        var volumes = campos.Count(campo => campo.Metrica == "ocupacao");
        var discos = campos.Where(campo => campo.Metrica == "desgaste").ToArray();
        var alertas = discos.Count(campo => campo.EstadoDisco is "Atencao" or "Critico/degradado");
        var inconclusivos = discos.Count(campo => campo.EstadoDisco is not ("Sem alertas detectados" or "Atencao" or "Critico/degradado"));
        var saude = discos.Length == 0 ? "Saúde física indisponível" :
            $"{discos.Length} disco(s) físico(s) avaliados · {alertas} com alerta · {inconclusivos} inconclusivo(s)";
        var leitura = resposta.Estado == "concluido" ? "Leitura concluída" : "Leitura parcial ou indisponível. Confira os avisos.";
        return $"{volumes} volume(s) · {saude}{Environment.NewLine}{leitura}{Environment.NewLine}" +
            "Ocupação mede o espaço usado. Desgaste mede o uso da vida útil estimada, quando informado pelo disco.";
    }
}
