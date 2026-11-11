# Homologação do Suporte TI Gráfico

**Versão em testes 1.0.0-beta.9.7 · Windows x64**

Extraia a beta e a 0.7.0 em pastas separadas. Compare o SHA-256 com o arquivo fornecido ao lado do ZIP. Abra os dois executáveis, confira o título de cada versão e use dados fictícios em todos os cenários de alteração. Registre versão do Windows, arquitetura, escala, tipo de conta, hash do ZIP e resultado de cada cenário.

## Ambientes e cobertura

| Ambiente | Evidência disponível | Aceite de laboratório |
| --- | --- | --- |
| Windows 11 Enterprise 25H2 x64, sessão padrão | Compilação, consultas CIM reais, automação WinForms, ZIP portátil, exportação EVTX e HTML | Repetir em VM limpa e offline, com pasta somente leitura e pendrive |
| Windows 10 x64 | Aguardando homologação | Abertura, CIM, PowerShell 5.1 e rotinas nativas na edição e no build declarados para suporte |
| Windows 11 x64, UAC e conta distinta | Simulações, validação do canal e verificação dos arquivos | Consentir e cancelar, elevar com credenciais distintas, inspecionar a cópia protegida e conferir a limpeza |
| Domínio, UNC e impressoras | Consultas locais e intervenções simuladas | GPO do usuário e do computador, pacote assinado por UNC, bloqueio de adulteração, fila de teste e spooler |
| Notebook, discos e acessibilidade | Ocupação real, SMART parcial e navegação automatizada | Bateria real, SATA/NVMe, DPI de 100%, 125% e 150%, alto contraste, teclado e foco |

## Estrutura da área de trabalho · beta.9.5

Conferir grupo e área no cabeçalho ao trocar guias, abrir Sobre/Configurações e retornar ao mesmo grupo. Recolher/expandir a lateral sem alterar a seleção. Em largura reduzida, conferir ocultação da contagem secundária e ausência de sobreposição. Comparar as superfícies e seleção de guias em Claro, Escuro e Sistema, com teclado e DPI. A moldura da etapa 2.2 usa cores DWM no Windows 11: conferir janela ativa/inativa, tema durante o uso, menu pelo ícone, minimizar/restaurar, maximizar, duplo clique, arraste e tamanho. Em laboratório, validar Snap por borda e layouts, monitores com escalas diferentes, DPI 100/125/150/200%, alto contraste entrando/saindo e Windows 10 com alternativa nativa. O sistema deve controlar os botões e os limites de maximização.

## Monitor CPU/RAM · etapa 3.1

Conferir cartões lado a lado em largura suficiente e empilhados na janela estreita. Iniciar a coleta real; verificar número principal, memória usada/disponível/total, escala fixa, média/pico e contagem de leituras válidas. Recolher/exibir histórico durante a coleta; pausar preserva os dados, trocar de ferramenta pausa e iniciar novamente limpa série e resumos. Repetir Claro/Sistema/Escuro, teclado, DPI e alto contraste. Comparar amostras da janela com os resumos, incluindo zero real, ausência e descarte após 60 posições; casos de contrato estão na suíte automatizada. Não interpretar a média como valor desde a abertura do programa.

## Navegação, gráficos e conexão

1. Em Escuro, Claro e Sistema, navegar pelas áreas e abas com mouse e teclado; conferir seleção, foco e títulos. Redimensionar para 780 × 560 e confirmar menu recolhido, acesso aos sete grupos por rolagem e Sobre/Configurações na base. Expandir manualmente, usar Ctrl+K e mudar entre as faixas de largura; a consulta e seus parâmetros não devem mudar ao recolher. Conferir as seis abas de Hardware e, quando houver excedentes, o acesso pela rolagem nativa e Ctrl+Tab. Rolar a página até resultados, Salvar log e Copiar resultado. Na tela de velocidade, conferir também o painel e o cancelamento.
2. Iniciar o monitor, conferir barras 0–100% e memória usada/disponível/total. Alternar Mostrar histórico de 60 amostras durante a coleta, conferir curvas e lacunas, pausar e iniciar novamente. No velocímetro, conferir unidade e escala crescente, zero real versus indisponibilidade, download/upload, falha parcial e cancelamento. Testar janela ampla/compacta nos três temas, teclado e DPI. Iniciar o monitor, observar amostras reais de CPU/RAM e pausar. Trocar de aba deve pausar o monitor e conservar os valores; nenhum movimento decorativo deve continuar nas telas ocultas.
3. Com a preferência de animações do Windows desativada, repetir a navegação e o monitor: as leituras continuam sendo atualizadas, sem transições. Repetir em alto contraste e escalas de 100%, 125%, 150% e 200% no laboratório; não considerar a implementação como substituta desse aceite real.
4. Executar o teste de internet manualmente, observando fase, tempo, bytes e taxa. Ele pode consumir até **1 GiB**. Conferir resultado de ambas as direções, cancelamento, falta de internet e falha em somente uma direção. Salvar/copiar resultado e confirmar avisos e método. Não comparar a estimativa diretamente com a velocidade contratada como critério único de aceite.
5. `Verificar-Aparencia.ps1` cobre temas, abas compactas, monitor e cancelamento; o roteiro de navegação adaptável acima complementa o verificador da beta.9.1. `-TestarVelocidadeInternet` habilita uma coleta real opcional, com o consumo acima. O teste automático do motor pode ser executado sem tráfego externo com `dotnet run --project Testes/SuporteTI.Grafico.Testes.csproj -c Release -- --testar-velocidade` a partir da pasta do projeto gráfico.

