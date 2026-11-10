namespace SuporteTI.Grafico;

internal sealed partial class JanelaPrincipal
{
    private static readonly Dictionary<string, string> Impactos = new()
    {
        ["spooler"] = "Reinicia o serviço de impressão e interrompe a impressão por alguns instantes. Os trabalhos da fila são preservados.",
        ["spoolerclear"] = "Cancela os trabalhos da fila local selecionada. Esta ação pode interromper documentos em impressão.",
        ["cache"] = "Recria o cache de download do Windows Update, preserva o cache anterior e restaura os serviços que estavam em execução.",
        ["gpupdateuser"] = "Atualiza as políticas do usuário desta sessão. O Windows pode aplicar novas configurações.",
        ["gpupdatecomputer"] = "Atualiza as políticas do computador. O Windows pode aplicar novas configurações.",
        ["chromecache"] = "Limpa o cache HTTP dos perfis padrão do Chrome nesta sessão. Feche o navegador antes de continuar.",
        ["netdhcp"] = "Ativa IPv4 e DNS automáticos no adaptador selecionado. Pode interromper a conexão e o atendimento remoto.",
        ["netstatic"] = "Substitui IPv4, gateway e DNS no adaptador selecionado. Pode interromper a conexão e o atendimento remoto.",
        ["sfcrepair"] = "Verifica e pode substituir arquivos protegidos do Windows. Mantenha a janela aberta durante o reparo.",
        ["dismrestore"] = "Repara a imagem do Windows e pode baixar componentes. Mantenha a janela aberta durante o reparo.",
        ["chkdskfull"] = "Corrige o sistema de arquivos e procura setores defeituosos. Responda na janela do CHKDSK se o Windows oferecer agendamento. O reinício é manual.",
        ["perfillocal"] = "Examina possíveis perfis locais duplicados ou temporários. Revise a conta, as pastas e as entradas de Registro na janela administrativa antes de confirmar qualquer exclusão.",
        ["autologonenable"] = "O Windows tentará entrar automaticamente com a conta escolhida. Use uma conta dedicada com privilégios limitados. Um administrador local pode recuperar a credencial armazenada.",
        ["autologondisable"] = "Desliga o login automático e remove a credencial usada por ele. No próximo logon, use a autenticação normal."
    };
    private bool preparandoIntervencao;
    private bool intervencaoEmAndamento;

