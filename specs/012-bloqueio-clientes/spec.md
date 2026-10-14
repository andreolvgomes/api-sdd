# Spec: Bloqueio de clientes nos pagamentos de orçamentos

## Objetivo
Permitir marcar um cliente como bloqueado para limitar as formas de pagamento dele em orçamentos. Clientes bloqueados só pagam à vista, em dinheiro ou Pix.

## Regras
- **R1** — Todo cliente tem um nível de bloqueio, que pode ser "liberado" ou "bloqueado". Um cliente novo começa como "liberado".
- **R2** — Um usuário com acesso ao cadastro pode alterar o nível de bloqueio de um cliente, sem informar motivo.
- **R3** — Alterar o nível de bloqueio de um cliente que não existe é recusado e não grava nada.
- **R4** — Um cliente liberado pode lançar pagamento com qualquer forma de pagamento cadastrada.
- **R5** — Um cliente bloqueado só pode lançar pagamento com forma de pagamento do tipo Dinheiro (1) ou Pix (4).
- **R6** — Um cliente bloqueado que tente lançar pagamento com forma de pagamento do tipo Cartão de crédito (2), Cartão de débito (3), Boleto (5) ou Crediário (6) é recusado, com a mensagem "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)", e o pagamento não é gravado.
- **R7** — Ao recusar um pagamento por bloqueio, a resposta é de validação (400).
- **R8** — Bloquear um cliente não altera pagamentos já lançados em orçamentos, inclusive os feitos com cartão, boleto ou crediário. Eles continuam gravados e válidos.
- **R9** — A regra R5 vale para o lançamento de pagamento. Alterar itens, fechar orçamento ou cancelar pagamentos não é afetado pelo bloqueio.
- **R10** — Ao desbloquear um cliente, os pagamentos antes recusados podem ser lançados de novo, com qualquer forma de pagamento cadastrada.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cliente liberado (id 1) lança pagamento de R$ 50,00 com Cartão de crédito | Pagamento gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Dinheiro | Pagamento gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Pix | Pagamento gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Cartão de crédito | 400: "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)"; pagamento não gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Cartão de débito | 400: "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)"; pagamento não gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Boleto | 400: "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)"; pagamento não gravado |
| Cliente bloqueado (id 1) lança pagamento de R$ 50,00 com Crediário | 400: "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)"; pagamento não gravado |
| Cliente (id 1) tem pagamento de R$ 50,00 com Cartão de crédito lançado; é bloqueado depois | Pagamento continua gravado e válido |
| Alterar o nível de bloqueio do cliente id 99, que não existe | 404: "Cliente não encontrado" |
| Alterar o nível de bloqueio do cliente id 1 para "pausado" | 400: "Nível de bloqueio deve ser liberado ou bloqueado" |
| Cliente bloqueado (id 1) tem orçamento aberto e altera o desconto | Desconto alterado normalmente |
| Cliente bloqueado (id 1) é desbloqueado e lança pagamento de R$ 50,00 com Cartão de crédito | Pagamento gravado |

## Fora do escopo
- Níveis de bloqueio além de "liberado" e "bloqueado".
- Motivo, data ou histórico do bloqueio.
- Remover pagamentos já lançados quando o cliente é bloqueado.
- Restringir formas de pagamento em vendas ou em outras telas que não sejam o lançamento de pagamento de orçamento.
- Listagem ou filtro de clientes por nível de bloqueio.

## Decisões
- Níveis de bloqueio: dois, "liberado" e "bloqueado" (sugestão aceita).
- Quem altera o bloqueio: um usuário com acesso ao cadastro do cliente, por alteração simples, sem motivo obrigatório (sugestão aceita).
- Pagamentos já lançados permanecem quando o cliente é bloqueado (resposta do usuário).
- Classificação das formas pelo campo Tipo: permitidas para cliente bloqueado são Dinheiro (1) e Pix (4); bloqueadas são Cartão de crédito (2), Cartão de débito (3), Boleto (5) e Crediário (6) (sugestão aceita).
- Recusa por bloqueio: validação, mensagem "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)", sem gravar o pagamento (sugestão aceita).
- Em aberto: nenhum.
