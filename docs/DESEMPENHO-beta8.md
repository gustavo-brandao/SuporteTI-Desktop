# Medições da beta.8

Comparação local em 10 de novembro de 2026, Windows 11 x64, mesma estação e resolução. Referência: beta.7, commit e7b3300. Candidata: alterações da beta.8 sobre essa referência. Três repetições por cenário, sem limpar cache do sistema operacional ou alterar configurações da estação. São medições exploratórias, não uma garantia para outros computadores.

## Resultados

| Cenário | Mediana beta.7 | Mediana beta.8 |
| --- | ---: | ---: |
| Preencher 1.000 campos e um aviso | 1.386,95 ms | 41,67 ms |
| Seis alterações consecutivas no filtro | 6.707,05 ms | 251,41 ms |
| Criar e mostrar a janela no medidor | 121,37 ms | 132,22 ms |
| Navegar pelas 44 operações | 1.437,34 ms | 1.513,27 ms |
| Memória privada após navegação | 47,65 MiB | 42,00 MiB |
| ZIP: abrir com cache de extração vazio | 323,16 ms | 326,88 ms |
| ZIP: abrir com cache de extração reutilizado | 337,08 ms | 311,04 ms |
| Consulta real do sistema pelo coletor | 1.729,68 ms | 1.698,09 ms |

A melhoria demonstrada se concentra no preenchimento e no filtro. O teste de seis alterações inclui a espera de 200 ms do filtro novo e conserva as 11 correspondências mais o aviso. Navegação e criação da janela variaram para cima; memória, abertura do ZIP e consulta real oscilaram entre amostras. Não se estabelece ganho geral nesses cenários a partir de apenas três repetições.

Na primeira abertura de cada versão, o ZIP levou 4.873,66 ms e 3.876,64 ms, respectivamente. As outras duas aberturas com cache de extração vazio ficaram entre 307 e 327 ms. Esse primeiro custo não foi descartado dos dados, e seu comportamento exige mais ambientes antes de definir uma meta de abertura fria.

Os handles passaram de 350 para 377 na primeira navegação, de 377 para 377 na segunda e de 379 para 379 na terceira, nas duas versões. Esse cenário não mostrou crescimento por navegação nas repetições aquecidas. A contagem não substitui uma análise prolongada de memória ou de recursos GDI.

## Método e reprodução

O projeto [Desempenho](../Suporte%20TI%20Grafico/Testes/Desempenho/Desempenho.csproj) carrega a DLL compilada de cada versão, abre a janela real e usa os mesmos 1.000 campos sintéticos com Unicode e valores extensos. Mede a apresentação, o filtro e os 44 destinos sem executar intervenções. A coleta de memória privada ocorre após GC nos mesmos pontos. A criação da janela nesse medidor não inclui o início do runtime do processo nem a extração do ZIP.

```powershell
dotnet build './Suporte TI Grafico/Testes/Desempenho/Desempenho.csproj' -c Release
dotnet './Suporte TI Grafico/Testes/Desempenho/bin/Release/net10.0-windows/Desempenho.dll' 'CAMINHO/DA/DLL/Suporte TI Grafico.dll'
```

[Medir-Pacote.ps1](../Suporte%20TI%20Grafico/Testes/Desempenho/Medir-Pacote.ps1) extrai um ZIP temporário e mede até o seletor de ferramentas estar disponível. Foram usados o ZIP público da beta.7 e o ZIP candidato local da beta.8; esse ensaio não controla diferenças de runtime incluído pelo SDK de cada empacotamento. Para cada repetição, abre com diretório de extração .NET novo e depois o reutiliza. A consulta do sistema é medida no coletor PowerShell, sem o transporte da interface. O medidor salva somente tempos e estado, sem dados do computador.

```powershell
& './Suporte TI Grafico/Testes/Desempenho/Medir-Pacote.ps1' -Zip 'CAMINHO/DO/PACOTE.zip' -Saida './medicao.json'
```

Dados brutos: [janela beta.7](medicoes/beta8/antes.json), [janela beta.8](medicoes/beta8/depois.json), [ZIP beta.7](medicoes/beta8/pacote-antes.json), [ZIP beta.8](medicoes/beta8/pacote-depois.json).

## Encerramento e homologação

A regressão cria apenas processos de teste. Três cancelamentos, uma conclusão com filho ainda ativo e um prazo excedido terminaram sem processos restantes nos cenários verificados. O controle usa um [Job Object do Windows](https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects) próprio para cada consulta não elevada. O canal elevado não foi alterado.

O Hyper-V recusou o acesso da sessão. Manutenção real em VM, rede e impressão de laboratório, perfil, reinício, conta administrativa distinta, UNC assinado, Windows 10, DPI e alto contraste continuam pendentes na [matriz](../Suporte%20TI%20Grafico/Testes/HOMOLOGACAO.md). A beta.8 continua sendo uma prévia de testes.
