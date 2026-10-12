# Spec: Lançar pagamentos e travas

## Objetivo
Permite lançar pagamentos em um orçamento, com a soma limitada ao total do orçamento. Enquanto houver pagamento não cancelado, o orçamento fica travado para alterações de itens, de cliente, de vendedor e para o fechamento. A prévia é para controle interno da empresa e não movimenta estoque nem caixa.

## Regras
- **R1** — Um pagamento pode ser lançado a qualquer momento depois que o orçamento existe, desde que o orçamento não esteja cancelado (R14).
- **R2** — Cada pagamento tem forma de pagamento, caixa ou banco e valor.
- **R3** — Forma de pagamento inexistente retorna o erro "Forma de pagamento não encontrada.". Forma de pagamento inativa retorna o erro "Forma de pagamento inativa não pode ser usada.". Nada é gravado.
- **R4** — Caixa ou banco inexistente retorna o erro "Caixa ou banco não encontrado.". Caixa ou banco inativo retorna o erro "Caixa ou banco inativo não pode ser usado.". Nada é gravado.
- **R5** — Valor igual a zero ou negativo retorna o erro "Valor do pagamento deve ser maior que zero.". Valor com mais de 2 casas decimais retorna o erro "Valor do pagamento deve ter até 2 casas decimais.". Nada é gravado.
- **R6** — Valor maior que o saldo do orçamento retorna o erro "Valor maior que o saldo do orçamento.". Nada é gravado.
- **R7** — A soma dos pagamentos não cancelados não pode passar do total do orçamento. A soma pode chegar exatamente ao total.
- **R8** — A conferência do saldo e a gravação do pagamento acontecem em uma única operação. Se dois pagamentos chegarem ao mesmo tempo e o segundo não couber no saldo, ele é recusado com o erro da R6.
- **R9** — Saldo = total do orçamento menos a soma dos pagamentos não cancelados.
- **R10** — Enquanto houver pelo menos um pagamento não cancelado, não é possível incluir item, alterar item, cancelar item, reativar item, alterar cliente, alterar vendedor nem gravar o desconto do fechamento. Essas operações retornam o erro "Orçamento possui pagamentos não cancelados.". Nada é gravado.
- **R11** — Quando todos os pagamentos do orçamento estiverem cancelados, as travas da R10 saem e as operações voltam a funcionar.
- **R12** — Um pagamento pode ser cancelado (`cancelado = true`). O pagamento continua no banco e não é excluído.
- **R13** — Cancelar um pagamento que já está cancelado retorna o erro "Pagamento já está cancelado.". Nada é gravado.
- **R14** — Orçamento cancelado fica congelado: não aceita lançamento nem cancelamento de pagamento. Essas operações retornam o erro "Orçamento cancelado não pode ser alterado.". Nada é gravado.
- **R15** — Cancelar o orçamento é permitido mesmo com pagamentos não cancelados. Os pagamentos continuam como estão, e o orçamento cancelado mantém o saldo.
- **R16** — O caixa ou banco é só referência: lançar ou cancelar pagamento não movimenta saldo de caixa ou banco.
- **R17** — A consulta do orçamento mostra todos os pagamentos, inclusive os cancelados, cada um com a indicação `cancelado`, e o saldo conforme a R9. Pagamentos cancelados não entram no saldo.
- **R18** — Lançar pagamento sem valor retorna o erro "Valor do pagamento é obrigatório.". Nada é gravado.
- **R19** — Lançar pagamento sem forma de pagamento retorna o erro "Forma de pagamento é obrigatória.". Lançar pagamento sem caixa ou banco retorna o erro "Caixa ou banco é obrigatório.". Nada é gravado.
- **R20** — Cada pagamento tem um número de pagamento (`NumeroPagamento`) com três dígitos, no formato 001, 002, 003, dentro do próprio orçamento. O número é atribuído no lançamento, em sequência, e nunca é reutilizado. É com esse número que o pagamento é cancelado.
- **R21** — Cancelar um pagamento que não existe no orçamento retorna o erro "Pagamento não encontrado.". Nada é gravado.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Orçamento 1 com total 13.50, lançar pagamento na forma 1 (Dinheiro, ativa) com caixa 1 (ativo), valor 5.00 | Pagamento gravado; saldo 8.50 |
| Orçamento 1 com saldo 8.50, lançar pagamento de 8.50 | Pagamento aceito; saldo 0.00 |
| Orçamento 1 com saldo 8.50, lançar pagamento de 9.00 | Erro: "Valor maior que o saldo do orçamento."; nada gravado |
| Orçamento 1, lançar pagamento de valor 0 | Erro: "Valor do pagamento deve ser maior que zero."; nada gravado |
| Orçamento 1, lançar pagamento de valor -2 | Erro: "Valor do pagamento deve ser maior que zero."; nada gravado |
| Orçamento 1, lançar pagamento de valor 1.234 | Erro: "Valor do pagamento deve ter até 2 casas decimais."; nada gravado |
| Orçamento 1, lançar pagamento na forma 99 (não existe) | Erro: "Forma de pagamento não encontrada."; nada gravado |
| Orçamento 1, lançar pagamento na forma 6 (Crediário, inativa) | Erro: "Forma de pagamento inativa não pode ser usada."; nada gravado |
| Orçamento 1, lançar pagamento no caixa 99 (não existe) | Erro: "Caixa ou banco não encontrado."; nada gravado |
| Orçamento 1, lançar pagamento no caixa 3 (inativo) | Erro: "Caixa ou banco inativo não pode ser usado."; nada gravado |
| Orçamento 1 com saldo 8.50, dois pagamentos de 8.50 chegam ao mesmo tempo | Um é aceito, saldo 0.00; o outro é recusado com "Valor maior que o saldo do orçamento." |
| Orçamento 1 com pagamento de 5.00 não cancelado, incluir item | Erro: "Orçamento possui pagamentos não cancelados."; nada gravado |
| Orçamento 1 com pagamento de 5.00 não cancelado, alterar item 002 | Erro: "Orçamento possui pagamentos não cancelados."; nada gravado |
| Orçamento 1 com pagamento de 5.00 não cancelado, cancelar item 002 | Erro: "Orçamento possui pagamentos não cancelados."; nada gravado |
| Orçamento 1 com pagamento de 5.00 não cancelado, reativar item 002 | Erro: "Orçamento possui pagamentos não cancelados."; nada gravado |
| Orçamento 1 com pagamento de 5.00 não cancelado, alterar cliente | Erro: "Orçamento possui pagamentos não cancelados."; cliente continua o mesmo |
| Orçamento 1 com pagamento de 5.00 não cancelado, alterar vendedor | Erro: "Orçamento possui pagamentos não cancelados."; vendedor continua o mesmo |
| Orçamento 1 com pagamento de 5.00 não cancelado, fechar com desconto 1.00 | Erro: "Orçamento possui pagamentos não cancelados."; desconto não gravado |
| Orçamento 1 com pagamento 001 de 5.00 cancelado, incluir item | Aceito; as travas saíram |
| Orçamento 1 com pagamento 001 de 5.00, cancelar o pagamento 001 | Pagamento gravado com `cancelado = true`; saldo volta ao total 13.50 |
| Orçamento 1, cancelar o pagamento 001 que já está cancelado | Erro: "Pagamento já está cancelado."; nada gravado |
| Orçamento 1 com pagamento de 5.00 não cancelado, cancelar o orçamento | Orçamento gravado com `cancelado = true`; o pagamento continua não cancelado; saldo 8.50 |
| Orçamento 1 cancelado, lançar pagamento de 1.00 | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 cancelado com pagamento 001 não cancelado, cancelar o pagamento 001 | Erro: "Orçamento cancelado não pode ser alterado."; pagamento continua não cancelado |
| Lançar pagamento de 5.00 no caixa 1 | Saldo do caixa 1 não muda |
| Consultar orçamento 1 com pagamento 001 de 5.00 cancelado e pagamento 002 de 3.00 não cancelado, total 13.50 | Os dois pagamentos aparecem, o 001 com `cancelado = true`; saldo 10.50 |
| Orçamento 1 com pagamento 001 lançado, lançar outro pagamento | O novo pagamento recebe o número 002 |
| Orçamento 1, lançar pagamento sem valor | Erro: "Valor do pagamento é obrigatório."; nada gravado |
| Orçamento 1, lançar pagamento sem forma de pagamento | Erro: "Forma de pagamento é obrigatória."; nada gravado |
| Orçamento 1, lançar pagamento sem caixa ou banco | Erro: "Caixa ou banco é obrigatório."; nada gravado |
| Orçamento 1, cancelar o pagamento 099 (não existe) | Erro: "Pagamento não encontrado."; nada gravado |

