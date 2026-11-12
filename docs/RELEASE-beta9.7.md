# Suporte TI Gráfico 1.0.0-beta.9.7

**Fechamento:** esta revisão intermediária não teve release pública. A 3.1 foi revisada junto da 3.2 e entregue na [beta.9.8](RELEASE-beta9.8.md), com CI aprovado e ZIP público. O registro abaixo preserva o estado observado durante o desenvolvimento.

Prévia da **etapa 3.1 — monitor CPU/RAM**. Aceite visual e publicação pendentes.

- Porcentagens em destaque, detalhes de memória separados e cartões adaptáveis lado a lado ou empilhados.
- Histórico de até 60 amostras aberto por padrão e barras de uso mais discretas.
- Média, pico e contagem das leituras válidas dessa janela. Zero real conta; lacunas não são tratadas como zero.
- Pausar conserva dados; iniciar abre uma nova sessão. Recolher curvas mantém a coleta e os resumos.

Validação: 163 testes com CIM real e 13 simulações aprovados; compilação sem avisos. A inspeção da nova composição foi impedida por acesso negado no controlador da área de trabalho. As capturas serão renovadas após o aceite. [Resultados](../Suporte%20TI%20Grafico/Testes/RESULTADO.md).

As animações atuais permanecem; uma nova linguagem de movimento pertence à etapa 5. Não foram acrescentados sensores ou dados fictícios. A etapa **3.2** refinará o velocímetro e seus estados. Depois seguem **4** busca Raycast; **5** animações; **6** homologação; **7** PowerShell 7; **8** versão estável. [Planejamento](PLANEJAMENTO.md).

## Revisão preparada · 11 de novembro de 2026

[PR #6 em rascunho](https://github.com/gustavo-brandao/SuporteTI-Desktop/pull/6). O [CI completo](https://github.com/gustavo-brandao/SuporteTI-Desktop/actions/runs/43119441634) aprovou a revisão `7f8cf9878ad0d260a3cb7d270f37cc49d300ac1e`. O pacote local passou em integridade, estrutura e consultas reais: sistema concluído e discos com leitura parcial válida. Os 24 arquivos de programa conferem com o pacote de validação aberto; essa comparação não substitui inspeção da nova tela.

ZIP da prévia: **50.104.204 bytes**, SHA-256 `2a7d460d10c0cdf1866cb93f06ab2535ef09d260722dc943bc9816a0f778fee7`. Manual atualizado e 44 links locais conferidos.

Após o usuário informar sessão desbloqueada, a retomada conseguiu capturar a janela, mas o clique em Hardware voltou a falhar na ativação. A recuperação repetiu `GetCursorPos failed: Acesso negado (0x80070005)`. Não houve aceite visual do monitor, renovação das imagens, integração na main ou release pública desta prévia. A beta.9.6 permanece como versão pública. Retomar os cenários da matriz quando o controlador voltar a operar a sessão.
