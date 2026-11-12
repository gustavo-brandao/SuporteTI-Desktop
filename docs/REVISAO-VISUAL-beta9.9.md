# Comparação visual · beta.9.9

Referência: [galeria aprovada](propostas-visuais/2026-11-10/conjunto-monitoramento/README.md) e [direção Linear + iStat + Raycast](PESQUISA-VISUAL-MODERNA-2026-11-11.md). O feedback foi de pouca diferença em relação à versão anterior; esta revisão muda composição, além das cores.

| Aspecto | Antes · beta.9.8 | Agora · beta.9.9 |
| --- | --- | --- |
| Busca | Campo na lateral, lista sem prévia | Paleta central, filtro por grupo, contexto e prévia; camadas na largura reduzida |
| Título | Contexto superior e título dentro da página | Um título principal, caminho pequeno acima e descrição abaixo |
| Entrada de Hardware | Seletor genérico antes do monitor | Monitor preparado diretamente; consulta pontual em CPU e processos |
| CPU/RAM | Lado a lado na largura ampla, histórico aberto | Empilhados, ícone/nome/porcentagem na mesma linha; histórico opcional |
| Barras | Traços de 8 pixels | Barras de 18 pixels efetivos, cor sem representar sensores inexistentes |

O diálogo agora é centralizado após calcular sua largura/altura e limitado à área útil da tela. A conferência final em janela de aproximadamente 800 px mostrou margens equilibradas e toda a busca visível. A preferência original Sistema foi restaurada.

As capturas documentam o executável real; a galeria conceitual continua separada. O catálogo, modos UAC e avisos não são substituídos por dados ilustrativos. O monitor mantém estatísticas da janela de 60 posições, zero real e lacunas.

Diferenças preservadas: controles de janela e seletores nativos; nenhum vidro simulado; sem sensores novos, atalhos sem implementação ou resultado fictício. Os demais menus ainda usam formulários e tabelas existentes. Sua composição específica e a linguagem de movimento devem ser avaliadas nas próximas entregas, sem declarar equivalência integral aos cinco mockups.

Verificações e limites efetivamente observados ficam em [Testes/RESULTADO](../Suporte%20TI%20Grafico/Testes/RESULTADO.md). A etapa 5 deve comparar as variantes de movimento do protótipo antes de aplicar; a seleção de uma variante para explorar não substitui o aceite da implementação em execução. Preservar a direção geral já aprovada.
