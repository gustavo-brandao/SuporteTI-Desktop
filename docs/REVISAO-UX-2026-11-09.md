# Revisão de menus, diagnósticos e experiência de uso

Revisão de 9 de novembro de 2026. Base Desktop 1.0.0-beta.2, revisão 0d51ce7, comparada com o catálogo público CLI disponível neste workspace. As versões abaixo são propostas cumulativas. Cada etapa termina em pacote de teste e registro de evidência.

A beta.3 concluiu a correção de transporte dos discos. A beta.4 implementa os sete menus, as abas por área, o catálogo central e a busca. A beta.5 acrescenta a cópia de resultados, Ctrl+C e menu de contexto, com recuperação após bloqueio do clipboard. A beta.6 melhora a legibilidade, o espaçamento e a apresentação dos discos, com 82 testes aprovados e verificação da janela redimensionada. O aceite em outras escalas e alto contraste continua pendente. A beta.7 agrupa GPO, IPv4, eventos e impressão em formulários por contexto, com 35 entradas para as 44 operações e 91 testes aprovados. A beta.8 otimiza preenchimento e filtro, isola a projeção dos resultados e acrescenta controle de processos filhos. As medições são locais, e o acesso ao Hyper-V foi recusado; os aceites de laboratório continuam pendentes. A próxima etapa é a beta.9, dedicada ao visual Windows 11 e aos temas.

## Resultado da revisão

O catálogo CLI tem 36 entradas. O Desktop distribui 42 opções em seletores, além do monitor e do mapa. A diferença inclui modos que a CLI apresenta dentro de submenus, como SFC, DISM, CHKDSK e Autologon. Contar botões não demonstra paridade de resultados, parâmetros ou permissões.

O principal problema de navegação é a separação de funções relacionadas por nível de permissão. Saúde dos discos está em Administrativo, enquanto volumes e mapa estão em outras páginas. GPO do usuário e do computador também ficaram em grupos diferentes. Atendimento reúne impressão, Windows Update e políticas em uma lista. Proponho agrupar pelo assunto e apresentar o escudo e a explicação de UAC junto da ação.

## Falha confirmada nos discos

A consulta comum de discos do pacote anterior retornou JSON sintaticamente válido, com percentual de desgaste representado por objeto vazio em três linhas. O modelo C# aceita número ou null nesse campo. A falha de conversão é apresentada como JSON inválido, embora o problema seja o tipo do campo.

A correção local normaliza o desgaste para número finito não negativo ou null. Um SSD pode informar mais de 100% usado, portanto 125 permanece como medição válida. A barra pode limitar o preenchimento a 100%, mantendo o valor textual original. Sem medição, a interface deve exibir indisponibilidade e nenhuma estimativa inventada.

O coletor é compartilhado pelos fluxos comum e administrativo. A reprodução inicial ocorreu no fluxo comum. Após a correção, o ZIP da beta.3 passou no teste de janela com UAC e retornou dados estruturados na consulta administrativa de discos. O teste com outra conta administrativa continua como aceite de laboratório.

A suíte C# anterior não executava a consulta real de discos e seu projeto não copiava o coletor SMART e as dependências. Foram acrescentados esses arquivos e uma consulta real ao teste de contrato. Há sete cenários adicionais de serialização.

## Organização proposta

| Ordem | Menu principal | Páginas e sequência |
| --- | --- | --- |
| 1 | Visão geral | Resumo do computador, alertas disponíveis, atalhos para investigar |
| 2 | Hardware | Desempenho e monitor; discos; bateria; dispositivos; serviços; programas |
| 3 | Rede | Conexão atual; conectividade; velocidade; configuração IPv4 |
| 4 | Windows | Eventos e códigos; Windows Update; integridade e reparos |
| 5 | Impressão | Impressoras e trabalhos; diagnóstico; ações sobre a seleção |
| 6 | Contas e políticas | GPO por escopo; perfis locais; login automático |
| 7 | Relatórios | Último resultado; salvar TXT; exportar HTML/EVTX |

Sobre e versão ficam no rodapé da navegação. Monitor e mapa continuam acessíveis por atalho na Visão geral e nas páginas correspondentes.

