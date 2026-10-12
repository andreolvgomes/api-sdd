# Spec: Fechar orçamento com desconto no total

## Objetivo
Permite gravar um desconto no total de um orçamento, depois da conclusão dos itens e antes dos pagamentos. A prévia é para controle interno da empresa e não movimenta estoque nem caixa.

## Regras
- **R1** — Fechar grava o desconto no total do orçamento. O desconto é um valor em reais, informado diretamente, e não em percentual.
- **R2** — O desconto é aplicado ao total do orçamento, não a cada item.
- **R3** — Total do orçamento = subtotal menos o desconto, com 2 casas decimais. Sem desconto gravado, o desconto é 0,00 e o total é igual ao subtotal.
- **R4** — Gravar outro desconto substitui o anterior, sem histórico. Desconto igual a zero remove o desconto.
- **R5** — Desconto negativo retorna o erro "Desconto não pode ser negativo.". Nada é gravado.
- **R6** — Desconto maior que o subtotal retorna o erro "Desconto não pode ser maior que o subtotal.". Nada é gravado.
- **R7** — Desconto com mais de 2 casas decimais retorna o erro "Desconto deve ter até 2 casas decimais.". Nada é gravado.
- **R8** — Fechar um orçamento sem itens ativos retorna o erro "Orçamento sem itens ativos não pode ser fechado.". Nada é gravado. Itens ativos são os que não estão cancelados.
- **R9** — Fechar um orçamento cancelado retorna o erro "Orçamento cancelado não pode ser alterado.". Nada é gravado.
- **R10** — Incluir, alterar, cancelar ou reativar item refaz os totais do orçamento e zera o desconto gravado: o desconto volta a 0,00 e o total volta a ser igual ao subtotal. Para aplicar um desconto de novo, é preciso fechar o orçamento outra vez.
- **R11** — O fechamento não trava a alteração de itens. Qualquer alteração de item segue a regra R10.
- **R12** — A consulta do orçamento mostra o desconto e o total, conforme a regra R3.
- **R13** — Fechar sem informar o valor do desconto retorna o erro "Desconto é obrigatório.". Nada é gravado. Para remover o desconto, é preciso informar zero.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Orçamento 1 com subtotal 13.50, fechar com desconto 1.50 | Desconto 1.50; total 12.00 |
| Orçamento 1 com desconto 1.50, fechar com desconto 2.00 | Desconto 2.00; total 11.50; o desconto anterior é substituído |
| Orçamento 1 com desconto 1.50, fechar com desconto 0 | Desconto 0,00; total 13.50 |
| Orçamento 1 com subtotal 13.50, fechar com desconto -1 | Erro: "Desconto não pode ser negativo."; nada gravado |
| Orçamento 1 com subtotal 13.50, fechar com desconto 14.00 | Erro: "Desconto não pode ser maior que o subtotal."; nada gravado |
| Orçamento 1 com subtotal 13.50, fechar com desconto 1.234 | Erro: "Desconto deve ter até 2 casas decimais."; nada gravado |
| Orçamento 1 com todos os itens cancelados, fechar com desconto 1.00 | Erro: "Orçamento sem itens ativos não pode ser fechado."; nada gravado |
| Orçamento 1 cancelado, fechar com desconto 1.00 | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 fechado com desconto 1.50 (total 12.00), incluir item `P002` quantidade 1 (total 1.00) | Desconto volta a 0,00; subtotal 14.50; total 14.50 |
| Orçamento 1 fechado com desconto 1.50, alterar quantidade do item 002 (`P002`, total 3.00) para 5 | Desconto volta a 0,00; total igual ao subtotal recalculado (15.50) |
| Orçamento 1 fechado com desconto 1.50 (subtotal 13.50), cancelar item 002 (total 3.00) | Desconto volta a 0,00; subtotal 10.50; total 10.50 |
| Orçamento 1 fechado com desconto 1.50, reativar item 002 (cancelado antes) | Desconto volta a 0,00; total igual ao subtotal |
| Consultar orçamento 1 sem desconto gravado, com subtotal 13.50 | Desconto 0,00; total 13.50 |
| Orçamento 1 com subtotal 13.50, fechar sem informar desconto | Erro: "Desconto é obrigatório."; nada gravado |

## Fora do escopo
- Pagamentos e travas de pagamento.
- Desconto por item.
- Desconto em percentual.
- Baixa de estoque e movimentação de caixa ou banco.
- Situação própria do orçamento para "fechado". O fechamento é só o desconto gravado.

## Decisões
- Desconto informado em valor, e não em percentual (resposta do usuário, pergunta 1, sugestão aceita).
- Outro desconto substitui o anterior, sem histórico, e desconto zero remove o desconto (sugestão aceita, pergunta 1).
- Alterações de itens refazem os totais e zeram o desconto gravado, o que chamamos de reset (resposta do usuário, pergunta 1). A R10 inclui a reativação de item, por ser uma alteração de item que também muda os totais (resposta do usuário, pergunta 1).
- Fechar um orçamento sem itens ativos é recusado (sugestão aceita, pergunta 3 da entrevista inicial).
- Desconto negativo e desconto maior que o subtotal são recusados (sugestão aceita).
- Desconto com mais de 2 casas decimais é recusado (resposta do usuário, pergunta 3).
- Orçamento cancelado não pode ser fechado (sugestão aceita).
- A consulta mostra desconto 0,00 e total igual ao subtotal quando não há desconto gravado (resposta do usuário, pergunta 2).
- Mensagens novas desta spec, confirmadas pelo usuário (pergunta 2): "Desconto não pode ser negativo.", "Desconto não pode ser maior que o subtotal.", "Desconto deve ter até 2 casas decimais.", "Orçamento sem itens ativos não pode ser fechado.".
- Fechar sem informar o desconto retorna "Desconto é obrigatório." e não grava nada (resposta do usuário, pergunta 1, sugestão aceita). Foi registrado como R13.
