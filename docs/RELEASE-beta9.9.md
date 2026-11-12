# Suporte TI Gráfico 1.0.0-beta.9.9

Etapa **4.1 busca em camadas**, **4.2 revisão dirigida de fidelidade visual** e **4.3 parada do SFC**.

- Paleta central com filtro por grupo, prévia de finalidade/permissões/impacto e acesso às 44 operações. Enter prepara; Esc retorna. Layout em camadas na janela estreita.
- Título principal consolidado, monitor direto em Hardware, CPU/RAM empilhados com ícones e barras mais presentes. Histórico inicialmente recolhido; CPU e processos preserva consulta pontual.
- SFC sem reparo permite parar. O reparo mostra o aviso da recomendação Microsoft e oferece Encerrar mesmo assim após confirmação, com Continuar aguardando como padrão. O aplicativo aguarda término e limpeza; saída parcial avisa que a verificação/reparo ficou incompleto. Não desfaz alterações já realizadas.
- Restrição aos dois modos SFC, pelo canal elevado validado; não encerra outros processos ou serviços de manutenção.

![Catálogo na busca compacta](imagens/busca-previa-beta99.png)

![Prévia compacta em Claro](imagens/busca-compacta-claro-beta99.png)

![Centralização corrigida em janela reduzida](imagens/busca-centralizada-beta99.png)

![Monitor reorganizado](imagens/monitor-escuro-beta99.png)

[Comparação visual](REVISAO-VISUAL-beta9.9.md) · [Resultados e limites](../Suporte%20TI%20Grafico/Testes/RESULTADO.md) · [Homologação](../Suporte%20TI%20Grafico/Testes/HOMOLOGACAO.md).

**181 testes automatizados com CIM real, 14 simulações de intervenção e 5 testes de parada aprovados**, incluindo os testes de parada no Windows PowerShell 5.1. Compilação sem avisos. Monitor e busca inspecionados; centralização corrigida e repetida em janela de aproximadamente 800 px. A interrupção de reparo real e o fluxo completo elevado com conta distinta ainda exigem laboratório. [Prévia publicada com ZIP e SHA-256](https://github.com/gustavo-brandao/SuporteTI-Desktop/releases/tag/v1.0.0-beta.9.9). A beta.9.8 permanece disponível para retorno. Esta prévia continua sem Authenticode; laboratório de reparo real/conta distinta, Windows 10, DPI e alto contraste permanece na matriz.

Próximas etapas: **5** animações/transições e continuidade da composição dos menus; **6** homologação visual/funcional; **7** avaliação de PowerShell 7; **8** preparação da versão estável.

ZIP definitivo: **50.122.250 bytes**, SHA-256 `01b80a8901946a114c504d1d8fe3ede419c02e0f11df3f95366b997d2f322322`. Os 24 arquivos de programa são idênticos à compilação inspecionada. Manual e homologação do pacote, 51 links locais e quatro PNGs conferidos. Integridade, estrutura e consultas reais do pacote final aprovadas; leitura de discos parcial válida.

## Fechamento da publicação · 12 de novembro de 2026

[PR #7 integrado](https://github.com/gustavo-brandao/SuporteTI-Desktop/pull/7) após [CI completo aprovado](https://github.com/gustavo-brandao/SuporteTI-Desktop/actions/runs/43254621410) no commit `4054da840c696ea701d8634928594b3df601cf80`, incluindo contrato/CIM, simulações, compilação, verificação do ZIP e envio do artefato. A integração `b1c1326a08ff3b7c4abb7fa56ac26e8281b62b0f` tem a mesma árvore validada, `1d0d1a480c10bf3def059e27eb1ffe87ce9990dd`.

A release pública aponta para essa integração, permanece prévia e não substitui a versão estável. O ZIP remoto tem o mesmo tamanho e digest SHA-256 do pacote definitivo local. README e quatro capturas da main conferidos por hash Git com a revisão local. As subdivisões 4.1, 4.2 e 4.3 estão encerradas com os limites de laboratório acima; o fechamento documental não altera o executável nem exige outro ZIP.