Dentro de Hardware → Discos, usar as abas Saúde, Espaço e Mapa. Saúde lista cada disco físico, evidências, limitações e data da leitura. Espaço lista os volumes. O vínculo entre disco físico e letra depende dos dados do Windows e deve ficar explícito quando indisponível. A ação Ler com acesso administrativo fica na própria página, com UAC por operação.

## Comparação completa e destino

Os estados de teste abaixo distinguem evidência atual, simulação e pendência. A matriz é um inventário para os aceites das próximas etapas.

| Entrada CLI | Entrada ou modo Desktop atual | Destino proposto | Verificação |
| --- | --- | --- | --- |
| sistema | Resumo do sistema | Visão geral | Contrato e CIM real |
| desempenho | CPU, memória e processos | Hardware → Desempenho | Contrato real |
| monitor | Monitor ao vivo | Hardware → Desempenho | Amostra e pausa na suíte de janela |
| discos | Volumes e saúde dos discos | Hardware → Discos | Falha reproduzida; regressão acrescentada |
| diskhealth | Administrativo → Saúde dos discos | Hardware → Discos | Coletor compartilhado; UAC pendente |
| dispositivos | Problemas de dispositivos | Hardware → Dispositivos | Contrato e nome ausente simulados |
| servicos | Serviços locais | Hardware → Serviços | Contrato real |
| programas | Programas instalados | Hardware → Programas | Contrato real |
| falhas | Falhas recentes | Windows → Eventos e códigos | Contrato e evento simulado |
| codigofalha | Consultar código de erro | Windows → Eventos e códigos | Decimal, hexadecimal, famílias e clique |
| chromecache | Limpar cache do Chrome | Hardware → Programas | Limpeza simulada em pasta descartável |
| mapaocupacao | Mapa de disco | Hardware → Discos → Mapa | Soma, ordenação, limite e cancelamento |
| autologon | Estado, configurar, desativar | Contas e políticas → Login automático | Estado real; gravação simulada na CLI; reinício pendente |
| bateria | Saúde da bateria | Hardware → Bateria | Contrato; notebook físico pendente |
| rede | Adaptadores e endereços | Rede → Conexão atual | Contrato real |
| dominio | Domínio e grupo de trabalho | Rede → Conexão atual | Contrato real |
| ping | Ping ICMP | Rede → Conectividade | Entrada inválida, perda e ausência de resposta |
| velocidade | Velocidade da internet | Rede → Velocidade | Falha de upload simulada; medição externa pendente |
| netdhcp | Ativar IPv4 automático | Rede → Configurar IPv4 | Simulação; troca real em VM pendente |
| netstatic | Configurar IPv4 fixo | Rede → Configurar IPv4 | Validação e falha parcial simuladas |
| update | Windows Update | Windows → Atualizações | Contrato real; erro de eventos simulado |
| cache | Recriar cache do Windows Update | Windows → Atualizações | Reinício pendente e restauração simulados |
| gpusuario | Políticas do usuário | Contas e políticas → GPO | Contrato real |
| gpcomputador | Políticas do computador | Contas e políticas → GPO | UAC e conta distinta pendentes |
| gphtml | HTML do usuário ou computador | Contas e políticas → GPO | Exportação do usuário na suíte de pacote; computador pendente |
| gpupdateuser | Atualizar GPO do usuário | Contas e políticas → GPO | Encaminhamento simulado na CLI; domínio real pendente |
| gpupdatecomputer | Atualizar GPO do computador | Contas e políticas → GPO | Encaminhamento simulado na CLI; domínio real pendente |
| filaimpressao | Diagnóstico de impressão | Impressão | Contrato; spooler parado e acesso parcial simulados |
| spooler | Reiniciar spooler | Impressão | Simulação preservando trabalhos |
| eventos | Eventos recentes | Windows → Eventos e códigos | Contrato e acesso negado simulado |
| ultimo | Último resultado | Relatórios | Log único e recuperação na suíte de janela |
| evtx | Exportar eventos | Relatórios | Exportação e manifesto na suíte de pacote |
| sfc | Verificar ou reparar | Windows → Integridade | Argumentos de reparo simulados; reparo real pendente |
| dism | CheckHealth, ScanHealth, reparar | Windows → Integridade | Argumentos de reparo simulados; reparo real pendente |
| chkdsk | Consultar ou reparo completo | Hardware → Discos → Verificação | Validação e argumentos simulados; agendamento real pendente |
| perfillocal | Revisar perfis locais duplicados | Contas e políticas → Perfis | Revalidação simulada na CLI; exclusão real pendente |

