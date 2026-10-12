# Spec: Alterar e cancelar itens e orçamento

## Objetivo
Permite ajustar uma prévia de orçamento para um cliente a qualquer momento: alterar itens, cancelar e reativar itens, e cancelar o orçamento. Nada é excluído do banco. A prévia é para controle interno da empresa e não movimenta estoque nem caixa.

## Regras
- **R1** — Alterar um item grava a nova quantidade e/ou o novo produto. Pelo menos um dos dois deve ser informado na alteração. Quantidade e produto podem ser alterados juntos ou separadamente.
- **R2** — Quantidade informada na alteração deve ser maior que zero e pode ter até 2 casas decimais. Zero ou negativa, e com mais de 2 casas decimais, retornam as mesmas mensagens da spec 008 (R9). Nada é gravado.
- **R3** — Produto alterado precisa existir e estar ativo. Produto inexistente retorna "Produto não encontrado.". Produto inativo retorna "Produto inativo não pode ser usado.". Produto sem estoque pode. Nada é gravado.
- **R4** — Ao alterar um item, o total do item é recalculado (quantidade × preço, com 2 casas decimais) e o subtotal do orçamento é recalculado.
- **R5** — O preço do item só muda quando o produto do item é trocado: passa a ser o preço de venda atual do novo produto. Alterar apenas a quantidade mantém o preço gravado. Alterar o produto para o mesmo produto que já está no item não muda o preço.
- **R6** — Cancelar um item grava `cancelado = true`. O item continua no banco e não é excluído.
- **R7** — Item cancelado não entra no subtotal do orçamento. O subtotal soma apenas os itens não cancelados.
- **R8** — Um item cancelado pode voltar a ficar ativo, por uma operação específica de reativação, separada da alteração do item. Ao voltar, ele entra novamente no subtotal, com o mesmo preço e a mesma quantidade que tinha. A reativação grava `cancelado = false`. Se o produto do item estiver inativo, a reativação não é permitida: retorna o erro "Produto inativo não pode ser usado.". Nada é gravado.
- **R9** — Cancelar um item que já está cancelado retorna o erro "Item já está cancelado.". Reativar um item que não está cancelado retorna o erro "Item não está cancelado.". Nada é gravado.
- **R10** — Alterar um item cancelado retorna o erro "Item cancelado não pode ser alterado.". Para mudar um item cancelado, ele precisa ser reativado antes.
- **R11** — Cada item tem um número de item (`NumeroItem`) com três dígitos, no formato 001, 002, 003, dentro do próprio orçamento. É com esse número que o item é identificado para alterar, cancelar e reativar.
- **R12** — O número de item é atribuído na inclusão, em sequência dentro do orçamento, e nunca é reutilizado, mesmo depois de um cancelamento.
- **R13** — Número de item inexistente no orçamento retorna o erro "Item não encontrado.". Nada é gravado.
- **R14** — Alterar o cliente ou o vendedor do orçamento exige que o novo cadastro esteja ativo. Cliente inativo retorna "Cliente inativo não pode ser usado."; vendedor inativo retorna "Vendedor inativo não pode ser usado.". Cliente ou vendedor inexistente retornam "Cliente não encontrado." ou "Vendedor não encontrado.". Nada é gravado.
- **R15** — Cancelar o orçamento grava `cancelado = true`. O orçamento continua no banco e não é excluído. Cancelar os itens não é feito automaticamente: os itens mantêm o seu estado.
- **R16** — Cancelar um orçamento que já está cancelado retorna o erro "Orçamento já está cancelado.". Nada é gravado.
- **R17** — Orçamento cancelado está congelado: não aceita nenhum tipo de modificação. Isso inclui incluir item, alterar item, cancelar item, reativar item, alterar cliente, alterar vendedor e cancelar de novo. Qualquer uma dessas operações retorna o erro "Orçamento cancelado não pode ser alterado.". Nada é gravado.
- **R18** — Não há trava por pagamento nesta spec. Fora de orçamento cancelado, a edição é liberada a qualquer momento.
- **R19** — Alterar, cancelar e reativar itens e orçamento não alteram o estoque de produto nem movimentam saldo de caixa ou banco.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Orçamento 1, item 002 (`P002`, preço 1.00, quantidade 3, total 3.00), alterar quantidade para 5 | Quantidade 5; preço continua 1.00; total 5.00; subtotal recalculado |
| Orçamento 1, item 001 (`P001`, preço 3.50, quantidade 2), trocar produto para `P005` (preço de venda atual 4.00) | Produto `P005`; preço 4.00; total 8.00; subtotal recalculado |
| Orçamento 1, item 001, trocar produto para `P001` (o mesmo) | Preço continua 3.50; total recalculado |
| Orçamento 1, item 002, quantidade 0 | Erro: "Quantidade deve ser maior que zero."; nada gravado |
| Orçamento 1, item 002, quantidade 1.234 | Erro: "Quantidade deve ter até 2 casas decimais."; nada gravado |
| Orçamento 1, item 002, trocar produto para `P009` (não existe) | Erro: "Produto não encontrado."; nada gravado |
| Orçamento 1, item 002, trocar produto para `P003` (inativo) | Erro: "Produto inativo não pode ser usado."; nada gravado |
| Orçamento 1, item 004, trocar produto para `P004` (estoque 0.00) | Aceito; estoque de `P004` continua 0.00 |
| Orçamento 1, item 002 (subtotal 13.50), cancelar item 002 (total 3.00) | Item gravado com `cancelado = true`; subtotal 10.50 |
| Orçamento 1, item 002 já cancelado, cancelar de novo | Erro: "Item já está cancelado."; nada gravado |
| Orçamento 1, item 002 cancelado, reativar item 002 | Item volta ativo; total 3.00 entra de novo; subtotal 13.50 |
| Orçamento 1, item 001 não cancelado, reativar item 001 | Erro: "Item não está cancelado."; nada gravado |
| Orçamento 1, item 002 cancelado, reativar item 002 com produto `P003` inativo | Erro: "Produto inativo não pode ser usado."; item continua cancelado; nada gravado |
| Orçamento 1, item 002 cancelado, alterar quantidade para 4 | Erro: "Item cancelado não pode ser alterado."; nada gravado |
| Orçamento 1, item 005 (não existe no orçamento) | Erro: "Item não encontrado."; nada gravado |
| Orçamento 1, trocar cliente para 2 (inativo) | Erro: "Cliente inativo não pode ser usado."; cliente continua o mesmo |
| Orçamento 1, trocar vendedor para 99 (não existe) | Erro: "Vendedor não encontrado."; vendedor continua o mesmo |
| Orçamento 1, cancelar orçamento | Orçamento gravado com `cancelado = true`; itens continuam como estavam |
| Orçamento 1 cancelado, cancelar de novo | Erro: "Orçamento já está cancelado."; nada gravado |
| Orçamento 1 cancelado, incluir item | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 cancelado, alterar item 001 | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 cancelado, reativar item 002 | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 cancelado, trocar vendedor | Erro: "Orçamento cancelado não pode ser alterado."; nada gravado |
| Orçamento 1 com item 003 cancelado e item 004 incluído depois | O item 004 recebe o número 004, mesmo com 003 cancelado; o número 003 não é reutilizado |

