# Pesquisa visual moderna · 11 de novembro de 2026

## Direção do usuário

**Direção aprovada:** estrutura profissional inspirada no Linear, monitoramento com o cuidado visual do iStat e busca em camadas inspirada no Raycast. O usuário aprovou prosseguir com essa combinação. A correção de legibilidade e identidade é a primeira implantação pequena, seguida da composição visual e dos componentes, uma entrega por vez.

Os primeiros exemplos ficaram simples demais. Ampliar a exploração além do estilo padrão do Windows, com identidade própria, composição mais elaborada e movimento mais bem resolvido. A variante Expressiva ter sido explorada não constitui aprovação: o feedback posterior pede uma revisão mais ampla.

As alternativas anteriores Discreta/Fluente/Expressiva ficam como estudo inicial. Não escolher a próxima aparência apenas mudando cor, velocidade ou raio das mesmas caixas. Produzir composições diferentes, com busca, monitoramento e uma tela técnica completos, antes de fechar a direção. Preservar legibilidade, edição pública e as 44 operações.

## Referências primárias e o que observar

### Raycast · comando e profundidade

[Tour oficial do Raycast v2](https://manual.raycast.com/new-in-v2) · [Relato técnico com imagens e comportamento da interface](https://www.raycast.com/blog/a-technical-deep-dive-into-the-new-raycast) · [Imagem oficial no Windows](https://www.raycast.com/uploads/blog-technical-deep-dive-new-raycast/raycast-2-windows.png).

A equipe apresenta uma busca central com estados compactos e expandidos e descreve o trabalho para evitar cintilação durante abertura, transições e redimensionamento. É uma referência de comportamento e acabamento, além de aparência. O relato também distingue a integração do material e da moldura em Windows e macOS.

**Proposta para avaliar:** busca como centro de comandos, superfície elevada, resultado selecionado com painel de contexto e atalhos visíveis. Abertura/fechamento com continuidade de escala, posição e opacidade; conteúdo já preparado antes de aparecer. A identidade do Suporte TI permanece própria. Não incluir agentes, IA ou execução livre de comandos por imitar a referência.

### Linear · acabamento de ferramenta profissional

[Revisão visual de 2026 com comparações e vídeo](https://linear.app/now/behind-the-latest-design-refresh) · [Redesenho com imagens completas](https://linear.app/now/how-we-redesigned-the-linear-ui).

Os artigos mostram a hierarquia entre navegação e conteúdo, alinhamento de cabeçalhos, guias compactas e redução do peso das bordas. A lateral recua visualmente para dar prioridade à área de trabalho. Usar esse material como referência de composição e coerência; os artigos não são medições de desempenho das animações.

**Proposta para avaliar:** área técnica com densidade bem organizada, navegação discreta, título integrado, ferramentas contextuais e detalhes laterais. Substituir sucessões de seletores e grandes regiões vazias por informações relacionadas e ações no contexto do diagnóstico. Não usar apenas uma paleta escura para tentar reproduzir o acabamento.

### iStat Menus · qualidade na apresentação de métricas

[Página oficial com exemplos dos painéis](https://bjango.com/mac/istatmenus/).

A referência organiza uso atual, histórico e detalhes de CPU, memória, discos e rede em diferentes níveis de leitura. Ela é relevante para nosso monitoramento; isso não implica disponibilizar sensores ou dados que os coletores atuais não oferecem.

**Proposta para avaliar:** painel de hardware com hierarquia clara entre valor atual e histórico, gráficos compactos úteis, legenda junto da leitura, detalhes por expansão e indicadores coerentes de indisponibilidade. A fluidez proposta precisa ser implementada e medida aqui; não foi feita inspeção do aplicativo instalado nesta pesquisa.

### CleanMyMac · identidade e diagnóstico visual

[Página oficial com demonstrações do aplicativo](https://macpaw.com/cleanmymac).

A página apresenta uma ferramenta de manutenção com identidade gráfica própria e organização por tarefas. Serve para explorar uma direção mais visual e expressiva, próxima do domínio de suporte e manutenção.

**Proposta para avaliar:** uma área de diagnóstico com composição central, estados visuais bem definidos, superfícies com profundidade e evolução coerente de pronto → consultando → resultado. Cor e ilustração devem reforçar a informação existente, sem anunciar uma máquina saudável ou um reparo concluído sem evidência.

### Warp · área de trabalho técnica contemporânea

[Produto e imagens oficiais](https://www.warp.dev/terminal) · [Aparência e temas](https://docs.warp.dev/terminal/appearance/).

O material apresenta uma área técnica organizada em painéis, sessões e resultados. É referência para pensar os detalhes de diagnóstico como uma área de trabalho legível e contextual, em vez de uma única tabela visualmente dominante.

**Proposta para avaliar:** resultados em blocos expansíveis, comandos relevantes junto de cada diagnóstico, filtros e detalhes preservados sem perder orientação. Não ampliar o escopo para um terminal livre ou implementar recursos de IA por associação com o produto pesquisado.

## Quatro composições a prototipar

| Proposta | Referências | Diferença de estrutura | Movimento a demonstrar |
| --- | --- | --- | --- |
| **Studio técnico** | Linear + Warp | Cabeçalho integrado, navegação discreta, resultado central e painel de detalhe contextual | Seleção contínua, passagem entre diagnósticos e expansão do detalhe sem salto |
| **Comando em camadas** | Raycast | Busca central elevada, categorias, prévia e ações contextualizadas | Abrir/fechar, redimensionar os resultados e voltar ao contexto preservado |
| **Monitoramento premium** | iStat Menus | Valores, históricos e detalhes desenhados como conjunto; sem cartões enormes quase vazios | Barras, curvas e agulha com alvo interrompível, sem fabricar medições |
| **Diagnóstico expressivo** | CleanMyMac | Área principal de diagnóstico com identidade visual própria e resultados por tarefa | Transição contínua entre estados, composição e revelação de resultados |

Essas são interpretações propostas para o Suporte TI, não descrições exatas dos produtos nem cópias aprovadas. A recomendação passa a ser comparar **Studio técnico** com **Monitoramento premium**, usando a busca de **Comando em camadas** como componente candidato. A intensidade mais expressiva deve ser avaliada em contexto antes de escolher.

## Qualidade exigida no próximo estudo

- Demonstrar busca, troca de tela e gráficos em movimento, com dados explicitamente ilustrativos e opção de repetir/interromper as transições.
- Diferenciar de verdade composição, densidade, hierarquia, tipografia, superfícies e comportamento; evitar três versões da mesma tela com tempos diferentes.
- Desenhar a moldura e o título como parte da composição, preservando redimensionamento, Snap, controles e comportamento de janela do Windows.
- Usar profundidade, transparência e iluminação somente quando ajudarem a separar camadas. Não confundir acabamento com brilho permanente, vidro em todos os painéis ou ícones decorativos.
- Preparar a entrada e saída dos estados. Reagir a navegação rápida e novas amostras sem reiniciar bruscamente, sobrepor capturas ou acumular animações.
- Avaliar redução de movimento, temas, foco, DPI, contraste e uso remoto. O caminho técnico depende do protótipo; a pesquisa não decide uma migração de WinForms, nem promete reproduzir materiais nativos de outra plataforma.
- Registrar tempo de abertura, memória, custo de renderização e fluidez na implementação real antes de aceitar o resultado. Não transferir números de desempenho dos produtos de referência para esta ferramenta.

## Registro e limite da pesquisa

Fontes oficiais consultadas em 11 de novembro de 2026. Foram revisados textos técnicos e materiais públicos de apresentação; os produtos não foram instalados nem medidos. Os links são referências para avaliação, não recomendação de compra ou dependências a adicionar. A correção da guia cortada e a troca para VERSÃO DESKTOP continuam como a primeira entrega pequena, separada da escolha visual ampla. README, documentação, imagens e ZIP devem acompanhar cada implantação posterior.