O Desktop acrescenta Limpar fila de impressão, que deve continuar distinto de Reiniciar spooler. A primeira cancela trabalhos de uma fila escolhida. A segunda reinicia o serviço e preserva os trabalhos.

## Reduzir passos e ampliar detalhes

| Fluxo | Simplificação | O que precisa continuar explícito |
| --- | --- | --- |
| Discos | Uma página para saúde, espaço e mapa | Disco físico, volume, limite da leitura e pedido de UAC |
| IPv4 | Um formulário com Automático ou Manual | Adaptador, gateway, DNS e confirmação da mudança |
| GPO | Um seletor Usuário ou Computador, ações Consultar, Exportar e Atualizar | Escopo e identidade usados |
| Eventos | Uma tela com período, canal e código associado | ID do evento, HRESULT/Win32/NTSTATUS e parada são conceitos distintos |
| Impressão | Selecionar a fila e mostrar ações no contexto | Cancelar um trabalho, limpar uma fila e reiniciar o serviço |
| Windows Update | Diagnóstico primeiro, ação contextual quando aplicável | Impacto, cópia do cache e falhas parciais |
| SFC/DISM | Modos de consulta em primeiro plano; reparo em ação separada | Comando, efeito, duração e estado final |
| Relatórios | Salvar o resultado da página atual por ação consistente | Destino escolhido, formato e dados incluídos |

Automatizar seleção de contexto, preenchimento de códigos e leituras seguras. A detecção deve sugerir uma ação e explicar o motivo. A execução de reparos, exclusões e configurações continua confirmada pelo técnico.

## Alternativas de desenho

1. **Painel técnico, recomendado.** Navegação lateral por assunto, abas curtas e uma área de detalhes da seleção. Mantém tabelas para os dados técnicos e cartões para estado e medidas.
2. **Atendimento por problema.** Entrada por Sem internet, Não imprime, Windows com falhas ou Disco sem espaço, com passos de diagnóstico. Pode ser uma camada futura sobre as mesmas operações.
3. **Lista compacta com busca.** Lista pesquisável de ferramentas, resultados e ações contextuais. Adequada para técnicos que já conhecem as operações e usam muito o teclado.

Começar pelo painel técnico com WinForms. Validar a navegação e os componentes atuais antes de decidir uma migração de framework.

## Aparência e interação

- Preservar o azul escuro. Proposta inicial de cores de fundo #0F172A, superfície #172033, destaque #60A5FA e texto #E5EDF7. Estados usam também ícone e texto. Conferir contraste antes do aceite.
- Usar Segoe UI como base WinForms, corpo de 10–11 pt, títulos de 16–20 pt e Consolas apenas para códigos e trechos técnicos. Avaliar Segoe UI Variable em protótipo no Windows 11 e conferir fallback.
- Usar espaçamento de 8 px, grupos com 16–24 px e alvos confortáveis. Reorganizar formulários quando a largura cair, em vez de comprimir campos e botões.
- Apresentar a conclusão e os avisos antes dos detalhes. Saúde, ocupação e desgaste precisam de rótulos e escalas distintas. Desgaste indisponível não representa 0%.
- Mostrar período consultado, atualização manual e horário do resultado. Cancelamento precisa refletir a capacidade real da operação.
- Acrescentar busca de ferramentas após fechar o catálogo central. O resultado da busca deve levar à página e ao contexto corretos.

## Copiar resultados

Acrescentar um botão de copiar ao lado de valores úteis, como IP, nome do equipamento, código e mensagem. O clique nesse botão copia o valor e mostra Copiado no rodapé. Menu de contexto com Copiar valor e Copiar linha, além de Ctrl+C para a seleção, complementa o recurso.

No código de erro, o clique no código continua abrindo a explicação. O botão adjacente copia. Uma linha selecionada não deve sobrescrever a área de transferência automaticamente. Tratar a área de transferência temporariamente ocupada com mensagem recuperável. Testar Unicode, texto longo, múltiplas linhas, teclado e preservação do resultado.

## Etapas de implementação e aceites

