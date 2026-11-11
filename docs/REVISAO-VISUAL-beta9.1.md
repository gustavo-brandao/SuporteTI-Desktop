# Revisão visual e medição · beta.9.1

## Referências consultadas em 10/11/2026

- [WinUI Gallery](https://github.com/microsoft/WinUI-Gallery): aplicação oficial de exemplos, útil para hierarquia visual, navegação e demonstrações de transições.
- [Files](https://github.com/files-community/Files): aplicação para Windows com navegação lateral e abas; referência de separação visual entre navegação e conteúdo. Não incorporar código ou dependências desse projeto.
- [Transições de página no Windows](https://learn.microsoft.com/windows/apps/design/motion/page-transitions): transições breves para indicar mudança entre áreas. Adaptar ao WinForms existente, mantendo foco e acessibilidade. Não declarar migração para WinUI ou macOS.
- [Preferência de animações do Windows](https://learn.microsoft.com/en-us/windows/win32/winauto/client-area-animation): consultar SPI_GETCLIENTAREAANIMATION; não alterar a configuração do sistema.
- [Motor de teste da Cloudflare](https://github.com/cloudflare/speedtest): referência para a necessidade de múltiplas transferências e duração suficiente. O motor deste aplicativo é próprio, não executa o módulo JavaScript e não declara equivalência com seus resultados.

## Decisões

| Área | Mudança | Aceite |
| --- | --- | --- |
| Abas | Contorno, texto, seleção azul, foco, apontamento e múltiplas linhas em janela estreita | Seis abas de Hardware acessíveis a 780 × 600 nos três temas |
| Menu lateral | Realce gradual de 160 ms e marcador do item atual | Seleção responde imediatamente; navegação preserva as 44 rotas |
| Conteúdo | Entrada com deslocamento de até 10 px em 180 ms | Sem bloquear clique, teclado, consulta ou cancelamento |
| Monitor | Área sob a curva, ponta da leitura e transição de 240 ms | Leituras numéricas reais a cada segundo; pausa ao ocultar; sem inventar amostras |
| Internet | Até 10 s por direção, quatro transferências e limite de 512 MiB por direção | Tempo, orçamento, cancelamento, falhas parciais e upload não confirmado testados |
| Atividade | Indicador de execução, fase, curva e progresso baseado na coleta | Sem velocidade aleatória ou espera artificial para prolongar o teste |

## Método do teste de internet

O teste gráfico anterior fazia somente 8 MiB de download e 4 MiB de upload. Em links rápidos, isso pode terminar antes de existir uma janela representativa de transferência. A nova coleta reutiliza conexões HTTPS com quatro requisições HTTP/1.1 em paralelo e buffers de 64 KiB por trabalhador. As requisições de dados usam blocos de até 8 MiB, repetidos durante a janela de coleta. Na conexão de teste, a solicitação de 16 MiB foi recusada com HTTP 403, enquanto 1 e 8 MiB receberam HTTP 200; isso orientou a escolha de blocos menores, sem inferir um limite universal do serviço. Nenhum arquivo pessoal é enviado; o upload usa dados gerados em memória.

Cada direção tem uma janela de dez segundos e orçamento compartilhado de 512 MiB. O upload começa em blocos de 256 KiB por trabalhador e amplia os seguintes somente após confirmação rápida, até 8 MiB; confirmações demoradas reduzem o bloco. Isso permite obter amostras em links lentos sem começar com grandes uploads que excederiam todo o prazo. O primeiro limite atingido encerra a coleta. Até três consultas HTTPS de latência precedem as transferências, com limite de cinco segundos cada. Não aguardar artificialmente depois de atingir o orçamento. A interface explica o consumo antes de iniciar.

Mbps = bytes válidos × 8 ÷ segundos decorridos ÷ 1.000.000. O download conta bytes realmente recebidos. O upload conta somente blocos cuja resposta HTTP de sucesso chegou; dados em trânsito ou rejeitados não são considerados velocidade confirmada. O tempo inclui o custo das requisições da fase. O resultado é conservador e pode subestimar links lentos se o último bloco não for confirmado até o prazo; os bytes válidos e a duração ficam visíveis. Não prometer medir a capacidade física total do link nem a velocidade contratada.

A latência é HTTPS até o servidor, incluindo o estabelecimento inicial quando necessário; não equivale a ping ICMP. As curvas de taxa mostram a média acumulada da fase. Variações de Wi-Fi, proxy, VPN, carga e distância ao servidor afetam o resultado.

O cliente não segue redirecionamentos, não usa cookies e não envia credenciais automaticamente. Cancelar interrompe as requisições ativas. Falha em uma direção preserva a outra com aviso, e o relatório completo continua disponível para cópia e TXT. A sessão padrão é suficiente.

## Validação e sequência

Cobrir motor com transporte simulado: concorrência, bytes, duração, resposta incompleta, indisponibilidade, cancelamento, upload não confirmado, contrato e log parcial. Rodar a regressão acumulada e os simuladores de intervenções. Conferir pacote extraído, abas, monitor, temas, confirmações e capturas reais da versão atual. O teste externo é opcional no verificador porque pode consumir até 1 GiB.

Registrar resultados efetivos em [Testes/RESULTADO.md](../Suporte%20TI%20Grafico/Testes/RESULTADO.md). Manter a beta.10 para PowerShell 7; esta revisão intermediária não promove a aplicação a versão estável.