## Fora do escopo
- Fechamento e desconto.
- Pagamentos e travas de pagamento.
- Excluir itens ou orçamentos.
- Baixa de estoque e movimentação de caixa ou banco.
- Transformar orçamento em venda.

## Decisões
- Itens cancelados saem do subtotal, e o subtotal soma apenas os itens não cancelados (resposta do usuário, pergunta 1, sugestão aceita). Esta spec altera a regra "subtotal soma todos os itens" da spec 008, que dizia que o cancelamento seria tratado aqui.
- Preço do item muda só quando o produto é trocado, pelo preço de venda atual do novo produto (resposta do usuário, pergunta 2, sugestão aceita). Isso mantém a R10 da spec 008 para alterações de quantidade.
- Item cancelado pode voltar a ficar ativo e volta a entrar no total (resposta do usuário, pergunta 3). O preço e a quantidade que o item tinha antes do cancelamento são mantidos.
- Orçamento cancelado é congelado: nenhuma modificação é aceita (resposta do usuário, pergunta 4). Inclui reativar item e cancelar de novo.
- Cancelar o orçamento não cancela os itens automaticamente (proposta, sugestão aceita).
- Número do item com três dígitos (001, 002, 003), sequencial dentro do orçamento e nunca reutilizado (resposta do usuário, pergunta 5). Esse é o identificador do item nesta operação, e resolve o ponto em aberto da spec 008.
- Mensagens de erro novas desta spec, propostas e para revisão: "Item já está cancelado.", "Item não está cancelado.", "Item cancelado não pode ser alterado.", "Item não encontrado.", "Orçamento já está cancelado.", "Orçamento cancelado não pode ser alterado.", "Produto inativo não pode ser usado.". Mensagens de cliente e vendedor reutilizam as da spec 008.
- A reativação de item cancelado é feita por um comando específico, separado da alteração do item (resposta do usuário).
- Reativação não é permitida se o produto do item estiver inativo (resposta do usuário).
- Em aberto: como preencher o número de item dos itens já incluídos antes desta spec (spec 008), que não têm número.
