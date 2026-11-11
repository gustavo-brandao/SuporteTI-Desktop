# Escopo das prévias

## Prévia 1.0.0-beta.9.3 · componentes de monitoramento

Entregar a segunda etapa visual em WinForms: barras horizontais reais de CPU/RAM a cada segundo, escala fixa 0–100%, memória usada/disponível/total e histórico opcional das últimas 60 amostras. Pausar e trocar de ferramenta conservam a leitura; iniciar novamente reinicia o histórico. A interpolação é apenas visual, respeita animações do Windows e não preenche amostras ausentes.

Substituir a curva de velocidade pelo velocímetro, mantendo o motor HTTPS, limites de dados, progresso, cancelamento e resultados parciais da beta.9.2. Usar escala crescente explicitamente identificada em Mbps, sem converter falta de dados em taxa zero. Ao concluir, exibir as duas direções e orientar a agulha pela taxa final disponível. Redesenhar somente ao receber progresso ou invalidar a área, sem timer decorativo contínuo no velocímetro.

Publicar ZIP e SHA-256, README e capturas reais após validar a etapa. Preservar o histórico e as pendências de laboratório. A redistribuição completa dos menus fica para a próxima prévia; PowerShell 7 permanece na avaliação da beta.10.


## Prévia 1.0.0-beta.9.2 · estrutura adaptável

Entregar a primeira etapa do dashboard aprovado no WinForms atual: paleta azul escura, seleção lateral discreta, navegação de 240/80 pixels efetivos, recolhimento automático abaixo de 1.000 pixels efetivos da largura cliente e alternância manual preservada na faixa atual. Ctrl+K expande e foca a busca. Manter os nomes dos controles para acessibilidade e os sete grupos; Sobre/Configurações ficam na base, separados da lista rolável.

Manter as abas nativas em uma linha, com rolagem quando excederem a largura, teclado, indicador e foco. Remover a repetição do nome do grupo nos títulos da área. Calcular uma altura mínima do conteúdo para que janelas baixas usem rolagem em vez de comprimir os resultados. Não alterar o catálogo, coletores, permissões, intervenções, pesquisa automática de códigos, formatos de log ou cópia.

Os gráficos atuais de monitoramento e velocidade permanecem nesta etapa. As barras horizontais, o velocímetro e a aplicação completa dos layouts da galeria entram nas próximas prévias. Não incluir PowerShell 7 nem migrar framework nesta entrega. Publicar a beta com capturas reais, notas, ZIP e SHA-256 após a validação; preservar as pendências da matriz de homologação.

## Avaliação visual · dashboard de monitoramento

O tema dashboard de monitoramento, as barras horizontais de CPU/RAM com atualização real e o velocímetro de internet são a direção aprovada. A [galeria consolidada](../docs/propostas-visuais/2026-11-10/conjunto-monitoramento/README.md) reúne cinco menus amplos/compactos e a análise de ajuda das 44 operações para avaliar sua disposição. Esta etapa documenta a proposta e os critérios de aceite; não promove versão nem declara os layouts implementados. Preservar temas Escuro/Claro/Sistema, edição genérica, logs, cópia, pesquisa de códigos e UAC por operação. As imagens são mockups e não substituem capturas reais do README.

## Prévia 1.0.0-beta.9.1

Refinar as abas superiores, com contorno, seleção explícita, estado de apontamento, foco e quebra em linhas quando a janela é estreita. Preservar o TabControl nativo para teclado e automação. Acrescentar realce gradual na navegação e entrada curta de conteúdo, sem bloquear ações ou sobrepor capturas às telas. Respeitar SPI_GETCLIENTAREAANIMATION (somente leitura), alto contraste e sessão remota. Descartar timers ao fechar e interromper animações de gráficos ocultos.

O monitor usa os mesmos dados nativos a cada segundo e conserva até 60 amostras. Interpolar somente a apresentação, com valores numéricos imediatos e lacunas preservadas. Não simular utilização de hardware nem executar testes de carga.

