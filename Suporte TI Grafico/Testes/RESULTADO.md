# Validação das prévias

## Prévia 1.0.0-beta.2

- O ZIP público foi gerado no [GitHub Actions](https://github.com/gustavo-brandao/SuporteTI-Desktop/actions/runs/42290808313) e tem SHA-256 **7fbc0488648e148072ab7fb2ae5c7520ee2f34f9a57d690a70675c083ada1438**.

- **64 testes gráficos e 13 simulações de intervenção aprovados** na estação Windows 11 x64. O novo teste reproduziu um dispositivo com nome em branco e confirmou o identificador PnP como rótulo.
- O ZIP local passou na abertura, navegação, consulta CIM, gráficos de discos, consulta de código de erro e exportações. A homologação de intervenções reais e assinatura segue no [roteiro](HOMOLOGACAO.md).

## Prévia 1.0.0-beta.1

- **63 testes gráficos e 13 simulações de intervenção aprovados** em Windows 11 Enterprise 25H2 x64. A consulta CIM real retornou os mesmos campos de identificação que a referência do terminal. Os dois diálogos sensíveis e os 204 testes do terminal passaram na etapa 0.7.0, cujas rotinas são as mesmas nesta beta.
- Os ZIPs extraídos da beta e da 0.7.0 abriram simultaneamente com títulos distintos. A beta passou na navegação, consulta de código, gráficos de discos, menu de impressão, monitor, cancelamento do mapa e exportações. Os três EVTX conferiram com o manifesto.
- A verificação estrita rejeitou o executável sem assinatura como esperado. A geração com Authenticode e o carimbo permanecem pendentes por falta de certificado de assinatura de código disponível.
- O Hyper-V da estação recusou o acesso às VMs. A [matriz e o roteiro de homologação](HOMOLOGACAO.md) registram os cenários pendentes, incluindo intervenções reais, UAC com conta distinta, UNC, reinício, pendrive, escalas e alto contraste.
- O ZIP local tem 49.974.546 bytes e SHA-256 `aebfed1565cd8c751ba9505c8bd56bee32a6e09f262f578250839803bbfbce06`.

## Prévia 0.7.0

- **63 testes gráficos**, **13 simulações de intervenção** e **204 testes de regressão do terminal** passaram em Windows 11 x64. O CHKDSK completo recusou argumentos livres, e a simulação confirmou os parâmetros fixos de SFC, DISM e CHKDSK.
- A automação abriu os diálogos com dados simulados e confirmou que perfil carregado fica bloqueado, o campo de senha usa máscara e fechar a janela cancela a operação.
- O ZIP extraído passou na abertura em sessão padrão e exibiu as seis opções novas em **Administrativo**. Os testes de consulta real, eventos, exportação, monitor, mapa e gráficos também passaram.
- O ZIP tem 49.972.252 bytes e SHA-256 `36f04d1f04634b92765e7d88f7e07c4dab5b90ced354bb9298afdc3d62417b46`.
- Reparos, agendamento e reinício reais, exclusão de perfil e Autologon permanecem pendentes de homologação em VM isolada.

## Prévia 0.6.0

- **62 testes automatizados aprovados** em Windows 11 x64, com consulta CIM real. A suíte verificou também a recusa de IPv4 sem adaptador e de limpeza sem fila selecionada.
- **9 simulações de intervenção aprovadas**. O reinício preservou os trabalhos, a limpeza ficou restrita à fila local selecionada e as falhas parciais foram registradas. Os cenários também cobriram DHCP, validação e falha parcial do IPv4 fixo, bloqueio do cache do Windows Update por reinício pendente, falha na restauração de serviço e preservação das preferências do Chrome.
- A janela do ZIP abriu com token padrão. A automação verificou as três opções de impressão em **Atendimento** e a coluna **Gráfico** na consulta de discos. O coletor retornou ocupação numérica real e sinalizou leitura parcial. O fluxo em **Falhas do Windows** consultou automaticamente o significado de um código simulado. O teste também verificou a opção de salvar log, a consulta comum, as exportações, o monitor e o cancelamento do mapa.
- A homologação de UAC e das alterações reais de spooler, cache, GPO e rede segue pendente em VM isolada.
- O ZIP tem 49.939.785 bytes e SHA-256 `f14905f81118be23a5b6be465a9dcf10c8a29e38ce3a03ae039080cd0d5c6cf6`.

## Prévia 0.5.0

- **60 testes automatizados aprovados** em Windows 11 x64. A suíte recusou reparos fora da lista, parâmetros livres do CHKDSK e um script administrativo alterado. O teste do canal rejeitou um PID diferente e trocou dados com o processo esperado. A gravação do HTML foi testada na sessão original.
- O executável do ZIP tem manifesto `asInvoker`. Na estação de teste com token padrão, o ZIP extraído abriu, concluiu uma consulta comum e exibiu o menu **Administrativo** sem pedir UAC. A suíte da janela também passou pelos fluxos de triagem, monitor e mapa.
- As exportações do usuário em HTML e EVTX passaram. Os hashes dos três EVTX conferiram com o manifesto.
- A homologação em VM ainda precisa executar uma consulta com o UAC autorizado e cancelado, inclusive com outra conta administrativa, conferir a cópia protegida e testar o pacote assinado em UNC. Os testes de canal desta rodada usaram processos da sessão de teste.
- O ZIP tem 49.899.301 bytes e SHA-256 `375d10038b33b9df0539be7206ce1c768a3a8c152bec39e2a7d939fe3de7de9d`.

## Prévia 0.4.1

- **55 testes automatizados aprovados** em Windows 11 x64. A pesquisa sem seleção de família retornou Win32 e Paradas para o código `5`, identificou NTSTATUS em `0xC0000005` e aceitou HRESULT em decimal com sinal.
- No ZIP extraído, o clique no código `0x000000EF` de um evento simulado preencheu o campo e exibiu `CRITICAL_PROCESS_DIED` automaticamente. A janela de pesquisa exibiu apenas o campo de código.
- O ZIP tem 49.318.489 bytes e SHA-256 `234379c07aca6765c412aa4f4dc0d7e77595d9537661e311309b2b50d2ff76da`.

## Consulta direta de códigos de erro

- A suíte gráfica aprovou 49 testes. No ZIP recompilado, a automação acionou o link `0x000000EF` em uma ocorrência simulada do Kernel-Power e confirmou que a pesquisa preencheu o código, selecionou **Paradas (tela azul)** e exibiu `CRITICAL_PROCESS_DIED` com o mesmo clique.

## Prévia 0.4.0

- **51 testes automatizados aprovados** em Windows 11 x64. A suíte verificou o código e a família da falha, as consultas de atendimento, o spooler parado, uma fila com trabalhos inacessíveis, o acesso negado aos eventos e a recusa de destino relativo.
- Na janela do ZIP extraído, um evento simulado do Kernel-Power produziu o link `0x000000EF`. O clique abriu o formulário de pesquisa com o código e a família **Paradas (tela azul)**. A automação também abriu Atendimento e recuperou o último resultado em Relatórios, com o salvamento em TXT disponível.
- O pacote final exportou o HTML das políticas do usuário da sessão e os três canais EVTX em uma pasta temporária. Os três hashes conferiram com o manifesto. O teste com caminho longo revelou um limite no argumento do `gpresult`, corrigido antes de gerar o pacote final.
- A exportação parcial de EVTX em um ambiente com um canal bloqueado e o acionamento do link pelo teclado ainda requerem verificação dirigida.
- O ZIP tem 49.317.632 bytes e SHA-256 `9dc9db465a770f1394f5ac9c25c2b2594218e7e63bf191cbabc03e6410f111fe`.

## Prévia 0.3.0

- **42 testes automatizados aprovados** em Windows 11 x64. A suíte cobriu o contrato das consultas de rede e triagem, destino de ping inválido, perda parcial, ausência de respostas, códigos decimal e hexadecimal, HRESULT decimal com sinal e catálogo indisponível.
- O teste de velocidade usou respostas simuladas. Uma falha no upload preservou a medição de download. O teste de salvamento gerou dois arquivos distintos e conferiu a presença da medição e do aviso no TXT.
- No ZIP extraído, foi conferida a consulta do código Win32 5. A automação da janela abriu **Rede** e **Triagem**, verificou que **Salvar log…** estava disponível após uma consulta e repetiu os fluxos de monitor e mapa.
- A transferência real para o serviço de medição e a seleção de destino no diálogo de salvamento seguem pendentes de verificação manual.
- O ZIP tem 49.310.461 bytes e SHA-256 `6b8403314b4ebf017e497dc54eda34650033a0925a1ff1668fe434f75fdc6445`.

## Prévia 0.2.0

- **31 testes automatizados aprovados** em Windows 11 x64. A suíte verificou os contratos dos diagnósticos, a amostra real de CPU e RAM, a soma de 640 bytes em uma árvore de teste, a ordenação por tamanho, o limite de itens e o cancelamento.
- O ZIP extraído manteve três itens na raiz. O coletor de discos foi executado a partir do pacote e sinalizou leitura parcial. Nome, modelo, build do Windows e versão da BIOS conferiram com o terminal.
- Na janela do pacote final, a automação abriu a consulta, iniciou o monitor, leu uma amostra real, pausou o monitor, entrou no mapa e cancelou a leitura.
- Pendências de homologação incluem a bateria de um notebook físico, VM limpa, pendrive físico, alto contraste e navegação por teclado em uma árvore de mapa preenchida. A cobertura de pastas inacessíveis e pontos de reanálise também requer verificação dirigida.
- O ZIP tem 49.169.899 bytes e SHA-256 `492ecc2be2c70d16e741db54f7859f902c22dbf3ad5223b18c1a54005a8434ee`.

## Prévia 0.1.1

- Compilação x64 concluída com zero avisos e zero erros. Os **15 testes automatizados** passaram com consulta CIM real.
- O ZIP extraído contém três itens na raiz. O hash, a versão do executável e a presença dos scripts, do README e da licença foram conferidos.
- A verificação dos recursos PE confirmou os nove tamanhos do ícone original no executável, de 16 a 256 px.
- A janela do executável extraído abriu. O teste navegou para **Sobre**, voltou para **Computador** e concluiu a consulta pela interface. Nome, modelo, build do Windows e versão da BIOS coincidiram com o terminal 0.1.22.
- O ZIP tem 47.455.270 bytes e SHA-256 `ab11090d46d9efeb42b9e35a06057c7d407bff91f80cdf7bd5d4ee38b9616ac7`.
- A homologação em VM limpa e em pendrive físico segue pendente. A suíte desta prévia foi executada na estação Windows 11 abaixo.

## Prévia 0.1.0

**6 de novembro de 2026 · Windows 11 Enterprise 25H2 x64, build 26200.9818 · SDK .NET 10.0.403**

| Verificação | Resultado |
| --- | --- |
| Testes automatizados | **15 aprovados**. Cobertura de estados concluído, parcial e indisponível, contrato inválido, versão incompatível, falha de processo, módulo ausente, tempo limite, cancelamento, caminho com espaços e acentos e conferência de dados reais do CIM. |
| Coletor no ambiente restrito | Resposta estruturada com estado indisponível e aviso nas cinco seções quando o Windows recusou o acesso ao CIM. |
| ZIP extraído | Hash, versão do executável, runtime e arquivos da consulta conferidos. O coletor extraído concluiu a leitura real. |
| Interface do ZIP | O executável abriu, exibiu **Sobre**, voltou a **Computador** e concluiu uma consulta disparada pelo botão. A navegação foi exercitada por UI Automation na estação local. |
| Paridade com o terminal 0.1.22 | Nome, modelo, build do Windows e versão da BIOS coincidiram com a consulta `sistema` da referência. CPU e RAM em uso são amostras pontuais. |
| Matriz de paridade | Os 36 identificadores da referência constam no [escopo](../ESCOPO.md). |

O pacote validado tem 51.505.156 bytes e SHA-256 `4259facf5271f9ebcd2d2612bfe1c1d0dbdcda8558afee6073af7363d1fd6795`.

## Repetir o teste

Execute a suíte e gere o ZIP conforme o [README](../README.md). Em uma sessão interativa de Windows PowerShell 5.1, a partir do subprojeto, execute também:

```powershell
.\Testes\Testar-Intervencoes.ps1
.\Testes\Testar-DialogosSensiveis.ps1
.\Testes\Verificar-Pacote.ps1 -Zip .\outputs\SuporteTI-Grafico-1.0.0-beta.2-win-x64.zip -ExigirCim -Janela
```

O argumento de referência é opcional e aponta para o módulo da edição de terminal. A opção `-Janela` abre o executável extraído, realiza o teste e fecha a janela.

## Homologação pendente

Repetir o aceite com usuário padrão em VM limpa e offline, inclusive a partir de pendrive físico. Registrar edição, build, DPI, tempo de abertura e resultado. A execução do workflow no GitHub fica disponível quando o código for enviado ao repositório.
