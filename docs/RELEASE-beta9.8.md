# Suporte TI Gráfico 1.0.0-beta.9.8

Prévia integrada das etapas **3.1 — CPU/RAM** e **3.2 — velocímetro e estados**. Publicar juntas após revisão visual e funcional das duas partes.

- Cartões adaptáveis de CPU/RAM, porcentagens em destaque, memória usada/disponível/total e histórico aberto por padrão.
- Média/pico das leituras válidas da janela de até 60 posições; zero real conta e lacunas não são inventadas como zero. Pausa preserva; nova sessão limpa.
- Taxa provisória separada dos resultados finais de download/upload e latência HTTPS. Resumos ao lado do arco ou abaixo dele em largura reduzida.
- Cancelamento remove a agulha e as leituras provisórias; resultado parcial conserva a direção válida. Progresso tardio é ignorado; nova execução limpa os resultados e a escala anteriores.
- Motor de medição, limites, servidor e relatórios preservados. Novas animações ficam na etapa 5.

**173 testes com CIM real e 13 simulações aprovados**, compilação sem avisos. Coleta real de CPU/RAM, pausa/reinício, histórico e medição real completa de conexão conferidos na execução integrada. A revisão encontrou e corrigiu falta de altura no monitor recolhido; repetir esse aceite, janela estreita, tema Claro e cancelamento real no pacote final antes de publicar. A automação voltou a perder acesso à sessão. [Resultados e limites](../Suporte%20TI%20Grafico/Testes/RESULTADO.md).

As imagens serão renovadas com a revisão final. A beta.9.6 permanece pública; a beta.9.7 não teve release pública independente. Sem Authenticode; matriz de DPI/alto contraste, Windows 10 e laboratório administrativo permanece em homologação.

Depois do fechamento conjunto: **4** busca Raycast; **5** animações e transições; **6** homologação visual e funcional; **7** PowerShell 7; **8** versão estável. [Planejamento](PLANEJAMENTO.md).