Substituir a pequena amostra do teste gráfico de internet por um motor C# isolado e cancelável: latência HTTPS, quatro requisições HTTP/1.1 simultâneas, até dez segundos e 512 MiB por direção. Limitar buffers, não seguir redirecionamentos e usar endpoints fixos da Cloudflare. Mostrar progresso da coleta, dados e taxa; admitir encerramento antecipado pelo limite. Calcular upload pelos blocos confirmados, conservando download quando upload falha. Manter resultados no contrato versão 1, logs completos e cópia. Não alterar os módulos da edição CLI nem incluir PS7 nesta revisão.

As referências, o método e os limites estão em [Revisão visual e medição](../docs/REVISAO-VISUAL-beta9.1.md). A beta.10 permanece destinada ao protótipo do PowerShell 7. Homologação de Windows 10, DPI, alto contraste real e laboratório administrativo continua pendente.

## Prévia 1.0.0-beta.9

Modernização no WinForms/.NET 10 atual, com superfícies neutras, azul de destaque, botões e cartões arredondados, ícones na navegação e abas desenhadas com indicador de seleção e foco. Manter controles acessíveis, navegação por teclado, as 44 operações, logs e o fluxo de UAC.

Configurações → Aparência oferece Escuro, Claro e Sistema. A primeira abertura usa Escuro. A preferência é gravada de forma atômica em LocalAppData/SuporteTI-Desktop/aparencia.json, por usuário, sem escrever na pasta do executável. Arquivo ausente, inválido, excessivo ou de versão desconhecida usa Escuro; falha ao salvar conserva o tema aplicado na sessão e informa a limitação. O diretório SUPORTETI_CONFIG_DIR é opcional para testes e cópias isoladas.

Sistema resolve a preferência de aplicativos do Windows e reage a eventos de personalização na interface. Alto contraste tem prioridade sobre as paletas. A troca recolore resultados e gráficos sem executar consultas ou descartar dados. Diálogos administrativos recebem somente o tema efetivo permitido, validado antes do UAC, e o ícone verificado junto aos demais arquivos da cópia protegida. Credenciais e confirmações mantêm suas restrições anteriores.

Conservar a barra de título nativa, versão e ícone do aplicativo. Usar recursos de título e cantos do DWM quando disponíveis, com fundo sólido como alternativa. Diálogos do sistema, UAC e console permanecem sob responsabilidade visual do Windows. Windows 10, transições reais de alto contraste e a matriz completa de DPI continuam como aceites de laboratório; não migrar para WinUI nem incluir PowerShell 7 nesta etapa.

## Prévia 1.0.0-beta.8

Otimizar a apresentação de resultados e isolar sua projeção em LinhasResultado. Inserir linhas em lote, calcular alturas das células visíveis e aplicar o filtro após 200 ms sem novas teclas. Limpar o filtro e receber um resultado continuam atualizando imediatamente. Todos os registros e avisos permanecem acessíveis; a cópia completa e o TXT usam a resposta original.

As consultas não elevadas recebem um Job Object próprio do Windows e encerramento confirmado do processo antes da próxima consulta. A saída padrão tem limite de 16 Mi caracteres e a saída de erro de 64 Ki caracteres. O canal elevado e as intervenções administrativas mantêm seu comportamento anterior. [Job Objects: referência oficial](https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects)

Comparar três repetições por cenário com a beta.7: janela, tabela com 1.000 campos, sequência de seis teclas, 44 destinos, memória e handles; medir também ZIP com cache de extração vazio/reutilizado e consulta real. Cobrir processos filhos após cancelamento repetido, prazo e conclusão. Acesso a laboratório Hyper-V foi recusado pelas permissões da sessão; manutenção real em VM, conta distinta, Windows 10, UNC assinado, DPI e alto contraste continuam pendentes, sem declarar homologação completa.

## Prévia 1.0.0-beta.7

O seletor apresenta 35 entradas para as 44 operações preservadas. GPO usa Escopo e Ação; IPv4 usa Modo; eventos usam Leitura; impressão usa Ação, com diagnóstico, reinício e limpeza de fila separados. A busca continua indexando os 44 IDs e preenche o formulário correspondente, inclusive ações administrativas.

Escolher um campo não executa consultas nem manutenção. A operação efetiva aparece por extenso com a indicação de UAC quando aplicável. Atalhos contextuais oferecem consulta de adaptadores, diagnóstico de impressão, consulta das políticas do escopo ou abertura da pesquisa de códigos. As intervenções mantêm seleção de alvo, revisão e confirmação. Não há mudança nos coletores, parâmetros, transporte JSON ou política de elevação.

