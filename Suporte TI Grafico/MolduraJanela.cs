using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuporteTI.Grafico;

/// <summary>Personaliza o acabamento sem substituir a área não cliente do Windows.</summary>
internal static class MolduraJanela
{
    private sealed class Estado { public bool Ativa; }
    private static readonly ConditionalWeakTable<Form, Estado> Janelas = new();
    private const int CorPadrao = -1; // DWMWA_COLOR_DEFAULT

    public static void Aplicar(Form janela)
    {
        if (!Janelas.TryGetValue(janela, out var estado))
        {
            estado = new Estado { Ativa = janela.ContainsFocus };
            Janelas.Add(janela, estado);
            janela.HandleCreated += (_, _) => Atualizar(janela, estado);
            janela.Activated += (_, _) => { estado.Ativa = true; Atualizar(janela, estado); };
            janela.Deactivate += (_, _) => { estado.Ativa = false; Atualizar(janela, estado); };
            janela.DpiChanged += (_, _) => Atualizar(janela, estado);
        }
        Atualizar(janela, estado);
    }

    private static void Atualizar(Form janela, Estado estado)
    {
        if (!janela.IsHandleCreated || janela.IsDisposed) return;
        Definir(janela, 20, !Estilo.AltoContraste && Estilo.Escuro ? 1 : 0);
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)) return;
        var p = Estilo.Paleta;
        // Alto contraste: remover as cores explícitas, inclusive após trocar de tema.
        Definir(janela, 33, Estilo.AltoContraste ? 0 : 2);
        Definir(janela, 34, Estilo.AltoContraste ? CorPadrao : Cor(estado.Ativa ? p.Borda : p.Navegacao));
        Definir(janela, 35, Estilo.AltoContraste ? CorPadrao : Cor(p.Fundo));
        Definir(janela, 36, Estilo.AltoContraste ? CorPadrao : Cor(estado.Ativa ? p.Texto : p.Secundario));
    }

    private static int Cor(Color cor) => cor.R | (cor.G << 8) | (cor.B << 16);

    private static void Definir(Form janela, int atributo, int valor)
    {
        // A composição e os atributos disponíveis dependem do Windows. Uma recusa
        // conserva a moldura nativa; nunca muda o estilo ou os limites da janela.
        _ = DwmSetWindowAttribute(janela.Handle, atributo, ref valor, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr janela, int atributo, ref int valor, int tamanho);
}
