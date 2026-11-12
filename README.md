# Suporte TI Gráfico

**Versão atual · 1.0.0-beta.9.9**

**Windows x64 · edição portátil · prévia de homologação**

Ferramenta gráfica de diagnóstico e manutenção do Windows para técnicos de TI e uso pessoal, baseada no [Suporte TI para terminal](https://github.com/gustavo-brandao/SuporteTI).

**[Baixar ZIP portátil](https://github.com/gustavo-brandao/SuporteTI-Desktop/releases/download/v1.0.0-beta.9.9/SuporteTI-Grafico-1.0.0-beta.9.9-win-x64.zip)** · [SHA-256](https://github.com/gustavo-brandao/SuporteTI-Desktop/releases/download/v1.0.0-beta.9.9/SuporteTI-Grafico-1.0.0-beta.9.9-win-x64.zip.sha256) · [Notas e validação](docs/RELEASE-beta9.9.md)

![Catálogo de ferramentas na busca compacta](docs/imagens/busca-previa-beta99.png)

A etapa **4.1** traz busca central com filtro por grupo, resultados contextualizados e prévia de permissões, finalidade e impactos. **4.2** aproxima o monitor da composição aprovada: título único, CPU/RAM empilhados, ícones, porcentagens alinhadas e barras mais presentes. O histórico começa recolhido; a consulta pontual continua em **CPU e processos**.

**4.3 — parada de atividades:** SFC sem reparo oferece **Parar verificação**. No reparo SFC, **Parar SFC…** mostra a recomendação da Microsoft de aguardar 100% e oferece **Encerrar mesmo assim**, com **Continuar aguardando** como padrão. O processo iniciado pelo aplicativo é encerrado, a limpeza é aguardada e o resultado incompleto conserva a saída parcial. Alterações já feitas não são desfeitas. Não encerra serviços de manutenção do Windows nem outros processos SFC.

![Prévia de CPU e memória na busca compacta em Claro](docs/imagens/busca-compacta-claro-beta99.png)

As 44 operações continuam disponíveis. [Comparação com a referência e diferenças restantes](docs/REVISAO-VISUAL-beta9.9.md). Novas animações e a redistribuição dos demais menus continuam no planejamento; esta versão não declara todos os mockups integralmente reproduzidos.

## Começar

1. Baixe o ZIP portátil da versão acima.
2. Extraia todo o conteúdo para uma pasta local ou pendrive. Mantenha **Modulos** e **Documentacao** ao lado do executável.
3. Abra **Suporte TI Grafico.exe**. O pacote inclui o runtime .NET e usa o Windows PowerShell 5.1 da máquina.
4. Use **Salvar log…** quando precisar guardar o resultado de uma consulta. Operações que exigem permissões administrativas apresentam o UAC ao serem executadas.

Para conferir a integridade, execute `Get-FileHash -Algorithm SHA256` sobre o ZIP e compare com o arquivo `.sha256` da mesma release. Extraia a pasta completa; o aplicativo não possui instalador.

A beta está sem assinatura Authenticode. Os testes de laboratório com intervenções reais, elevação com outra conta e execução por compartilhamento seguem na [matriz de homologação](Suporte%20TI%20Grafico/Testes/HOMOLOGACAO.md).

## O que a ferramenta faz

- **Visão geral e Hardware** — inventário, monitor de CPU e memória, saúde SMART, gráficos de ocupação e mapa de pastas.
- **Rede e Windows** — adaptadores, ping, eventos, pesquisa de códigos de erro, Windows Update e integridade.
- **Impressão** — diagnóstico, reinício do spooler e limpeza de fila selecionada.
- **Contas e políticas** — GPO do usuário e computador, perfis locais e login automático.
- **Relatórios** — exportação TXT, HTML e EVTX.

Use a busca para encontrar uma ferramenta pelo nome, assunto ou identificador. Palavras podem ser combinadas; acentos e maiúsculas não alteram a correspondência. As abas agrupam as operações por assunto e o indicador **(UAC)** identifica as ferramentas administrativas.

**Ctrl+K** abre a busca sem expandir a lateral nem alterar a tela de origem. **↑/↓** e **Page Up/Down** selecionam; **Enter** prepara o formulário, sem executar; **Esc** volta ou fecha. Em largura reduzida, **Ver detalhes** ou **Alt+→** abre a prévia; **Alt+←** retorna aos resultados. Busca vazia lista o catálogo; sem correspondências, Abrir fica desabilitado. A lateral se recolhe abaixo de 1.000 pixels efetivos de largura da área cliente; o botão de menu permite alternar manualmente. A escolha manual permanece na faixa de largura atual. Abaixo desse limite, os ícones conservam seus nomes acessíveis e dicas. Sobre e Configurações ficam na base da lateral; a lista de grupos pode rolar em janelas baixas.

As abas permanecem em uma linha. Quando não couberem, use os controles nativos de rolagem ou **Ctrl+Tab/Ctrl+Shift+Tab**. A largura e o DPI determinam quanto conteúdo fica visível. A janela mantém o mínimo de 780 × 560; resultados e comandos adicionais continuam acessíveis pela rolagem.

O seletor reúne **35 entradas para as 44 operações**. A busca continua encontrando todas elas e preenche o formulário correspondente. Selecionar um modo ou ação prepara a operação para revisão. Os atalhos de diagnóstico permitem consultar antes de fazer alterações.

O filtro aguarda 200 ms sem novas teclas antes de atualizar a tabela. Limpar o campo restaura os registros imediatamente. Os avisos de leitura parcial permanecem visíveis. Confira as [medições da beta.8](docs/DESEMPENHO-beta8.md), com três amostras por cenário e as pendências de homologação.

Use a coluna **Copiar** para um valor, **Ctrl+C** para as linhas selecionadas ou **Copiar resultado** para o relatório completo. O menu de contexto oferece as mesmas ações e o rodapé confirma a cópia. O clique nos códigos de erro abre sua explicação.

## Teste de velocidade

Em **Rede → Conectividade → Velocidade da internet**, acompanhe latência HTTPS, download e upload. O painel distingue a taxa provisória do resultado final, mostra cada direção em seu próprio cartão e conserva os avisos na tabela. Cancelar remove a taxa e a agulha, sem converter a leitura provisória em resultado. Uma direção indisponível não apaga a outra que tenha dados válidos. Em largura reduzida, os cartões passam para baixo do arco, com rolagem da página. São quatro transferências paralelas por direção, por até dez segundos ou **512 MiB por direção (até 1 GiB total)**. O painel mostra a fase, Mbps, tempo e dados; cancelamento, cópia e **Salvar log…** permanecem disponíveis. O resultado estima a vazão útil até a Cloudflare e pode variar com Wi-Fi, VPN, proxy e tráfego concorrente.

O monitor mantém amostras reais de CPU e memória a cada segundo, com cartões empilhados e barras horizontais em escala fixa de 0–100%. **Ver histórico** revela as curvas de até 60 amostras, sem interromper a coleta. Média e pico usam somente leituras válidas. Pausar conserva a leitura; iniciar novamente limpa a sessão; trocar de ferramenta pausa o monitor. O movimento existente respeita as preferências do Windows.

![Velocímetro da beta.9.8 com resultados finais reais por direção](docs/imagens/velocidade-resultado-beta98.png)

A [entrega integrada de monitoramento](docs/RELEASE-beta9.8.md) registra os estados, verificações e limites desta versão. A [beta.9.3](docs/RELEASE-beta9.3.md) preserva o histórico da primeira implantação desses componentes.

## Aparência

Em **Configurações → Aparência**, escolha **Escuro**, **Claro** ou **Sistema**. A primeira abertura usa Escuro. A preferência é salva por usuário e a troca é aplicada durante o uso, preservando resultados. Alto contraste do Windows tem prioridade. O pacote continua portátil, com configuração fora da pasta do executável.

![Monitor reorganizado da beta.9.9](docs/imagens/monitor-escuro-beta99.png)

As capturas da busca e do monitor são reais da beta.9.9. A imagem do velocímetro registra a beta.9.8, cujo comportamento foi preservado. O rodapé que identifica a estação é recortado nas imagens da janela principal. Os mockups permanecem na documentação de propostas. A inspeção desta etapa conferiu aparência escura via Sistema e tema Claro; DPI, alto contraste e a matriz completa de ambientes continuam em homologação.

## Requisitos e arquitetura

| Componente | Implementação |
| --- | --- |
| Distribuição | Windows x64, ZIP portátil com .NET Desktop Runtime incluído |
| Interface | C# / .NET 10 / Windows Forms, com controles nativos e desenho dos componentes visuais |
| Consultas e manutenção | Módulos Windows PowerShell 5.1 locais, com contrato JSON versão 1 e operações permitidas explicitamente |
| Monitor e internet | Coleta nativa em C#; teste HTTPS até a Cloudflare, iniciado manualmente |
| Permissões | Janela principal na sessão atual; processo separado e validado para operações com UAC |
| Preferências | Tema por usuário em LocalAppData; resultados em memória e exportação manual |

O ambiente de validação local é Windows 11 Enterprise 25H2 x64. Windows 10, execução em VM limpa/offline, pendrive físico, conta administrativa distinta e matriz completa de DPI/alto contraste permanecem em homologação. PowerShell 7 ainda não acompanha o pacote.

## Compilação e testes

Com o SDK .NET 10 em Windows x64, a partir da raiz do repositório:

```powershell
dotnet build '.\Suporte TI Grafico\SuporteTI.Grafico.csproj' -c Release
dotnet run --project '.\Suporte TI Grafico\Testes\SuporteTI.Grafico.Testes.csproj' -c Release -- --exigir-cim
& '.\Suporte TI Grafico\Testes\Testar-Intervencoes.ps1'
& '.\Suporte TI Grafico\Construcao\Publicar.ps1'
```

O publicador gera ZIP e SHA-256 em `Suporte TI Grafico/outputs`; uma versão existente não é sobrescrita. A suíte verifica contrato, catálogo, permissões, falhas, cancelamento, logs, medições e consultas reais. A etapa local registrou **173 testes automatizados e 13 simulações de intervenção aprovados**. A revisão local conferiu coleta, histórico, pausa/reinício, troca de ferramenta, medição e cancelamento reais, temas e largura reduzida. Os testes da janela e suas pendências estão em [Resultados](Suporte%20TI%20Grafico/Testes/RESULTADO.md); simulações não substituem manutenção real em laboratório.

## Documentação e código

- [Pesquisa visual moderna: Linear, Raycast, Warp e painéis de monitoramento](docs/PESQUISA-VISUAL-MODERNA-2026-11-11.md) — exploração de identidade própria, composição e movimento além do padrão Windows.
- [Próxima revisão: legibilidade, Versão Desktop, busca, bordas e movimento](docs/REVISAO-ACABAMENTO-2026-11-11.md) — planejamento após a avaliação da beta.9.3; exemplos para escolha antes da implementação.
- [Manual, requisitos e compilação](Suporte%20TI%20Grafico/README.md)
- [Escopo e paridade com a edição CLI](Suporte%20TI%20Grafico/ESCOPO.md)
- [Resultados dos testes](Suporte%20TI%20Grafico/Testes/RESULTADO.md)
- [Roteiro de homologação](Suporte%20TI%20Grafico/Testes/HOMOLOGACAO.md)
- [Histórico das prévias](docs/PLANEJAMENTO.md)
- [Proposta de dashboard: cinco menus amplos/compactos e análise das 44 operações](docs/propostas-visuais/2026-11-10/conjunto-monitoramento/README.md) — conceitos para avaliação, separados das capturas reais acima.

O projeto usa WinForms em C# com .NET 10 e reaproveita módulos em Windows PowerShell 5.1. A base de referência é a edição CLI 0.1.22. [Revisão de referência](https://github.com/gustavo-brandao/SuporteTI/tree/1e3e0109e8135a5b3a80fdf0f0a1b1dab7f6962e)

## Créditos e licença

Desenvolvido por **Gustavo Brandão Gimenez**, com contribuições do **OpenAI Codex** na implementação, nos testes e na documentação.

Código sob a [licença MIT](LICENSE). O smartmontools é distribuído com [licença própria](Ferramentas/smartmontools/doc/COPYING.txt) e código-fonte na pasta de terceiros.