O aceite inclui as 44 rotas da janela, todas as combinações de GPO, modos IPv4, leituras de eventos e ações de impressão, busca de intervenções sem executá-las, log e clipboard, além da regressão das consultas. Alterações reais continuam restritas ao laboratório previsto na matriz.

## Prévia 1.0.0-beta.6

Os formulários recebem rótulos e a fonte base passa a Segoe UI 10 pt. A tabela ganha espaçamento e as instruções se ajustam à largura. O cancelamento e os comandos de relatório aparecem nos estados em que podem ser usados.

O contrato mantém a versão 1 e acrescenta os campos opcionais metrica e estadoDisco. Eles distinguem ocupação e desgaste e transportam a classificação do coletor. O resumo conta discos com alertas ou leitura inconclusiva pela classificação recebida. O texto completo da evidência permanece na tabela, no TXT e na cópia.

## Prévia 1.0.0-beta.5

A tabela oferece cópia por valor, seleção múltipla e Ctrl+C. O menu de contexto permite copiar valor, linhas ou resultado completo. O relatório completo usa a mesma formatação do TXT e conserva os dados ocultos pelo filtro, horários, estado e avisos. O clique em códigos permanece dedicado à pesquisa.

A falha ao acessar a área de transferência é apresentada no rodapé e pode ser repetida. A seleção de linhas não executa cópia automática. O cabeçalho do relatório passa a identificar a versão de pré-lançamento.

## Prévia 1.0.0-beta.4

O catálogo central preserva os 44 identificadores de ferramentas e define sete destinos por assunto. A janela usa abas para as áreas e um seletor de ferramentas. Saúde, volumes, mapa e CHKDSK ficam em Hardware → Discos. Impressão tem uma página própria. GPO, perfis e Autologon ficam em Contas e políticas.

A busca encontra ferramentas por nome, área, grupo ou identificador, combinando palavras sem distinguir acentos. Abrir um resultado seleciona o contexto e conserva as confirmações e o UAC das ações. O clique em códigos de erro usa o identificador da pesquisa, sem depender da posição de um item na lista.

## Prévia 1.0.0-beta.3

A prévia corrige o campo percentual de desgaste para transportar número finito não negativo ou null. A leitura indisponível conserva o texto do coletor. A regressão passa a executar discos com as dependências SMART presentes. A reorganização de menus, a cópia de resultados e as mudanças visuais seguem as etapas de [revisão de UX](../docs/REVISAO-UX-2026-11-09.md).

## Prévia 1.0.0-beta.2

A consulta de dispositivos usa o identificador PnP quando o Windows não informa um nome. Quando ambos estão ausentes, a interface exibe um rótulo genérico e mantém o código de problema.

## Prévia 1.0.0-beta.1

A beta reúne as operações da 0.7.0 com identificação de pré-lançamento no título, no executável e no ZIP. A fase de homologação tem roteiro e matriz de ambientes próprios. O pacote local é destinado a testes. A promoção a versão estável depende da assinatura e dos aceites de laboratório.

## Atualização 0.7.0

Os reparos e fluxos sensíveis ficam em **Administrativo** e exigem revisão, confirmação e UAC por operação. O CHKDSK usa a janela nativa para responder ao agendamento. A exclusão de perfil usa a seleção e a confirmação do processo elevado, com revalidação do alvo. O Autologon recebe a senha no processo elevado e usa a LSA. O retorno à sessão principal leva o resultado da configuração. O reinício é manual.

## Atualização 0.6.0

Falhas do Windows passa a nomear a área de leitura e interpretação de eventos. As consultas de disco apresentam gráficos para valores numéricos disponíveis e mantêm os avisos do diagnóstico.

As intervenções confirmadas incluem recriar o cache de download do Windows Update, reiniciar o spooler, cancelar trabalhos de uma fila local escolhida, atualizar GPO por escopo, limpar o cache HTTP do Chrome e alterar IPv4 e DNS em adaptador físico. As ações administrativas passam pelo canal elevado da 0.5.0. A limpeza do Chrome e o GPO do usuário executam na sessão original.

## Atualização 0.5.0

