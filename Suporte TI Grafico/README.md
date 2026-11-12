# Suporte TI Gráfico

**1.0.0-beta.9.8** é uma prévia de homologação para Windows x64, destinada a técnicos de qualquer empresa e a uso pessoal. A interface WinForms usa superfícies neutras e azul de destaque, com temas Escuro, Claro e Sistema. O rodapé mostra a máquina e o nível da sessão atual. [Download e capturas reais](https://github.com/gustavo-brandao/SuporteTI-Desktop/releases/tag/v1.0.0-beta.9.8).

## Monitor de CPU/RAM · etapa 3.1

Hardware → Desempenho → Monitor ao vivo apresenta porcentagens em destaque e resumos de média/pico das leituras válidas da janela de até 60 amostras. Histórico aberto por padrão; recolher as curvas preserva as estatísticas e a coleta. Os cartões ficam lado a lado a partir de 720 pixels efetivos disponíveis e empilhados abaixo disso, com rolagem. A RAM conserva memória usada, disponível e total.

Zero real entra nas estatísticas. Amostras ausentes, inválidas ou não finitas ficam como lacunas, sem entrar na média ou no pico. Pausar ou trocar de ferramenta conserva o último estado; iniciar novamente limpa série e resumos. A coleta segue a cada segundo, sem estimar valores entre amostras. O movimento existente continua apenas visual e respeita as preferências do Windows. A etapa 3.2 acompanha esta prévia integrada.

As duas subdivisões do monitoramento compõem a mesma prévia; a beta.9.7 ficou como revisão intermediária sem release pública.

## Velocímetro e estados · etapa 3.2

O número principal é a taxa provisória da fase em andamento; os cartões Download/Upload só recebem números finais ao concluir. Durante o upload, o cartão Download informa que o resultado será apresentado ao concluir o teste. O painel mostra latência HTTPS quando disponível no resultado. A escala cresce para cobrir as taxas medidas e não encolhe durante a execução; o resultado orienta a agulha pela taxa final de upload disponível, ou pela de download.

Cancelar remove taxa e agulha e identifica interrupção sem resultado final. Falha parcial conserva a direção válida e identifica a outra como indisponível. Atualizações tardias são ignoradas após encerrar. Iniciar outro teste limpa resultados e escala anteriores. O arco fica ao lado dos resumos em área ampla; em largura reduzida, os resumos ficam abaixo e a página rola. O motor, servidor, limites de dados, parâmetros e relatórios permanecem os mesmos. As animações atuais continuam; uma nova linguagem de movimento pertence à etapa 5.

## Estrutura da área de trabalho · etapa 2.1

O cabeçalho superior mostra o grupo e a área atuais, acompanhando guias, busca, Sobre e Configurações. Em larguras menores, a contagem de ferramentas cede espaço ao contexto. A navegação separa **Ferramentas** e **Aplicativo**, com nomes e dicas preservados quando recolhida.

A área de conteúdo fica em uma superfície própria, com guias de fundo contínuo, títulos menores, linhas de resultado mais compactas e bordas neutras. As paletas Escuro/Claro usam tons neutros com azul de destaque; Sistema continua acompanhando o Windows, e alto contraste mantém prioridade. O catálogo continua com 44 operações.

A etapa 2.2 integra as cores do título e da borda à paleta no Windows 11, com texto e borda próprios para janela ativa/inativa. Arraste, tamanho, menu de sistema, Snap e botões permanecem sob controle nativo. No alto contraste, as cores explícitas são removidas; no Windows 10, usa os recursos disponíveis. O acabamento é reaplicado ao recriar o identificador da janela, ativar/desativar, mudar tema ou DPI. O refinamento do monitoramento está nesta prévia; busca Raycast e novos movimentos continuam em etapas posteriores.

## Legibilidade e identidade Desktop

A identificação lateral mostra **VERSÃO DESKTOP**. As guias mantêm a mesma fonte ao selecionar; o fundo e a linha inferior identificam a guia ativa. Isso mantém a medição e o desenho do texto coerentes, corrigindo o recorte em **Windows → Integridade e reparos**. Nomes, atalhos, rolagem nativa e operações permanecem disponíveis.

A direção aprovada para as próximas entregas combina estrutura inspirada no Linear, monitoramento no iStat e busca em camadas no Raycast. A correção de legibilidade e identidade foi entregue na beta.9.4; esta prévia acrescenta a estrutura da área de trabalho, e as demais partes serão implantadas uma de cada vez.

## Navegação, monitor e velocidade

As abas ficam em uma linha, com seleção destacada e foco de teclado. Se não couberem, o TabControl oferece rolagem nativa; Ctrl+Tab/Ctrl+Shift+Tab também navegam entre as guias. A lateral alterna entre nomes e ícones abaixo de 1.000 pixels efetivos da largura cliente, com botão manual, nomes acessíveis e dicas. Ctrl+K expande o menu e leva à busca. A escolha manual dura até mudar a faixa de largura e não altera a consulta ou seus parâmetros. Sobre e Configurações ficam na base; os grupos usam rolagem quando a altura é insuficiente.

Os títulos mostram a área atual sem repetir o menu principal. O tamanho mínimo continua 780 × 560. A página calcula uma altura mínima conforme o cabeçalho, os parâmetros, os comandos e uma área legível de resultados; quando necessário, rola verticalmente. Salvar log e copiar permanecem acessíveis, inclusive após rolar a página.

A navegação recebe realce gradual, e a entrada do conteúdo usa um deslocamento curto de 180 ms. O monitor mantém as amostras nativas de CPU e memória a cada segundo; o movimento entre pontos é apenas apresentação, sem alterar as leituras. Trocar de ferramenta pausa o monitor e interrompe suas animações. A preferência de animações do Windows é respeitada; alto contraste e sessões remotas usam apresentação sem movimento decorativo.

Esta prévia reúne as subdivisões de monitoramento da etapa 3. O monitor apresenta barras horizontais de CPU/RAM em escala fixa de 0–100%, com memória usada, disponível e total. **Mostrar histórico de 60 amostras**, ativado por padrão, recolhe ou revela as curvas sem interromper a coleta. Pausar conserva os valores; iniciar novamente limpa o histórico. Leituras ausentes não são mostradas como zero. A redistribuição completa dos menus segue no planejamento; as imagens conceituais continuam separadas das capturas reais.

Em **Rede → Conectividade → Velocidade da internet**, o motor C# mede três amostras de latência HTTPS e executa quatro transferências paralelas por direção, por até dez segundos cada. O limite é **512 MiB de download e 512 MiB de upload**; ao alcançá-lo, a coleta termina antes. Em condições normais, espere cerca de 20–35 segundos. A fase, a taxa em Mbps, o tempo e os bytes são atualizados durante o teste. O velocímetro mostra a vazão média acumulada da fase. A escala começa em 100 Mbps e aumenta quando necessário, sem diminuir durante a execução; seu limite fica escrito abaixo do arco. A linha inferior indica o progresso da fase, separadamente da taxa. Ao concluir, download e upload permanecem identificados, inclusive quando uma direção estiver indisponível. Cancelar remove a agulha e a leitura em andamento sem apresentar uma medição incompleta como resultado final.

A taxa de upload considera blocos confirmados pelo servidor, evitando confundir a escrita no buffer com velocidade da internet. O resultado informa duração, dados válidos, encerramento e método; **Salvar log…** e **Copiar resultado** preservam os avisos. Cancelar interrompe as transferências; a navegação permanece disponível. O resultado é uma estimativa da vazão útil até a Cloudflare, sujeita a Wi-Fi, VPN, proxy, servidor e tráfego concorrente. Não equivale à garantia da velocidade contratada. O teste usa HTTPS sem enviar arquivos pessoais e só inicia mediante clique.

Este motor é específico da interface gráfica. Os coletores PowerShell de referência permanecem preservados; PowerShell 7 não foi incluído nesta versão.

## Temas, títulos e guias

A primeira abertura usa **Escuro**, independentemente do tema do Windows. **Claro** aplica a paleta clara. **Sistema** acompanha a preferência de aplicativos do Windows e recebe suas alterações durante a sessão. Alto contraste tem prioridade; a escolha permanece guardada para quando ele for desativado.

A preferência fica em `%LOCALAPPDATA%\SuporteTI-Desktop\aparencia.json`. A gravação usa arquivo temporário na mesma pasta e substituição atômica. Se o arquivo estiver inválido ou inacessível, a tela informa o uso de Escuro. Se não puder salvar, o tema escolhido continua aplicado na sessão e a tela informa a limitação. O executável não precisa gravar configurações em sua própria pasta.

Navegação, guias, seletores, tabelas, gráficos, menus e confirmações do aplicativo acompanham a paleta. A troca preserva o resultado e os campos escolhidos. Os títulos mantêm versão e ícone do aplicativo; o Windows desenha a moldura e os botões da janela. Os diálogos administrativos recebem o tema efetivo ao abrir, inclusive quando a elevação usa outra conta. O ícone é validado e incluído na cópia protegida.

Pastas, UAC, console e outros diálogos do Windows podem seguir o visual nativo do sistema. Cantos e barra de título usam os recursos disponíveis do Windows, com fundo sólido como alternativa. Windows 10, mudanças reais de alto contraste e a matriz completa de DPI ainda aguardam homologação.

## Usar a prévia

A beta.3 normaliza o desgaste dos discos para número ou valor indisponível antes de enviar os resultados à interface. O plano de reorganização está em [Revisão de UX e diagnósticos](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/docs/REVISAO-UX-2026-11-09.md).

1. Extraia todo o ZIP em uma pasta local ou no pendrive.
2. Abra **Suporte TI Grafico.exe**, identificado pelo ícone de um monitor com um prompt de terminal.
3. Em **Visão geral**, use **Consultar computador** para ler o resumo do sistema.
4. Em **Hardware**, escolha a aba **Desempenho**, **Discos**, **Bateria**, **Dispositivos**, **Serviços** ou **Programas** e selecione a ferramenta. **Discos** reúne a saúde administrativa, os volumes, o mapa e os modos de CHKDSK. O indicador **(UAC)** identifica as ações que exigem elevação.
5. Em **Hardware → Desempenho → Monitor ao vivo**, inicie ou pause a amostragem de CPU e RAM. Em **Hardware → Discos → Mapa de disco**, analise um volume e navegue por pastas. Trocar de ferramenta pausa o monitor.
6. Em **Rede**, use as abas **Conexão**, **Conectividade** e **Configuração IPv4**. Na configuração, escolha o **Modo** automático (DHCP) ou manual. **Ver adaptadores** consulta os endereços atuais; a alteração exige **Revisar ação…** e seleção explícita do adaptador.
7. Em **Windows → Eventos e códigos**, escolha **Ler eventos do Windows** e a **Leitura** de falhas ou eventos recentes. A pesquisa de código continua separada. Clicar em um código destacado preenche a pesquisa e abre a explicação. As abas **Windows Update** e **Integridade e reparos** reúnem seus respectivos diagnósticos e ações.
8. Em **Impressão**, escolha a **Ação**: diagnosticar, reiniciar o spooler ou limpar uma fila local. Ao preparar manutenção, **Diagnosticar impressão** permite consultar primeiro. A limpeza pede a fila local que será afetada.
9. Em **Contas e políticas → Políticas de grupo**, escolha o **Escopo** usuário da sessão ou computador e a **Ação** consultar, exportar HTML ou atualizar. A consulta contextual respeita o escopo escolhido. **Perfis locais** e **Login automático** mantêm suas próprias abas.
10. Em **Relatórios**, recupere o último resultado ou exporte eventos em EVTX com manifesto SHA-256. **Salvar log…** continua disponível nos resultados das consultas e exporta também os dados ocultos pelo filtro.
11. Digite parte do nome na **Buscar ferramenta**. A busca aceita palavras sem acento. Use duplo clique no resultado ou seta para baixo e Enter para abrir. Encontrar uma ferramenta apenas abre sua tela. A execução exige a ação do técnico.
12. Use **Sobre** para conferir a versão e os créditos.

A saúde dos discos apresenta barras quando existe uma medição numérica. Uma leitura limitada ou indisponível conserva seu aviso. O formulário principal utiliza as permissões da sessão e solicita o UAC por operação.

## Formulários por contexto

Desde a beta.7, as 44 operações são agrupadas em 35 entradas no seletor. GPO, IPv4, eventos e impressão usam campos próprios em vez de repetir todas as combinações na lista. O texto **Selecionado:** confirma a operação efetiva e identifica o UAC. A busca continua encontrando cada operação pelo nome ou ID e preenche os campos automaticamente. Trocar um campo não executa ações.

Os atalhos **Ver adaptadores**, **Diagnosticar impressão** e **Consultar políticas do escopo** executam somente a consulta indicada; esta última pode pedir UAC no escopo computador. **Pesquisar código de erro** abre o campo para preenchimento. Resultados parciais, salvamento de logs, cópia e confirmações das intervenções permanecem disponíveis.

## Legibilidade e discos

Desde a beta.6, a interface usa Segoe UI como fonte base de 10 pt, rótulos permanentes nos campos e linhas de tabela mais espaçadas. As instruções acompanham a largura da janela. A ação de cancelar aparece durante consultas que admitem cancelamento, e os botões de relatório aparecem após haver um resultado.

Em **Hardware → Discos**, o resumo separa volumes, discos físicos avaliados, alertas e leituras inconclusivas. Os indicadores mostram **Ocupação** ou **Desgaste** junto do percentual. Uma medição de desgaste zero, por si só, não classifica a saúde do disco. Dados indisponíveis e avisos de leitura parcial continuam visíveis.

## Desempenho e encerramento de consultas

Desde a beta.8, a interface preenche as tabelas em lote e ajusta a altura das linhas conforme entram na área visível. O filtro espera 200 ms sem novas teclas antes de aplicar a pesquisa; limpar o campo restaura o resultado imediatamente. Todos os registros continuam disponíveis ao rolar a tabela. Os avisos permanecem visíveis mesmo quando não correspondem ao filtro. A cópia completa e o log conservam todos os dados recebidos.

As consultas não elevadas usam um grupo de processos próprio do Windows, encerrado ao concluir, cancelar ou atingir o prazo. O processo principal é recolhido antes de liberar outra consulta. Saída acima de 16 Mi caracteres no canal padrão ou 64 Ki caracteres no canal de erro é recusada com mensagem de falha. A elevação e as intervenções administrativas mantêm seus fluxos anteriores.

O [relatório de desempenho](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/docs/DESEMPENHO-beta8.md) registra as três amostras por cenário, seus limites e as pendências de homologação. Leituras de saúde e ocupação continuam sendo consultadas novamente; esta versão não adiciona cache de diagnósticos.

## Copiar resultados

A coluna **Copiar** copia o valor completo da linha. O clique em um código de erro continua abrindo sua explicação. Para copiar o código, use o botão da mesma linha.

Selecione uma ou mais linhas e pressione **Ctrl+C**, ou use o menu de contexto com **Copiar valor**, **Copiar linhas selecionadas** e **Copiar resultado completo**. O botão **Copiar resultado** também copia o relatório completo com horários, estado e avisos, inclusive os dados ocultos pelo filtro.

O rodapé mostra **Copiado.** após a gravação. Se a área de transferência estiver ocupada, a janela informa a falha e permite tentar novamente. Selecionar uma linha mantém o conteúdo anterior da área de transferência.

## Intervenções de suporte

- Em **Impressão**, **Reiniciar spooler** preserva os trabalhos. **Limpar fila de impressão** pede uma fila local e cancela os trabalhos encontrados nela. O resultado informa quantos foram cancelados e quantos falharam.
- Em **Windows → Windows Update**, **Recriar cache do Windows Update** renomeia o cache de download, preserva a cópia anterior e tenta restaurar o estado dos serviços. Reinício pendente e manutenção ativa bloqueiam a operação.
- Em **Contas e políticas → Políticas de grupo**, escolha a consulta, exportação ou atualização para usuário ou computador.
- Em **Hardware → Programas**, **Limpar cache do Chrome** limpa o cache HTTP de perfis padrão da sessão. Feche o Chrome antes. Preferências e outros dados fora da pasta Cache são preservados.
- Em **Rede**, escolha DHCP ou IPv4 fixo e selecione o adaptador físico. O resultado mostra o estado antes e depois. Se uma etapa falhar, confira as alterações já aplicadas. Alterar a conexão pode interromper o suporte remoto.

Cada intervenção exibe seu impacto e pede confirmação. A limpeza do Chrome e a atualização de GPO do usuário executam na sessão atual. As ações de spooler, Windows Update, GPO do computador e IPv4 solicitam UAC por operação.

## Reparos e alterações de conta

Na página da operação, escolha a ferramenta, use **Revisar ação…** e confira o impacto antes de confirmar o UAC.

- **SFC · reparar arquivos** usa `/scannow`. **DISM · reparar imagem** usa `/RestoreHealth /NoRestart`. O resultado registra a saída e o código retornado pelo Windows. Aguarde o término.
- **CHKDSK · reparo completo** aplica `/f /r` ao volume fixo selecionado. A janela nativa pode oferecer agendamento para o próximo reinício. Responda nela. A tela gráfica registra o código de saída e indica onde encontrar o log do Windows. Reinicie manualmente se tiver aceitado agendar.
- **Revisar perfis locais duplicados** abre os candidatos na janela elevada, com conta, SID, pastas, entradas do Registro e bloqueios. A exclusão exige confirmação separada e nova conferência do perfil. Encerre a sessão da conta alvo e confira a cópia dos dados antes de autorizar.
- **Configurar login automático** recebe a senha apenas na janela elevada e armazena a credencial como segredo LSA. Use conta dedicada de privilégios limitados. **Desativar login automático** desliga a configuração e remove a credencial. Verifique o próximo logon manualmente.

## Acesso administrativo

Abra o executável normalmente. A janela usa as permissões da sessão e as consultas comuns ficam disponíveis sem elevação. Ao executar uma ferramenta identificada com **(UAC)** ou uma intervenção que exija permissão, o aplicativo inicia um processo separado pelo UAC. Use o próprio diálogo do Windows para autorizar ou cancelar. Uma conta sem direitos administrativos pode precisar informar credenciais de uma conta autorizada. O aplicativo recebe o resultado pelo canal local e salva o HTML no destino escolhido com as permissões da sessão original.

As consultas administrativas desta prévia verificam o estado do Windows e dos discos, e os comandos nativos podem registrar informações nos logs do sistema. As verificações podem levar até 30 minutos. Depois de autorizar o UAC, aguarde o resultado na janela.

Antes da execução administrativa, a sessão original compara os arquivos com as versões embutidas no executável e envia os dados verificados para o processo elevado. Ele grava a cópia em uma pasta protegida em `Windows\Temp`, preserva a marca de origem, confere os hashes e remove a pasta ao final. Para usar o modo administrativo por compartilhamento, abra pelo caminho UNC com scripts administrativos assinados e empacotados depois da assinatura. A política de execução do Windows continua aplicável.

A raiz do ZIP contém o executável e as pastas **Modulos** e **Documentacao**. Mantenha esses itens juntos ao mover o aplicativo. O executável inclui o .NET Desktop Runtime x64. O .NET extrai os componentes nativos para o cache local do usuário ao iniciar. A consulta usa o Windows PowerShell 5.1 da própria máquina, roda localmente na identidade da sessão, em modo de leitura e sem pedir elevação. Os resultados ficam na janela.

O pacote local desta beta está sem assinatura. O processo do PowerShell usa RemoteSigned, sujeito à política do Windows. Caso a origem do ZIP seja marcada como Internet, verifique sua procedência e o SHA-256 antes de seguir o procedimento de autorização do ambiente. Para testar lado a lado, extraia esta beta e a 0.7.0 em pastas separadas e confira a versão no título antes de operar.

A análise do mapa pode ser cancelada e mostra resultados parciais quando encontra itens inacessíveis, pontos de reanálise ou limites de leitura. Os limites são 350 mil entradas, 90 segundos e 64 níveis. O total lido pode diferir da ocupação do volume.

A saúde dos discos usa os indicadores do Windows e o smartmontools 7.5 para até quatro discos internos compatíveis. Indicadores indisponíveis ficam sinalizados. O inventário de programas usa o Registro. A bateria depende das capacidades publicadas pelo Windows. A consulta de login automático lê apenas o estado e a presença de configuração legada.

O smartmontools é distribuído em `Modulos/Ferramentas/smartmontools` com seu código-fonte e licença própria. O executável e a base de discos são conferidos por SHA-256 antes de cada leitura.

## Construir e testar

O [planejamento de evolução](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/docs/PLANEJAMENTO.md) registra as etapas do dashboard após a base adaptável da beta.9.2 e a avaliação de PowerShell 7 na beta.10. Esta versão continua usando Windows PowerShell 5.1 para suas rotinas. A documentação distingue testes aprovados, simulações e pendências de laboratório.

Em Windows x64 com o SDK .NET 10 disponível, execute a partir desta pasta.

```powershell
dotnet run --project .\Testes\SuporteTI.Grafico.Testes.csproj -c Release
.\Construcao\Publicar.ps1
```

O publicador gera o ZIP e o SHA-256 em `outputs`. Use `-Dotnet` para indicar um SDK portátil ou `-Destino` para escolher uma pasta nova. Acrescente `-- --exigir-cim` ao comando de testes em uma estação com acesso a CIM para conferir nome e build reais.

O roteiro **HOMOLOGACAO.md**, incluído em **Documentacao**, descreve os ambientes, os cenários em VM e a geração de um pacote assinado. [Roteiro versionado no repositório](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/Suporte%20TI%20Grafico/Testes/HOMOLOGACAO.md)

## Origem

Desenvolvido por Gustavo Brandão Gimenez, sob a [licença MIT](LICENSE). A referência funcional é o Suporte TI para terminal 0.1.22 no commit `1e3e0109e8135a5b3a80fdf0f0a1b1dab7f6962e`. O projeto gráfico mantém sua versão, fontes e pacote próprios.
