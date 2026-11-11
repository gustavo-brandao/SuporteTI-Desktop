# Revisão de acabamento e movimento · 11 de novembro de 2026

Solicitação após a beta.9.3: preservar a direção visual estática e melhorar legibilidade, moldura, busca, animações e transições. Este documento registra o planejamento; os exemplos são propostas para avaliação.

## Itens solicitados e aceites

| Item | Revisão prevista | Critério de aceite |
| --- | --- | --- |
| Windows → Integridade e reparos | Corrigir letras cortadas nas extremidades da guia selecionada, conforme a primeira imagem. Investigar medição em negrito, largura da guia, margens e recorte do TabControl. A imagem confirma o sintoma, mas a causa precisa de reprodução dirigida. | Título completo nos estados normal, selecionado, foco e apontamento. Testar guias longas, temas, janela estreita e DPI 100/125/150%. Não resolver apenas abreviando o nome. |
| Barra de título | Rever faixa superior, contraste da versão, ícone, alinhamento e integração com a janela, conforme a segunda imagem. | Estados ativo/inativo, maximizado/restaurado, arrastar, redimensionar, duplo clique, menu do sistema e Snap preservados. Avaliar alternativa antes de substituir a moldura nativa. |
| Identificação lateral | Trocar **EDIÇÃO GRÁFICA** por **VERSÃO DESKTOP**, conforme a terceira imagem. Revisar consistência dos textos visíveis. | Texto alinhado e legível nos temas e DPI. Navegação recolhida mantém nomes acessíveis e dicas. |
| Busca | Modernizar campo, ícone, foco, superfície dos resultados, seleção e animação de abertura/fechamento. Comparar busca integrada e paleta de comandos com Ctrl+K. | Setas/Enter/Escape, busca vazia/sem resultados, nomes longos e indicação de UAC. Sem atraso na digitação ou execução ao apenas selecionar. |
| Bordas do aplicativo | Unificar raios, espessuras, divisórias, seleção e foco; reduzir aparência de retângulos sobrepostos. | Acabamento coerente entre título, guias, busca, cartões e tabelas; sem recortes em DPI diferentes, com foco e alto contraste distinguíveis. |
| Transições de tela | Rever entrada de área, troca de aba, abertura de detalhes e continuidade do indicador. Interromper corretamente ao navegar rápido. | Sem salto de layout, cintilação, captura sobreposta, clique bloqueado ou acúmulo de movimentos. Preservar resultado e foco. |
| Animações dos gráficos | Melhorar continuidade das barras, histórico e agulha, com desaceleração consistente e retomada pelo novo alvo. | Números representam a amostra atual; interpolação apenas visual, sem inventar leituras ou preencher lacunas. Pausa/cancelamento encerram movimento; telas ocultas não mantêm timers ativos. |

As três capturas recebidas são evidência dos pontos a revisar, não de correções já implementadas.

[Guia recortada](referencias/2026-11-11/guia-integridade-recortada.png) · [Barra de título](referencias/2026-11-11/barra-titulo.png) · [Identificação lateral](referencias/2026-11-11/identificacao-lateral.png).

## Sequência de implantação

1. **Legibilidade e identidade:** reproduzir/corrigir a guia e trocar o texto lateral. Encerrar com uma prévia testável antes de ampliar o escopo.
2. **Avaliação do acabamento e movimento:** comparar os exemplos, escolher intensidade e comportamento da busca e prototipar a barra de título. A escolha das referências não implica migração de framework.
3. **Implementar a direção escolhida:** busca, bordas, transições e gráficos em entregas sucessivas, coordenadas com a reorganização dos menus. Cada uma termina com aceite e pacote próprios.
4. **Homologar:** teclado, DPI, alto contraste, animações desativadas no Windows, sessão remota, redimensionamento e navegação rápida. Guardar gravações curtas para avaliar movimento, além de capturas estáticas.

Manter PowerShell 7 como avaliação separada na beta.10. Ao publicar cada implementação: revisar README local e do GitHub, manual, escopo, planejamento e resultados; renovar as imagens afetadas e gerar ZIP e SHA-256. Esta revisão documental não cria uma nova versão executável.

## Referências consultadas

| Referência oficial | O que avaliar | Aplicação proposta |
| --- | --- | --- |
| [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery) e [transições de página](https://learn.microsoft.com/en-us/windows/apps/develop/motion/page-transitions) | Exemplos interativos de controles e navegação. A documentação distingue entrada de área e navegação em profundidade. | Direção **Fluente**: seleção contínua, entrada breve de conteúdo e detalhes com movimento próprio. APIs WinUI são referência, não substitutos diretos de controles WinForms. |
| [Files](https://github.com/files-community/Files) | Organização e acabamento de um aplicativo desktop moderno. | Direção **Discreta**: estrutura estável e mudanças localizadas. As durações abaixo são propostas para este projeto, não medições do Files. |
| [Flow Launcher](https://github.com/Flow-Launcher/Flow.Launcher) | Busca como ação central e apresentação dos resultados. | Direção **Expressiva**: paleta dedicada e abertura mais perceptível. A busca permanece restrita ao catálogo, sem execução livre de comandos. |
| [Fluent 2 · Motion](https://fluent2.microsoft.design/motion) | Consistência, hierarquia do movimento e alternativa sem animação. | Compartilhar durações/curvas entre componentes e respeitar as preferências de movimento do Windows. |
| [Personalização da barra de título](https://learn.microsoft.com/en-us/windows/apps/develop/title-bar) | Responsabilidades da barra e dos controles de janela. | Avaliar integração visual preservando comportamento e compatibilidade da tecnologia atual. |

## Propostas para avaliação

**Atualização de direção:** o usuário considerou os primeiros exemplos simples demais e pediu pesquisa além do padrão Windows. As três variantes abaixo permanecem como histórico; a recomendação inicial Fluente não representa uma escolha aprovada. A [pesquisa visual moderna](PESQUISA-VISUAL-MODERNA-2026-11-11.md) traz referências e composições mais distintas para a próxima avaliação.

- **Discreta:** busca integrada; movimento de 4–6 px próximo de 140 ms; gráficos com interpolação curta, sem ultrapassar o alvo.
- **Fluente — recomendação inicial:** busca ancorada no topo em superfície própria; transições de 8–12 px próximas de 200 ms; seleção contínua e gráficos com desaceleração.
- **Expressiva:** paleta central de comandos, escala leve e entrada sequencial dos resultados; transições próximas de 260 ms. Comparar se o movimento adicional ajuda ou distrai.

Tempos e distâncias são pontos de partida, não requisitos prescritos pelas referências nem resultados de desempenho. Os exemplos animados usam dados ilustrativos. Avaliar movimento, interrupção e continuidade em execução antes de selecionar uma direção; não animar apenas por decoração.

## Observações sobre o código atual

A guia selecionada usa negrito dentro de uma área com margens adicionais; é uma hipótese para investigar o recorte, não uma causa confirmada. A transição altera o Padding durante 180 ms, ponto a revisar quanto a relayout e continuidade. O velocímetro redesenha a cada progresso sem interpolar a agulha. A implementação de referência está em ControlesModernos.cs, Movimento.cs e PainelVelocidade.cs.

Aceite documental: todos os itens solicitados foram registrados e as referências consultadas em 11 de novembro de 2026. Reprodução dirigida, correções funcionais, medidas de fluidez e homologação permanecem pendentes.