A janela usa o nível de acesso da sessão. O botão de consulta em Administrativo abre um processo separado pelo UAC, com uma lista fixa de diagnósticos e parâmetros. O processo elevado executa arquivos de uma cópia protegida, com hashes conferidos contra os recursos do executável e assinatura exigida em origem UNC. O canal local de retorno confere o processo remetente. A sessão original recebe o resultado e grava o HTML ou o TXT no destino que o técnico escolher.

## Atualização 0.4.1

Ao acionar um código destacado, a interface preenche a pesquisa e consulta o significado no mesmo passo, usando a família identificada no evento. A pesquisa avulsa recebe apenas o código e lista as correspondências por família. Se houver ambiguidade, o contexto de origem ajuda o técnico a escolher a interpretação.

## Atualização 0.4.0

A triagem recebe códigos estruturados com a família definida pelo coletor. O código clicável preenche a pesquisa, que o técnico executa pelo botão **Consultar**. Eventos do Windows Update com HRESULT identificável também oferecem esse atalho. O ID do evento permanece visível nos detalhes.

Atendimento reúne Windows Update, impressão e GPO da sessão atual. Relatórios reúne a revisão em memória, a leitura de eventos recentes e a exportação EVTX por canal, com o manifesto dos arquivos válidos. O TXT continua disponível para os resultados das consultas e exportações.

## Atualização 0.3.0

Rede e Triagem ampliam as consultas do protocolo 1. Ping e pesquisa de códigos recebem entrada explícita. O teste de velocidade depende de acionamento manual e preserva uma direção medida quando a outra falha. A triagem reutiliza o coletor e o catálogo da edição de terminal. A leitura de rede usa CIM e apresenta os adaptadores com IP ativo.

O salvamento manual de resultados em TXT chegou nesta etapa às três áreas de consulta. A exportação EVTX segue na etapa 0.4.0.

## Atualização 0.2.0

A área Computador ganhou o seletor de diagnósticos e o filtro, com telas próprias para o monitor e o mapa. As consultas ampliam o protocolo 1 com identificadores fixos. A saúde dos discos reutiliza o coletor SMART da edição de terminal. O monitor usa contadores nativos, e a bateria consulta capacidades ACPI disponíveis pelo Windows. A consulta de login automático usa acesso somente leitura ao Registro e exibe o estado sem expor a conta.

A seleção do mapa é sincronizada com a lista, o caminho permite voltar por nível e os resultados parciais têm indicação própria. As capacidades do volume são apresentadas separadamente do tamanho lógico dos arquivos.

## Atualização 0.1.1

A edição pública adota tema azul escuro e ícone original de terminal, com paleta do sistema em alto contraste. O ZIP agrupa o runtime no executável, mantém os scripts em `Modulos` e coloca instruções e licença em `Documentacao`. O catálogo da prévia continua limitado à consulta `sistema`.

## Base 0.1.0

A primeira entrega valida o caminho completo entre WinForms e uma consulta real em Windows PowerShell 5.1. A edição gráfica fica em um subprojeto isolado do workspace público, com estrutura própria para futura extração. O commit de referência é `1e3e0109e8135a5b3a80fdf0f0a1b1dab7f6962e`.

O protocolo 1 retorna um objeto JSON UTF-8 pela saída padrão. A operação fixa `sistema` informa `concluido`, `parcial` ou `indisponivel` e devolve cinco seções com pares de nome e valor e um aviso opcional. A interface trata falhas do processo, resposta inválida, cancelamento e tempo limite. A entrada aceita somente o identificador dessa consulta. O coletor adapta as mesmas classes CIM e os mesmos campos da consulta de sistema do terminal e reaproveita suas funções de CPU e memória.

## Matriz de paridade

Os fluxos mistos indicam as versões em que cada modo entra. Na 0.1.0, apenas `sistema` tem entrada gráfica. A evidência dos demais fluxos será registrada a cada prévia.