| Etapa e prévia proposta | Entrega | Aceite |
| --- | --- | --- |
| 1 · beta.3 | Corrigir o transporte dos discos, ampliar testes e explicar falhas de leitura | Regressão com valores ausentes/invalidos, consulta real, teste pelo UAC, ZIP extraído e leitura parcial preservada |
| 2 · beta.4 | Centralizar catálogo, reorganizar menus e mover discos, GPO e impressão | Todos os IDs antigos têm destino; menu, busca e atalhos chegam à mesma operação; zero opção órfã; confirmação preservada |
| 3 · beta.5 | Copiar valores, linhas e resultados; ações contextuais | Clipboard real em sessão interativa, Unicode, Ctrl+C, feedback e clique de códigos preservado |
| 4 · beta.6 | Aplicar fonte, cores, espaçamento, abas e resumo de discos | Escalas 100/125/150/200%, alto contraste, teclado, textos longos e redimensionamento sem controles sobrepostos |
| 5 · beta.7 | Reduzir opções repetidas com formulários de escopo e diagnóstico contextual | GPO, IPv4, eventos e impressão com resultados equivalentes e parâmetros explícitos; falhas parciais legíveis |
| 6 · beta.8 | Otimizar e homologar o catálogo completo | Medições antes/depois, regressão completa, laboratório de UAC, rede, impressão, perfil e reinício |
| 7 · beta.9 | Modernização visual inspirada no Windows 11 e na referência enviada, com temas Escuro, Claro e Sistema | Escuro na primeira abertura; preferência persistida; troca de tema durante a sessão; alto contraste; DPI; diálogos e gráficos legíveis em Windows 10 e 11 |
| 8 · beta.10 | Protótipo e decisão de distribuição do PowerShell 7 dentro do pacote portátil | Windows 10 e 11 sem PS7 instalado, funcionamento offline, paridade das 44 ferramentas, UAC, módulos Windows, integridade dos arquivos e medição de tamanho, memória e abertura |

O critério de término de cada etapa é o aceite registrado e um ZIP próprio. A etapa seguinte começa após resolver regressões da anterior. O histórico de versões e os IDs existentes orientam a migração.

## Etapa visual após a beta.8

A imagem enviada pelo usuário apresenta fundo cinza escuro, cartões com bordas discretas e cantos arredondados, barras finas, ícones pequenos, títulos claros e informação compacta. Adotar essa hierarquia como referência e combinar superfícies neutras com o azul de destaque da identidade do aplicativo. O leiaute deve acomodar as tabelas e os diagnósticos extensos da ferramenta.

Na beta.9, acrescentar **Configurações → Aparência**, com **Escuro**, **Claro** e **Sistema**. A primeira abertura usa Escuro. Sistema acompanha a preferência do Windows e reage a mudanças durante a sessão. Salvar a escolha em local gravável da sessão e testar a abertura por pendrive e pasta somente leitura. O alto contraste do Windows tem prioridade sobre as paletas personalizadas.

Aplicar o tema também a menus, abas, seletores, tabelas, barras, estados de foco, seleção, gráficos e janelas de intervenção. Testar explicitamente os seletores que ainda mostram superfícies claras dentro do tema escuro. Usar ícones coerentes, Segoe UI, separação entre resumo e detalhe e estados indicados por texto e cor.

Prototipar primeiro no WinForms atual. Se a fidelidade visual e a acessibilidade exigirem outra camada de interface, comparar um protótipo WinUI 3 antes de decidir a migração. Avaliar Mica e outros efeitos conforme disponibilidade, com superfície sólida em ambientes sem suporte. Preservar o formato portátil como critério de decisão.

