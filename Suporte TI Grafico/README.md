# Suporte TI Gráfico

**1.0.0-beta.5** é uma prévia de homologação para Windows x64, destinada a técnicos de qualquer empresa e a uso pessoal. A interface WinForms usa tema azul escuro e respeita a paleta de alto contraste do Windows. O rodapé mostra a máquina e o nível da sessão atual.

## Usar a prévia

A beta.3 normaliza o desgaste dos discos para número ou valor indisponível antes de enviar os resultados à interface. O plano de reorganização está em [Revisão de UX e diagnósticos](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/docs/REVISAO-UX-2026-11-09.md).

1. Extraia todo o ZIP em uma pasta local ou no pendrive.
2. Abra **Suporte TI Grafico.exe**, identificado pelo ícone de um monitor com um prompt de terminal.
3. Em **Visão geral**, use **Consultar computador** para ler o resumo do sistema.
4. Em **Hardware**, escolha a aba **Desempenho**, **Discos**, **Bateria**, **Dispositivos**, **Serviços** ou **Programas** e selecione a ferramenta. **Discos** reúne a saúde administrativa, os volumes, o mapa e os modos de CHKDSK. O indicador **(UAC)** identifica as ações que exigem elevação.
5. Em **Hardware → Desempenho → Monitor ao vivo**, inicie ou pause a amostragem de CPU e RAM. Em **Hardware → Discos → Mapa de disco**, analise um volume e navegue por pastas. Trocar de ferramenta pausa o monitor.
6. Em **Rede**, use as abas **Conexão**, **Conectividade** e **Configuração IPv4**.
7. Em **Windows**, use **Eventos e códigos** para ler eventos e consultar significados. Clicar em um código destacado preenche a pesquisa e abre a explicação. As abas **Windows Update** e **Integridade e reparos** reúnem seus respectivos diagnósticos e ações.
8. Em **Impressão**, o diagnóstico, o reinício do spooler e a limpeza da fila ficam juntos. A limpeza pede a fila local que será afetada.
9. Em **Contas e políticas**, escolha **Políticas de grupo**, **Perfis locais** ou **Login automático**. As políticas de usuário e computador compartilham a mesma aba, com seus escopos identificados.
10. Em **Relatórios**, recupere o último resultado ou exporte eventos em EVTX com manifesto SHA-256. **Salvar log…** continua disponível nos resultados das consultas e exporta também os dados ocultos pelo filtro.
11. Digite parte do nome na **Buscar ferramenta**. A busca aceita palavras sem acento. Use duplo clique no resultado ou seta para baixo e Enter para abrir. Encontrar uma ferramenta apenas abre sua tela. A execução exige a ação do técnico.
12. Use **Sobre** para conferir a versão e os créditos.

A saúde dos discos apresenta barras quando existe uma medição numérica. Uma leitura limitada ou indisponível conserva seu aviso. O formulário principal utiliza as permissões da sessão e solicita o UAC por operação.

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

O [planejamento de evolução](https://github.com/gustavo-brandao/SuporteTI-Desktop/blob/main/docs/REVISAO-UX-2026-11-09.md) prevê uma etapa visual após a beta.8, com temas Escuro, Claro e Sistema, e uma avaliação posterior de PowerShell 7 incluído na distribuição. A beta.4 utiliza Windows PowerShell 5.1 para suas rotinas.

Em Windows x64 com o SDK .NET 10 disponível, execute a partir desta pasta.

```powershell
dotnet run --project .\Testes\SuporteTI.Grafico.Testes.csproj -c Release
.\Construcao\Publicar.ps1
```

O publicador gera o ZIP e o SHA-256 em `outputs`. Use `-Dotnet` para indicar um SDK portátil ou `-Destino` para escolher uma pasta nova. Acrescente `-- --exigir-cim` ao comando de testes em uma estação com acesso a CIM para conferir nome e build reais.

O roteiro **HOMOLOGACAO.md**, incluído em **Documentacao**, descreve os ambientes, os cenários em VM e a geração de um pacote assinado. [Roteiro de homologação](Testes/HOMOLOGACAO.md)

## Origem

Desenvolvido por Gustavo Brandão Gimenez, sob a [licença MIT](LICENSE). A referência funcional é o Suporte TI para terminal 0.1.22 no commit `1e3e0109e8135a5b3a80fdf0f0a1b1dab7f6962e`. O projeto gráfico mantém sua versão, fontes e pacote próprios.
