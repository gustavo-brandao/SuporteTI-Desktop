namespace SuporteTI.Grafico;

/// <summary>Separa progresso provisório, resultado final e interrupção da medição.</summary>
internal sealed class EstadoMedicaoVelocidade
{
    public bool Ativo { get; private set; }
    public bool Finalizado { get; private set; }
    public bool PossuiResultado { get; private set; }
    public string Fase { get; private set; } = "Pronto";
    public double? Taxa { get; private set; }
    public double Escala { get; private set; } = 100;
    public double Fracao { get; private set; }
    public string? Download { get; private set; }
    public string? Upload { get; private set; }
    public string? Latencia { get; private set; }

    public void Limpar()
    {
        Ativo = Finalizado = PossuiResultado = false;
        Fase = "Pronto"; Taxa = null; Escala = 100; Fracao = 0;
        Download = Upload = Latencia = null;
    }
    public void Iniciar() { Limpar(); Ativo = true; Fase = "Conectando"; }

    public bool Atualizar(ProgressoVelocidade progresso)
    {
        if (!Ativo || progresso.Etapa is not ("Latência" or "Download" or "Upload")) return false;
        Fase = progresso.Etapa;
        Taxa = Fase == "Latência" ? null : MedidasVisuais.Taxa(progresso.Mbps);
        Escala = MedidasVisuais.Escala(Escala, Taxa);
        Fracao = Math.Clamp(double.IsFinite(progresso.Fracao) ? progresso.Fracao : 0, 0, 1);
        return true;
    }

    public void Concluir(Resposta? resultado)
    {
        Ativo = false; Finalizado = true; PossuiResultado = resultado is not null;
        Fase = resultado is null ? "Interrompido" : resultado.Estado == "concluido" ? "Concluído" : "Com avisos";
        string? Ler(string direcao)
        {
            var texto = TesteVelocidade.ObterTaxa(resultado, direcao);
            return MedidasVisuais.LerTaxa(texto) is not null ? texto : null;
        }
        Download = Ler("Download"); Upload = Ler("Upload");
        Latencia = resultado?.Secoes.FirstOrDefault(s => s.Nome == "Latência HTTPS · Cloudflare")?
            .Campos.FirstOrDefault(c => c.Nome == "Mediana")?.Valor;
        Taxa = MedidasVisuais.LerTaxa(Upload ?? Download);
        Escala = MedidasVisuais.Escala(Escala, Taxa);
        if (PossuiResultado) Fracao = 1;
    }

    public string Situacao(string direcao)
    {
        if (Finalizado)
            return !PossuiResultado ? "Interrompido · sem resultado final" :
                (direcao == "Download" ? Download : Upload) is null ? "Indisponível · veja os avisos" : "Resultado final";
        return Ativo && Fase == direcao ? "Em medição · taxa provisória" :
            Ativo && Fase == "Upload" && direcao == "Download" ? "Resultado ao concluir o teste" : "Aguardando medição";
    }
}
