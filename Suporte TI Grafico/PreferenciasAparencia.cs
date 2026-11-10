using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuporteTI.Grafico;

internal enum ModoTema { Escuro, Claro, Sistema }
internal sealed record PreferenciaAparencia(int Versao, ModoTema Tema);

internal static class PreferenciasAparencia
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter<ModoTema>(allowIntegerValues: false) }
    };

    public static string CaminhoPadrao
    {
        get
        {
            // Diretório opcional para testes e cópias isoladas; nunca necessário para uso comum.
            var isolado = Environment.GetEnvironmentVariable("SUPORTETI_CONFIG_DIR");
            var pasta = !string.IsNullOrWhiteSpace(isolado) && Path.IsPathFullyQualified(isolado) ? isolado :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SuporteTI-Desktop");
            return Path.Combine(pasta, "aparencia.json");
        }
    }

    public static (ModoTema Tema, string? Aviso) Ler(string caminho)
    {
        try
        {
            if (!File.Exists(caminho)) return (ModoTema.Escuro, null);
            using var arquivo = File.OpenRead(caminho);
            if (arquivo.Length > 16384) throw new InvalidDataException();
            var dados = JsonSerializer.Deserialize<PreferenciaAparencia>(arquivo, Json);
            if (dados is not { Versao: 1 } || !Enum.IsDefined(dados.Tema)) throw new InvalidDataException();
            return (dados.Tema, null);
        }
        catch (Exception erro) when (erro is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
        {
            return (ModoTema.Escuro, "Não foi possível ler a preferência anterior. Usando Escuro.");
        }
    }

    public static void Salvar(string caminho, ModoTema tema)
    {
        if (!Enum.IsDefined(tema)) throw new ArgumentOutOfRangeException(nameof(tema));
        var pasta = Path.GetDirectoryName(Path.GetFullPath(caminho))!;
        Directory.CreateDirectory(pasta);
        var temporario = Path.Combine(pasta, "aparencia-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllText(temporario, JsonSerializer.Serialize(new PreferenciaAparencia(1, tema), Json));
            File.Move(temporario, caminho, overwrite: true);
        }
        finally { if (File.Exists(temporario)) File.Delete(temporario); }
    }

    public static bool UsarEscuro(ModoTema escolha, bool sistemaEscuro) => escolha switch
    {
        ModoTema.Escuro => true, ModoTema.Claro => false, ModoTema.Sistema => sistemaEscuro,
        _ => throw new ArgumentOutOfRangeException(nameof(escolha))
    };
}