Se a sessão bloquear o próprio preparo do clipboard no verificador, `Verificar-Pacote.ps1 -Janela -SemClipboard` permite testar as demais rotas sem ler nem escrever nessa área. A saída identifica a exclusão; essa execução não conta como aceite de cópia. Repetir sem a opção em uma sessão com clipboard disponível. Não remover os testes de cópia nem alterar o aplicativo para contornar uma falha no preparo do ambiente.

## Cenários que alteram o Windows

Antes das intervenções, revisar a aparência nos modos Escuro, Claro e Sistema. O verificador `Verificar-Aparencia.ps1 -Zip CAMINHO_DO_ZIP -Capturas PASTA_DE_SAIDA` isola as preferências, confere persistência, resultado preservado, título/ícone, janela compacta e cancelamento das confirmações. A opção `-Tema` de `Verificar-Pacote.ps1 -Janela` permite repetir a regressão de navegação e consultas em cada modo.

O aceite manual ainda deve cobrir escalas 100/125/150/200%, transições reais de alto contraste e personalização do Windows com Sistema selecionado, guias por teclado, foco, ícone pequeno no título, janela ativa/inativa, seletores abertos, gráficos e diálogos administrativos em Windows 10 e 11. Usar somente contas e dados de laboratório nas operações sensíveis.

1. Crie um snapshot da VM e uma conta de teste sem dados reais. Use também um volume descartável e um adaptador de teste. Registre o estado inicial.
2. Em **Impressão**, crie trabalhos em duas filas locais. Reinicie o spooler e confirme que os trabalhos permanecem. Limpe uma fila e confira o resultado e a outra fila. Exercite o cache do Windows Update com reinício pendente e a atualização das GPO por escopo.
3. Em **Hardware → Programas**, teste o cache do Chrome com o navegador fechado. Confira que as preferências continuam disponíveis. Em **Rede**, mude para IPv4 fixo e volte para DHCP. Compare endereços, gateway e DNS antes e depois.
4. Em **Windows → Integridade e reparos**, execute primeiro os modos de consulta. Depois teste SFC e DISM com reparo e registre seus códigos de saída. Em **Hardware → Discos**, execute CHKDSK completo no volume descartável. Em um volume ocupado, confirme o agendamento na janela nativa, reinicie manualmente e verifique o evento Wininit.
5. Prepare um candidato de perfil de teste com dados descartáveis. Confira o bloqueio com a sessão carregada. Após o logoff, revise conta, SID, pastas e Registro. Cancele uma vez, confirme em nova execução e verifique que as demais contas continuam utilizáveis.
6. Configure Autologon com uma conta dedicada de teste. Confira o próximo logon, desative o recurso e confirme o retorno ao login manual após reiniciar. Verifique que os resultados exportados contêm apenas o estado da operação. Restaure o snapshot.

## Pacote assinado

Use um certificado de assinatura de código confiável e o servidor de carimbo autorizado para ele. O empacotador assina os scripts em uma cópia de compilação antes de incorporar seus hashes ao aplicativo e assina o executável antes de gerar o ZIP.

```powershell
.\Construcao\Publicar.ps1 -CertificadoThumbprint $thumbprint -ServidorCarimbo $url -Destino .\outputs\assinado
.\Testes\Verificar-Pacote.ps1 -Zip .\outputs\assinado\SuporteTI-Grafico-1.0.0-beta.9-win-x64.zip -ExigirAssinatura -ExigirCim -Janela
```

Para verificar a origem UNC, extraia o pacote assinado no compartilhamento e abra pelo caminho UNC. Repita uma consulta administrativa com conta distinta. Em uma cópia descartável, altere um script administrativo e confirme a rejeição. O pacote local sem assinatura cobre a regressão desta beta. A assinatura e os aceites em VM continuam como critérios de promoção.
