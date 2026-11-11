# Suporte TI Gráfico 1.0.0-beta.9.6

Conclui a implementação da etapa **2.2 — barra de título e moldura** e inclui a estrutura da etapa 2.1.

- Título integrado à paleta e borda discreta no Windows 11; texto e borda acompanham ativação e desativação.
- Cores reaplicadas em recriação da janela, tema e DPI; alto contraste devolve o acabamento ao sistema.
- Arraste, tamanho, Snap, menu e controles continuam nativos. Windows 10 conserva o acabamento disponível no sistema.
- As 44 operações continuam disponíveis; monitoramento iStat, busca Raycast e novas transições serão entregas próprias.

![Título e moldura no tema Escuro](imagens/moldura-escuro-beta96.png)

![Título e moldura no tema Claro](imagens/moldura-claro-beta96.png)

![Janela compacta com moldura nativa](imagens/moldura-compacta-beta96.png)

Capturas reais, com recorte somente do rodapé que identifica a estação. Validação local: 159 testes com CIM e 13 simulações aprovados; maximizar/restaurar, duplo clique, arraste, menu, redução de largura, temas e reabertura conferidos. Snap, mudanças reais de DPI, alto contraste e Windows 10 seguem na matriz de laboratório. [Resultados e limites](../Suporte%20TI%20Grafico/Testes/RESULTADO.md).

A beta.9.5 ficou como rascunho da etapa 2.1; esta prévia reúne as duas subdivisões. A beta.9.4 pública permanece disponível para retorno. Extraia o ZIP completo em uma pasta nova e confira o SHA-256. A prévia continua sem Authenticode.

Próximas etapas: **3** monitoramento iStat; **4** busca Raycast; **5** animações e transições; **6** homologação visual e funcional; **7** avaliação do PowerShell 7; **8** preparação da versão estável. [Planejamento](PLANEJAMENTO.md).

## Fechamento da publicação · 11 de novembro de 2026

[Release pública com ZIP e SHA-256](https://github.com/gustavo-brandao/SuporteTI-Desktop/releases/tag/v1.0.0-beta.9.6), após integrar o [PR #5](https://github.com/gustavo-brandao/SuporteTI-Desktop/pull/5). O [CI completo da revisão](https://github.com/gustavo-brandao/SuporteTI-Desktop/actions/runs/43113372987) aprovou testes, simulações, publicação, verificação do pacote e envio do artefato no commit `a1902bf627b44c7c81394d95ebdb8b6ee5103426`. A integração `cfca990d79c8e92e234bb6295b0e01f2ec01be79` tem a mesma árvore de arquivos validada: `0c38d8932259d2875bedd6e7f6de889bd9545bf1`.

README e três capturas no GitHub conferidos por hash Git com os arquivos locais. O ZIP publicado tem **50.102.667 bytes** e digest remoto SHA-256 `96b7327b969f739582a21981c0d39794b327ffefb70cfbe4e81ea3c48cc226c3`, igual ao pacote local inspecionado. A verificação local do ZIP final aprovou integridade, estrutura de três itens, consulta de sistema concluída e leitura parcial válida de discos. O manual da beta.9.6, os links locais e os 24 arquivos de programa foram conferidos.

As duas subdivisões da etapa 2 estão entregues. A matriz de laboratório continua na etapa 6, conforme os limites descritos acima; a aprovação do CI não equivale ao aceite de Snap, DPI, alto contraste ou Windows 10 em todos os ambientes.
