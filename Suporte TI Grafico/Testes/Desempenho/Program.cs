using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var assembly = Assembly.LoadFrom(Path.GetFullPath(args[0]));
        Type Tipo(string nome) => assembly.GetType("SuporteTI.Grafico." + nome, true)!;
        object Novo(string nome, params object?[] parametros) => Activator.CreateInstance(Tipo(nome),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, parametros, null)!;
        var tipoJanela = Tipo("JanelaPrincipal");
        var privado = BindingFlags.Instance | BindingFlags.NonPublic;
        var amostras = new List<object>();
        for (var repeticao = 1; repeticao <= 3; repeticao++)
        {
            var tempo = Stopwatch.StartNew();
            using var janela = (Form)Novo("JanelaPrincipal", Novo("ConsultaPowerShell"));
            janela.Show();
            Application.DoEvents();
            var abertura = tempo.Elapsed.TotalMilliseconds;
            T Campo<T>(string nome) => (T)tipoJanela.GetField(nome, privado)!.GetValue(janela)!;
            void Atribuir(string nome, object valor) => tipoJanela.GetField(nome, privado)!.SetValue(janela, valor);
            void Atualizar() { tipoJanela.GetMethod("AtualizarGrade", privado)!.Invoke(janela, null); Application.DoEvents(); }
            var grade = Campo<DataGridView>("grade");
            var filtro = Campo<TextBox>("filtro");
            var campos = Array.CreateInstance(Tipo("Campo"), 1000);
            for (var i = 0; i < campos.Length; i++)
                campos.SetValue(Novo("Campo", "Serviço " + i, $"Descrição de teste çã 漢字 · {i} · " + new string('x', 100), null, null, null, null), i);
            var secoes = Array.CreateInstance(Tipo("Secao"), 1);
            secoes.SetValue(Novo("Secao", "Serviços de teste", campos, "Leitura parcial de demonstração"), 0);
            Atribuir("resultado", Novo("Resposta", 1, "servicos", "parcial", secoes));
            tempo.Restart();
            Atualizar();
            var preencher = tempo.Elapsed.TotalMilliseconds;
            if (grade.Rows.Count != 1001) throw new InvalidOperationException("A medição perdeu linhas ou aviso.");
            tempo.Restart();
            foreach (var termo in new[] { "S", "Se", "Ser", "Serv", "Servi", "Serviço 99" }) filtro.Text = termo;
            // Dar ao filtro diferido até 2 s para aplicar a última tecla.
            var limite = Stopwatch.StartNew();
            while (grade.Rows.Count != 12 && limite.ElapsedMilliseconds < 2000)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            var filtrar = tempo.Elapsed.TotalMilliseconds;
            if (grade.Rows.Count != 12) throw new InvalidOperationException("O filtro final não preservou as 11 correspondências e o aviso.");
            var abrir = Campo<Action<string>>("abrirFerramenta");
            var catalogo = (IEnumerable)Tipo("CatalogoFerramentas").GetProperty("Itens")!.GetValue(null)!;
            var ids = catalogo.Cast<object>().Select(item => (string)item.GetType().GetProperty("Id")!.GetValue(item)!).ToArray();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            using var processo = Process.GetCurrentProcess();
            processo.Refresh();
            var memoriaAntes = processo.PrivateMemorySize64;
            var handlesAntes = processo.HandleCount;
            tempo.Restart();
            foreach (var id in ids) { abrir(id); Application.DoEvents(); }
            var navegacao = tempo.Elapsed.TotalMilliseconds;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            processo.Refresh();
            amostras.Add(new { repeticao, aberturaJanelaMs = abertura, preencher1000Ms = preencher,
                seisTeclasMs = filtrar, navegar44Ms = navegacao, memoriaAntesBytes = memoriaAntes,
                memoriaDepoisBytes = processo.PrivateMemorySize64, handlesAntes, handlesDepois = processo.HandleCount });
            janela.Close();
        }
        Console.WriteLine(JsonSerializer.Serialize(new { versao = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion,
            ambiente = "Windows x64; mesma estação e resolução; dados sintéticos; três repetições no mesmo processo", amostras },
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