| Operação | Contexto | Dependência e efeito | Saída principal | Versão |
| --- | --- | --- | --- | --- |
| `sistema` | Sessão atual | CIM local, leitura | Tabela e avisos | 0.1.0 |
| `desempenho` | Sessão atual | CIM e processos, leitura | Amostra de CPU/RAM e processos | 0.2.0 |
| `monitor` | Sessão atual | CIM, leituras sucessivas | Barras de CPU/RAM | 0.2.0 |
| `discos` | Sessão atual | CIM, Storage e smartctl, leitura | Ocupação e saúde | 0.2.0 |
| `diskhealth` | Elevado | CIM, Storage e smartctl, leitura | Saúde e evidências | 0.5.0 |
| `dispositivos` | Sessão atual | CIM/PnP, leitura | Dispositivos com problema | 0.2.0 |
| `servicos` | Sessão atual | Gerenciador de serviços, leitura | Estado dos serviços | 0.2.0 |
| `programas` | Sessão atual | Registro, leitura | Programas instalados | 0.2.0 |
| `falhas` | Sessão atual | Eventos e catálogo local, leitura | Falhas do Windows e evidências | 0.3.0 |
| `codigofalha` | Sessão atual | Catálogo e mensagens do Windows, leitura | Descrição e referência | 0.3.0 |
| `chromecache` | Sessão comum | Perfil local do Chrome, limpeza restrita | Resumo da limpeza | 0.6.0 |
| `mapaocupacao` | Sessão atual | Sistema de arquivos, leitura | Mapa e lista de itens | 0.2.0 |
| `autologon` | Sessão atual ou elevada conforme o modo | Registro e LSA, consulta ou alteração | Estado ou confirmação | 0.2.0 e 0.7.0 |
| `bateria` | Sessão atual | powercfg, relatório temporário | Capacidade e avisos | 0.2.0 |
| `rede` | Sessão atual | ipconfig, leitura | Configuração dos adaptadores | 0.3.0 |
| `dominio` | Sessão atual | CIM, leitura | Ingresso e domínio | 0.3.0 |
| `ping` | Sessão atual | ICMP para destino escolhido | Resumo e tentativas | 0.3.0 |
| `velocidade` | Sessão atual | HTTPS para Cloudflare | Amostras de download e upload | 0.3.0 |
| `netdhcp` | Elevado | Adaptador escolhido, alteração de IPv4/DNS | Estado antes e depois | 0.6.0 |
| `netstatic` | Elevado | Adaptador escolhido, alteração de IPv4/DNS | Estado antes e depois | 0.6.0 |
| `update` | Sessão atual | Serviços, Registro e eventos, leitura | Diagnóstico | 0.4.0 |
| `cache` | Elevado | Windows Update, recriação do cache de download | Resumo da ação | 0.6.0 |
| `gpusuario` | Usuário original | gpresult, leitura | Políticas aplicadas | 0.4.0 |
| `gpcomputador` | Elevado | gpresult, leitura | Políticas aplicadas | 0.5.0 |
| `gphtml` | Usuário original ou elevado por escopo | gpresult, gravação no destino escolhido | Arquivo HTML | 0.4.0 e 0.5.0 |
| `gpupdateuser` | Usuário original | gpupdate, atualização das políticas do usuário | Resumo da ação | 0.6.0 |
| `gpupdatecomputer` | Elevado | gpupdate, atualização das políticas do computador | Resumo da ação | 0.6.0 |
| `filaimpressao` | Sessão atual | Spooler e filas visíveis, leitura | Diagnóstico das filas | 0.4.0 |
| `spooler` | Elevado | Spooler, reinício do serviço | Estado do serviço | 0.6.0 |
| `spoolerclear` | Elevado | Fila local selecionada, cancelamento de trabalhos | Contagem e falhas parciais | 0.6.0 |
| `eventos` | Sessão atual | Eventos de System e Application, leitura | Lista de eventos | 0.4.0 |
| `ultimo` | Sessão atual | Resultado em memória e gravação opcional | Texto ou arquivo TXT | 0.4.0 |
| `evtx` | Sessão atual | Logs e gravação no destino escolhido | EVTX e manifesto SHA-256 | 0.4.0 |
| `sfc` | Elevado | sfc.exe, verificação ou reparo | Saída e resumo | 0.5.0 e 0.7.0 |
| `dism` | Elevado | dism.exe, verificação ou reparo | Saída e resumo | 0.5.0 e 0.7.0 |
| `chkdsk` | Elevado | chkdsk.exe, consulta ou reparo | Saída e resumo | 0.5.0 e 0.7.0 |
| `perfillocal` | Elevado | Perfil local, exclusão confirmada | Diagnóstico e confirmação | 0.7.0 |
