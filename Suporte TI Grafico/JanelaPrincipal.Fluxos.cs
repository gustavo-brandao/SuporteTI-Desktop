namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private readonly ComboBox escopoFluxo = new SeletorModerno() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, AccessibleName = "Escopo da operação" };
    private readonly ComboBox acaoFluxo = new SeletorModerno() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, AccessibleName = "Opção da operação" };
    private readonly Label selecionadoFluxo = CriarRotulo("", 9, false);
    private readonly Button diagnosticoContextual = new BotaoModerno() { AutoSize = true };
    private FlowLayoutPanel campoEscopo = null!, campoAcao = null!;
    private Ferramenta? ferramentaAtual;
    private Action? atualizarFerramenta;
    private bool organizandoFluxo;

    private void CriarCamposFluxo(FlowLayoutPanel parametros, TableLayoutPanel comandos)
    {
        campoEscopo = CampoRotulado("Escopo", escopoFluxo);
        campoAcao = CampoRotulado("Ação", acaoFluxo);
        parametros.Controls.AddRange([campoEscopo, campoAcao]);
        if (!AltoContraste)
            foreach (var combo in new[] { escopoFluxo, acaoFluxo })
            {
                combo.BackColor = Superficie;
                combo.ForeColor = Texto;
            }
        AplicarCorBotao(diagnosticoContextual, false);
        diagnosticoContextual.Margin = new Padding(3, 4, 3, 4);
        var contexto = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        contexto.Controls.AddRange([selecionadoFluxo, diagnosticoContextual]);
        comandos.Controls.Add(contexto, 0, 3);
        escopoFluxo.SelectedIndexChanged += (_, _) =>
        {
            if (organizandoFluxo || diagnosticos.SelectedItem is not Ferramenta principal ||
                FluxoFerramentas.Obter(principal.Id) is not { } fluxo)
                return;
            var acaoAnterior = acaoFluxo.SelectedItem as string;
            organizandoFluxo = true;
            PreencherAcoes(fluxo, acaoAnterior);
            organizandoFluxo = false;
            AplicarEscolhaFluxo();
        };
        acaoFluxo.SelectedIndexChanged += (_, _) => AplicarEscolhaFluxo();
        diagnosticoContextual.Click += async (_, _) =>
        {
            if (ativa is not null || NavegacaoBloqueada || ferramentaAtual is not { } atual)
                return;
            var id = FluxoFerramentas.Principal(atual.Id) switch
            {
                "gpusuario" => escopoFluxo.SelectedItem as string == "Computador" ? "gpcomputador" : "gpusuario",
                "netdhcp" => "rede",
                "filaimpressao" => "filaimpressao",
                _ => "codigofalha"
            };
            abrirFerramenta?.Invoke(id);
            if (id == "codigofalha") entrada.Focus();
            else await ExecutarConsultaAsync();
        };
    }

    private void ConfigurarFluxo(string? preferido = null)
    {
        if (organizandoFluxo) return;
        organizandoFluxo = true;
        var principal = diagnosticos.SelectedItem as Ferramenta;
        var fluxo = principal is null ? null : FluxoFerramentas.Obter(principal.Id);
        var opcao = fluxo?.Opcoes.FirstOrDefault(item => item.Id == preferido);
        campoEscopo.Visible = fluxo is not null && fluxo.Escopos.Length > 1;
        campoAcao.Visible = fluxo is not null;
        escopoFluxo.Items.Clear();
        acaoFluxo.Items.Clear();
        if (fluxo is not null)
        {
            ((Label)campoAcao.Controls[0]).Text = fluxo.RotuloAcao;
            escopoFluxo.Items.AddRange(fluxo.Escopos);
            escopoFluxo.SelectedItem = opcao?.Escopo ?? fluxo.Escopos[0];
            PreencherAcoes(fluxo, opcao?.Acao);
        }
        organizandoFluxo = false;
        AplicarEscolhaFluxo();
    }

    private void PreencherAcoes(FluxoFerramenta fluxo, string? preferida)
    {
        var acoes = fluxo.Acoes(escopoFluxo.SelectedItem as string ?? "");
        acaoFluxo.Items.Clear();
        acaoFluxo.Items.AddRange(acoes.Select(item => item.Acao).ToArray());
        acaoFluxo.SelectedItem = acoes.Any(item => item.Acao == preferida) ? preferida : acoes[0].Acao;
    }

    private void AplicarEscolhaFluxo()
    {
        if (organizandoFluxo) return;
        var principal = diagnosticos.SelectedItem as Ferramenta;
        var fluxo = principal is null ? null : FluxoFerramentas.Obter(principal.Id);
        var id = fluxo is null ? principal?.Id : fluxo.Resolver(escopoFluxo.SelectedItem as string ?? "", acaoFluxo.SelectedItem as string ?? "").Id;
        ferramentaAtual = CatalogoFerramentas.Itens.SingleOrDefault(item => item.Id == id);
        atualizarFerramenta?.Invoke();
        selecionadoFluxo.Visible = fluxo is not null;
        selecionadoFluxo.Text = ferramentaAtual is null ? "" : "Selecionado: " + ferramentaAtual;
        diagnosticoContextual.Text = fluxo?.Id switch
        {
            "gpusuario" => "Consultar políticas do escopo",
            "netdhcp" => "Ver adaptadores",
            "filaimpressao" => "Diagnosticar impressão",
            _ => "Pesquisar código de erro"
        };
        diagnosticoContextual.Visible = fluxo is not null && (ferramentaAtual?.Id switch
        {
            "gpusuario" or "gpcomputador" or "filaimpressao" => false,
            _ => true
        });
    }

    private void HabilitarCamposFluxo(bool habilitar)
    {
        escopoFluxo.Enabled = acaoFluxo.Enabled = diagnosticoContextual.Enabled = habilitar;
    }
}