## Fora do escopo
- Transformar orçamento em venda.
- Movimentação de saldo do caixa ou banco.
- Baixa de estoque.
- Excluir pagamentos.

## Decisões
- Cliente e vendedor também ficam travados enquanto houver pagamento não cancelado (resposta do usuário, pergunta 1). Motivo: as condições de pagamento dependem da situação do cliente com a empresa.
- Cancelar o orçamento com pagamentos é permitido (resposta do usuário, pergunta 2). Os pagamentos continuam como estão, e o orçamento cancelado mantém o saldo (alternativa escolhida).
- Cancelar pagamento de orçamento cancelado não é permitido (resposta do usuário). Orçamento cancelado fica congelado, sem exceção.
- A conferência do saldo e a gravação do pagamento acontecem juntas, para que pagamentos simultâneos não passem do total (resposta do usuário, pergunta 3, sugestão aceita).
- Mensagens de valor, de forma e de caixa ou banco seguem as sugestões aceitas (resposta do usuário, perguntas 4 e 5).
- Mensagens novas, propostas e para revisão: "Orçamento possui pagamentos não cancelados." (R10) e "Pagamento já está cancelado." (R13).
- Lacunas decididas pelo agente, a pedido do usuário ("seguir da forma melhor"), e para revisão: valor, forma e caixa ausentes (R18 e R19), numeração de pagamentos no mesmo padrão dos itens (R20) e pagamento inexistente (R21).