Referências oficiais para os temas e para os materiais do Windows. [Temas Win32](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-windows-themes) · [Materiais e comportamento em Windows 10](https://learn.microsoft.com/en-us/windows/apps/get-started/best-practices)

## PowerShell 7 incluído na distribuição

A revisão do código confirma que a beta.4 chama o Windows PowerShell 5.1 por caminho fixo, tanto nas consultas quanto na elevação. O pacote .NET do aplicativo já inclui seu runtime, mas esse empacotamento não incorpora automaticamente o PowerShell 7.

Há três caminhos para avaliar na beta.10.

| Alternativa | Funcionamento sem PS7 instalado | Consequência para este projeto |
| --- | --- | --- |
| PowerShell 7 portátil incluído | Distribuir os arquivos oficiais e executar o pwsh.exe por caminho interno | Aproveita a separação atual de processos; acrescenta runtime, módulos, licenças e arquivos ao pacote |
| Motor hospedado com Microsoft.PowerShell.SDK | Publicar o SDK e suas dependências com o aplicativo .NET self-contained | Permite runspaces dentro do processo; exige adaptar cancelamento, saída, módulos e isolamento, além de manter um processo separado para elevação |
| APIs .NET e Windows para operações específicas | Implementar diretamente em C# as operações escolhidas | Pode reduzir uso de scripts aos poucos; cada substituição precisa preservar parâmetros, permissões, resultados e comportamento em falha |

O SDK fornece a implementação do PowerShell, mas não garante que todos os módulos e recursos presentes no executável pwsh estejam disponíveis da mesma forma. Por exemplo, a documentação registra dependência de pwsh para Start-Job. Usar a versão de SDK e de .NET compatíveis entre si. [Hospedagem e pacotes oficiais](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/choosing-the-right-nuget-package)

O ZIP oficial permite executar pwsh a partir de uma pasta. Para a arquitetura atual, proponho começar a prova de conceito por esse processo incluído e comparar o SDK com as mesmas consultas. [Distribuição ZIP](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)

Auditar Storage, NetTCPIP, PrintManagement, CIM, diálogos WinForms, a DLL nativa compartilhada, a criação da cópia com ACL e a limpeza do processo elevado. A importação de módulos legados pode criar uma sessão em Windows PowerShell 5.1 e devolver objetos desserializados, o que precisa ser registrado na matriz. Incluir PS7 não elimina automaticamente essa dependência. [Compatibilidade com Windows PowerShell](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_windows_powershell_compatibility)

O aceite exige uma VM Windows 10 sem PS7 instalado e sem internet, além de Windows 11. Fixar edição e build compatíveis com o runtime escolhido e sua política de suporte. Medir custo de distribuição, atualizar o manifesto de integridade e a cópia protegida para o motor incluído, e repetir os testes de UAC com credenciais distintas. O protótipo deve indicar explicitamente qual motor executou cada ferramenta e quais rotinas dependeram do 5.1.

A adoção do novo motor depende dessa paridade. A investigação e os testes fazem parte da beta.10, e o resultado define se a prévia poderá oferecer o motor incluído para uso geral.

## Refatoração e desempenho

Centralizar nome, grupo, parâmetros, permissão, confirmação e timeout de cada ferramenta. O código hoje distribui essas decisões por seletores, dicionários, validações C# e ValidateSet do PowerShell. A refatoração precisa conferir os dois lados do contrato.

Extrair componentes de página apenas quando houver responsabilidade distinta. Manter o serviço de consultas separado da renderização e isolar a cópia para o clipboard para que falhas sejam recuperáveis.

Medir abertura fria e quente, memória após navegação, tempo de consulta, tempo de preencher e filtrar tabelas e quantidade de processos filhos após cancelamento. Fazer pelo menos três execuções por cenário no mesmo ambiente. Definir metas a partir dessa base. Cachear somente dados estáveis, com horário e atualização explícita. Dados de saúde e ocupação precisam conservar sua origem e atualidade.

## Referências examinadas

As aplicações abaixo inspiram padrões de organização. As propostas deste documento são decisões para este projeto.

- CrystalDiskInfo destaca estado, temperatura e identificação do disco. Usar como referência de hierarquia para Saúde, sem transformar ausência de SMART em diagnóstico positivo. [Documentação oficial](https://crystalmark.info/en/software/crystaldiskinfo/crystaldiskinfo-general-information/)
- Process Explorer usa lista e detalhes associados à seleção. Aplicável à separação entre resumo e evidência técnica. [Documentação oficial](https://learn.microsoft.com/en-us/sysinternals/downloads/process-explorer)
- WizTree documenta filtro e cópia de dados de arquivos e pastas. Aplicável ao mapa e à exportação da seleção. [Guia oficial](https://diskanalyzer.com/guide)
- A Microsoft orienta navegação clara e previsível e hierarquias rasas. Sua documentação de tipografia fornece referência para consistência de fonte. [Navegação](https://learn.microsoft.com/en-us/windows/apps/design/basics/navigation-basics) · [Tipografia](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography)