    private async Task<(bool Confirmada, ParametrosIntervencao? Dados)> PrepararIntervencaoAsync(Ferramenta escolhido, string volume)
    {
        preparandoIntervencao = true;
        diagnosticos.Enabled = consultar.Enabled = false;
        HabilitarCamposFluxo(false);
        var estadoAnterior = estado.Text;
        try
        {
            ParametrosIntervencao? dados = null;
            string? alvo = escolhido.Id == "chkdskfull" ? volume : null;
            if (escolhido.Id is "netdhcp" or "netstatic" or "spoolerclear")
            {
                var operacao = escolhido.Id == "spoolerclear" ? "filaslocais" : "adaptadores";
                estado.Text = "Carregando opções disponíveis…";
                var resposta = await consulta.ConsultarAsync(operacao, CancellationToken.None);
                var opcoes = resposta.Secoes.SelectMany(secao => secao.Campos).ToArray();
                if (opcoes.Length == 0)
                {
                    DialogoTema.Mostrar(this, resposta.Secoes.FirstOrDefault()?.Aviso ?? "Nenhuma opção disponível.",
                        escolhido.Nome, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return (false, null);
                }
                (dados, alvo) = EscolherAlvoIntervencao(escolhido, opcoes);
                if (dados is null)
                    return (false, null);
            }
            var mensagem = Impactos[escolhido.Id];
            if (alvo is not null)
                mensagem += $"\n\nAlvo: {alvo}";
            if (escolhido.Id == "netstatic" && dados is not null)
                mensagem += $"\nIPv4: {dados.IPv4} · Máscara/prefixo: {dados.Prefixo}\nGateway: {dados.Gateway}\nDNS: {dados.Dns1} {dados.Dns2}";
            var confirmada = DialogoTema.Mostrar(this, mensagem, escolhido.Nome, MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            return (confirmada, dados);
        }
        catch (Exception erro)
        {
            DialogoTema.Mostrar(this, erro.Message, escolhido.Nome, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return (false, null);
        }
        finally
        {
            preparandoIntervencao = false;
            diagnosticos.Enabled = consultar.Enabled = true;
            HabilitarCamposFluxo(true);
            estado.Text = estadoAnterior;
        }
    }

    private (ParametrosIntervencao? Dados, string? Alvo) EscolherAlvoIntervencao(Ferramenta escolhido, Campo[] opcoes)
    {
        var estatico = escolhido.Id == "netstatic";
        var rotuloAlvo = escolhido.Id == "spoolerclear" ? "Fila local" : "Adaptador físico";
        using var janela = new Form
        {
            Text = escolhido.Nome, ClientSize = new Size(590, estatico ? 320 : 130), StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false, ShowInTaskbar = false,
            AutoScaleMode = AutoScaleMode.Dpi, BackColor = BackColor, ForeColor = ForeColor
        };
        var layout = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(12) };
        janela.Icon=Icon;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lista = new SeletorModerno { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, AccessibleName = rotuloAlvo };
        if (!AltoContraste)
        {
            lista.BackColor = Superficie;
            lista.ForeColor = Texto;
        }
        lista.Items.AddRange(opcoes.Select(item => (object)item.Nome).ToArray());
        Adicionar(rotuloAlvo, lista, 0);

        string[] nomes = ["IPv4", "Máscara ou prefixo", "Gateway (opcional)", "DNS primário", "DNS secundário (opcional)"];
        string[] exemplos = ["192.0.2.10", "24 ou 255.255.255.0", "192.0.2.1", "192.0.2.53", "192.0.2.54"];
        var campos = new TextBox[nomes.Length];
        if (estatico)
        {
            for (var i = 0; i < nomes.Length; i++)
            {
                campos[i] = new TextBox { Dock = DockStyle.Top, AccessibleName = nomes[i], PlaceholderText = exemplos[i], MaxLength = 20 };
                if (!AltoContraste)
                {
                    campos[i].BackColor = Superficie;
                    campos[i].ForeColor = Texto;
                }
                Adicionar(nomes[i], campos[i], i + 1);
            }
        }
        var acoes = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var rever = new BotaoModerno { Text = "&Revisar", AutoSize = true, DialogResult = DialogResult.OK, Enabled = false };
        var desistir = new BotaoModerno { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };
        AplicarCorBotao(rever, true);
        AplicarCorBotao(desistir, false);
        void AtualizarBotao() => rever.Enabled = lista.SelectedIndex >= 0 &&
            (!estatico || new[] { 0, 1, 3 }.All(i => !string.IsNullOrWhiteSpace(campos[i].Text)));
        lista.SelectedIndexChanged += (_, _) => AtualizarBotao();
        if (estatico)
            foreach (var campo in campos)
                campo.TextChanged += (_, _) => AtualizarBotao();
        acoes.Controls.AddRange([rever, desistir]);
        layout.Controls.Add(acoes, 0, estatico ? 6 : 1);
        layout.SetColumnSpan(acoes, 2);
        janela.AcceptButton = rever;
        janela.CancelButton = desistir;
        janela.Controls.Add(layout);
        Estilo.Aplicar(janela);
        janela.HandleCreated += (_, _) => Estilo.Aplicar(janela);
        if (janela.ShowDialog(this) != DialogResult.OK)
            return (null, null);
        var alvo = opcoes[lista.SelectedIndex];
        if (!estatico)
            return (new ParametrosIntervencao(alvo.Valor), alvo.Nome);
        var valores = campos.Select(campo => campo.Text.Trim()).ToArray();
        return (new ParametrosIntervencao(alvo.Valor, valores[0], valores[1], valores[2], valores[3], valores[4]), alvo.Nome);

        void Adicionar(string nome, Control controle, int linha)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(CriarRotulo(nome, 10, false), 0, linha);
            layout.Controls.Add(controle, 1, linha);
        }
    }
}
