# Suporte TI Gráfico

**1.0.0-beta.1** é uma prévia de homologação para Windows x64, destinada a técnicos de qualquer empresa e a uso pessoal. A interface WinForms usa tema azul escuro e respeita a paleta de alto contraste do Windows. O rodapé mostra a máquina e o nível da sessão atual.

## Usar a prévia

1. Extraia todo o ZIP em uma pasta local ou no pendrive.
2. Abra **Suporte TI Grafico.exe**, identificado pelo ícone de um monitor com um prompt de terminal.
3. Em **Computador**, escolha um diagnóstico e selecione **Consultar**. A lista cobre resumo do sistema, CPU e memória com os cinco processos de maior uso, volumes e saúde dos discos, bateria, problemas de dispositivos, serviços, programas registrados e estado do login automático. Use o filtro para localizar resultados na tabela. O prazo é de 25 segundos para consultas gerais e 45 segundos para discos.
4. Em **Monitor ao vivo**, escolha **Iniciar monitor** para ver CPU e memória com valores e gráficos separados. A leitura ocorre a cada segundo e guarda os últimos 60 pontos. Pausar conserva a visualização. Iniciar novamente começa um novo histórico. Trocar de tela pausa o monitor.
5. Em **Mapa de disco**, escolha um volume local e selecione **Analisar volume**. A barra superior mostra a ocupação do volume. Os blocos e a lista mostram os tamanhos lógicos lidos na pasta, com seleção sincronizada. Use duplo clique ou Enter na lista para abrir uma pasta. Use o caminho clicável, **Pasta anterior** ou Backspace na lista para voltar um nível. O mapa agrupa os itens além dos 30 maiores. A lista mostra até 250 itens por pasta.
6. Em **Rede**, consulte adaptadores com IP ativo e o ingresso em domínio. O ping faz quatro tentativas ICMP para um host, IP ou URL informado. O teste de velocidade estima download e upload com a Cloudflare e só envia tráfego quando acionado.
7. Em **Falhas do Windows**, consulte falhas das últimas 24 horas ou pesquise códigos em decimal e hexadecimal. Clique em um código destacado para preencher o campo e ver automaticamente o significado e a referência disponível. No teclado, selecione a linha e pressione Enter. Na pesquisa manual, digite apenas o código. Quando o mesmo valor aparecer em mais de uma família, a tabela mostra as interpretações encontradas.
8. Em **Atendimento**, consulte serviços, políticas, reinício pendente, espaço e eventos do Windows Update. Códigos HRESULT identificados nos eventos de atualização também abrem a pesquisa. A área inclui diagnóstico de impressão e consulta e exportação em HTML das políticas do usuário da sessão.
9. Em **Relatórios**, reveja o último resultado da sessão ou consulte até 30 avisos e erros de cada canal System e Application nas últimas 24 horas. A exportação EVTX inclui Application, System e Setup no mesmo período e registra em um manifesto SHA-256 os arquivos gerados com sucesso. Falhas por canal aparecem no resultado.
10. Use **Salvar log…** para exportar o resultado completo com horários e avisos, inclusive se houver um filtro na tela. A pasta escolhida recebe uma subpasta por máquina e execução com `resultado.txt` em UTF-8. O diálogo abre em `Documentos\SuporteTI\Resultados` quando disponível, ou em Documentos.
11. Em **Administrativo**, escolha saúde dos discos, GPO do computador, SFC `/verifyonly`, DISM `CheckHealth` ou `ScanHealth`, ou CHKDSK de consulta. Para CHKDSK, selecione um volume fixo local. Os resultados aparecem na janela e podem ser salvos em TXT. Para GPO, também é possível exportar o HTML do computador.

As consultas de discos em **Computador** e **Administrativo** apresentam barras de ocupação por volume e de desgaste estimado quando o disco informa essa métrica. O resultado textual mostra o estado SMART e avisa sobre leituras parciais.

## Intervenções de suporte

- Em **Atendimento**, **Reiniciar spooler** preserva os trabalhos. **Limpar fila de impressão** pede uma fila local e cancela os trabalhos encontrados nela. O resultado informa quantos foram cancelados e quantos falharam.
- Em **Atendimento**, **Recriar cache do Windows Update** renomeia o cache de download, preserva a cópia anterior e tenta restaurar o estado dos serviços. Reinício pendente e manutenção ativa bloqueiam a operação. A atualização de GPO separa usuário da sessão e computador.
- Em **Computador**, **Limpar cache do Chrome** limpa o cache HTTP de perfis padrão da sessão. Feche o Chrome antes. Preferências e outros dados fora da pasta Cache são preservados.
- Em **Rede**, escolha DHCP ou IPv4 fixo e selecione o adaptador físico. O resultado mostra o estado antes e depois. Se uma etapa falhar, confira as alterações já aplicadas. Alterar a conexão pode interromper o suporte remoto.

Cada intervenção exibe seu impacto e pede confirmação. A limpeza do Chrome e a atualização de GPO do usuário executam na sessão atual. As ações de spooler, Windows Update, GPO do computador e IPv4 solicitam UAC por operação.

## Reparos e alterações de conta

