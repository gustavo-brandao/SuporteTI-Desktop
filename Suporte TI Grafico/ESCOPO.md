# Escopo das prévias

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