Em **Administrativo**, escolha a operação, use **Revisar ação…** e confira o impacto antes de confirmar o UAC.

- **SFC · reparar arquivos** usa `/scannow`. **DISM · reparar imagem** usa `/RestoreHealth /NoRestart`. O resultado registra a saída e o código retornado pelo Windows. Aguarde o término.
- **CHKDSK · reparo completo** aplica `/f /r` ao volume fixo selecionado. A janela nativa pode oferecer agendamento para o próximo reinício. Responda nela. A tela gráfica registra o código de saída e indica onde encontrar o log do Windows. Reinicie manualmente se tiver aceitado agendar.
- **Revisar perfis locais duplicados** abre os candidatos na janela elevada, com conta, SID, pastas, entradas do Registro e bloqueios. A exclusão exige confirmação separada e nova conferência do perfil. Encerre a sessão da conta alvo e confira a cópia dos dados antes de autorizar.
- **Configurar login automático** recebe a senha apenas na janela elevada e armazena a credencial como segredo LSA. Use conta dedicada de privilégios limitados. **Desativar login automático** desliga a configuração e remove a credencial. Verifique o próximo logon manualmente.

## Acesso administrativo

Abra o executável normalmente. A janela usa as permissões da sessão e as consultas comuns ficam disponíveis sem elevação. Ao executar uma consulta de **Administrativo** ou uma intervenção que exija permissão, o aplicativo inicia um processo separado pelo UAC. Use o próprio diálogo do Windows para autorizar ou cancelar. Uma conta sem direitos administrativos pode precisar informar credenciais de uma conta autorizada. O aplicativo recebe o resultado pelo canal local e salva o HTML no destino escolhido com as permissões da sessão original.

As consultas administrativas desta prévia verificam o estado do Windows e dos discos, e os comandos nativos podem registrar informações nos logs do sistema. As verificações podem levar até 30 minutos. Depois de autorizar o UAC, aguarde o resultado na janela.

Antes da execução administrativa, a sessão original compara os arquivos com as versões embutidas no executável e envia os dados verificados para o processo elevado. Ele grava a cópia em uma pasta protegida em `Windows\Temp`, preserva a marca de origem, confere os hashes e remove a pasta ao final. Para usar o modo administrativo por compartilhamento, abra pelo caminho UNC com scripts administrativos assinados e empacotados depois da assinatura. A política de execução do Windows continua aplicável.

A raiz do ZIP contém o executável e as pastas **Modulos** e **Documentacao**. Mantenha esses itens juntos ao mover o aplicativo. O executável inclui o .NET Desktop Runtime x64. O .NET extrai os componentes nativos para o cache local do usuário ao iniciar. A consulta usa o Windows PowerShell 5.1 da própria máquina, roda localmente na identidade da sessão, em modo de leitura e sem pedir elevação. Os resultados ficam na janela.

O pacote local desta beta está sem assinatura. O processo do PowerShell usa RemoteSigned, sujeito à política do Windows. Caso a origem do ZIP seja marcada como Internet, verifique sua procedência e o SHA-256 antes de seguir o procedimento de autorização do ambiente. Para testar lado a lado, extraia esta beta e a 0.7.0 em pastas separadas e confira a versão no título antes de operar.

A análise do mapa pode ser cancelada e mostra resultados parciais quando encontra itens inacessíveis, pontos de reanálise ou limites de leitura. Os limites são 350 mil entradas, 90 segundos e 64 níveis. O total lido pode diferir da ocupação do volume.

A saúde dos discos usa os indicadores do Windows e o smartmontools 7.5 para até quatro discos internos compatíveis. Indicadores indisponíveis ficam sinalizados. O inventário de programas usa o Registro. A bateria depende das capacidades publicadas pelo Windows. A consulta de login automático lê apenas o estado e a presença de configuração legada.

O smartmontools é distribuído em `Modulos/Ferramentas/smartmontools` com seu código-fonte e licença própria. O executável e a base de discos são conferidos por SHA-256 antes de cada leitura.

## Construir e testar

Em Windows x64 com o SDK .NET 10 disponível, execute a partir desta pasta.

```powershell
dotnet run --project .\Testes\SuporteTI.Grafico.Testes.csproj -c Release
.\Construcao\Publicar.ps1
```

O publicador gera o ZIP e o SHA-256 em `outputs`. Use `-Dotnet` para indicar um SDK portátil ou `-Destino` para escolher uma pasta nova. Acrescente `-- --exigir-cim` ao comando de testes em uma estação com acesso a CIM para conferir nome e build reais.

O roteiro **HOMOLOGACAO.md**, incluído em **Documentacao**, descreve os ambientes, os cenários em VM e a geração de um pacote assinado. [Roteiro de homologação](Testes/HOMOLOGACAO.md)

## Origem

Desenvolvido por Gustavo Brandão Gimenez, sob a [licença MIT](LICENSE). A referência funcional é o Suporte TI para terminal 0.1.22 no commit `1e3e0109e8135a5b3a80fdf0f0a1b1dab7f6962e`. O projeto gráfico mantém sua versão, fontes e pacote próprios.
