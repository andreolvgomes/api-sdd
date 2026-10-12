# Spec: Incluir itens no orçamento

## Objetivo
Permite montar uma prévia de orçamento para um cliente: o primeiro item cria o orçamento, e os demais itens são incluídos um a um. Também permite consultar um orçamento com seus itens e listar os orçamentos. A prévia é para controle interno da empresa e não movimenta estoque nem caixa.

## Regras
- **R1** — Ao incluir o primeiro item, o sistema cria o orçamento com número e data, e depois grava o item. Se gravar o item falhar, o orçamento continua existindo, sem itens (ver R16). O número é sequencial, começando em 1, e nunca se repete, mesmo quando dois orçamentos são criados ao mesmo tempo. A data é a da criação.
- **R2** — Cliente e vendedor são obrigatórios na criação do orçamento. Cliente ausente retorna o erro "Cliente é obrigatório."; vendedor ausente retorna "Vendedor é obrigatório.". Nada é gravado.
- **R3** — Cliente informado que não existe retorna o erro "Cliente não encontrado.". Nada é gravado.
- **R4** — Vendedor informado que não existe retorna o erro "Vendedor não encontrado.". Nada é gravado.
- **R5** — Cliente inativo retorna o erro "Cliente inativo não pode ser usado." e vendedor inativo retorna "Vendedor inativo não pode ser usado.". Nada é gravado.
- **R6** — Produto é obrigatório em cada item: ausente retorna "Produto é obrigatório.". Produto que não existe retorna "Produto não encontrado.". Nada é gravado.
- **R7** — Produto inativo não pode entrar como item e retorna o erro "Produto inativo não pode ser incluído.". Nada é gravado.
- **R8** — Produto sem estoque pode entrar como item. O estoque do produto não muda.
- **R9** — Quantidade é obrigatória, deve ser maior que zero e pode ter fração de até 2 casas decimais. Ausente retorna "Quantidade é obrigatória."; zero ou negativa retorna "Quantidade deve ser maior que zero."; com mais de 2 casas decimais retorna "Quantidade deve ter até 2 casas decimais.". Nada é gravado.
- **R10** — O preço do item é o preço de venda do produto no momento da inclusão. Depois de gravado, esse preço não muda.
- **R11** — Total do item = quantidade × preço, com 2 casas decimais.
- **R12** — Subtotal do orçamento = soma dos totais de todos os itens, com 2 casas decimais.
- **R13** — Total do item e subtotal são calculados pelo sistema. Se a requisição trouxer esses valores, eles são ignorados.
- **R14** — O mesmo produto pode ser incluído várias vezes. Cada inclusão cria um item independente, com seu próprio total.
- **R15** — Cada item é salvo imediatamente ao ser incluído, sem esperar outros itens. Se um item falhar, os itens já salvos continuam gravados.
- **R16** — Se gravar um item falhar, o orçamento já existente continua existindo, com os itens já gravados (inclusive nenhum). Orçamento sem itens é aceito. Cada falha fica registrada em log, para investigação.
- **R17** — Consultar um orçamento pelo número retorna o cabeçalho (número, data, cliente com número e nome, vendedor com número e nome), os itens com seu total e o subtotal. Número inexistente retorna o erro "Orçamento não encontrado.".
- **R18** — Listar orçamentos retorna, para cada um, número, data, cliente (número e nome) e vendedor (número e nome), ordenados pelo número. Os filtros por cliente e por vendedor são opcionais e podem ser usados juntos. Sem filtro, lista todos.
- **R19** — Incluir item não altera o estoque do produto nem movimenta saldo de caixa ou banco.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Primeiro item: cliente 1, vendedor 1, produto `P001` (preço 3.50), quantidade 2 | Orçamento nº 1 criado com a data de hoje; item `P001`, quantidade 2, preço 3.50, total 7.00; subtotal 7.00 |
| Segundo item no orçamento 1: produto `P002` (preço 1.00), quantidade 3 | Item com total 3.00; subtotal 10.00 |
| Terceiro item no orçamento 1: `P001` de novo, quantidade 1 | Item separado, total 3.50; subtotal 13.50 |
| Item `P002`, quantidade 0.5 | Aceito; total 0.50 |
| Item `P002`, quantidade 0 | Erro: "Quantidade deve ser maior que zero." |
| Item `P002`, quantidade 1.234 | Erro: "Quantidade deve ter até 2 casas decimais." |
| Item `P002`, sem quantidade | Erro: "Quantidade é obrigatória." |
| Primeiro item com cliente 99 (não existe) | Erro: "Cliente não encontrado."; nenhum orçamento criado |
| Primeiro item sem cliente | Erro: "Cliente é obrigatório."; nenhum orçamento criado |
| Primeiro item com vendedor 99 (não existe) | Erro: "Vendedor não encontrado."; nenhum orçamento criado |
| Primeiro item com cliente 2, inativo | Erro: "Cliente inativo não pode ser usado."; nenhum orçamento criado |
| Primeiro item válido, mas a gravação do item falha | Orçamento nº 1 continua existindo, sem itens; a falha fica registrada em log |
| Item com `P002`, quantidade 1, enviando também `total: 999.00` na requisição | Aceito; total do item 1.00, o valor enviado é ignorado |
| Item com produto `P009` (não existe) | Erro: "Produto não encontrado."; nada gravado |
| Item com produto `P003`, inativo | Erro: "Produto inativo não pode ser incluído."; nada gravado |
| Item com produto `P004`, estoque 0.00 | Aceito; estoque de `P004` continua 0.00 |
| Consultar orçamento 1 | Cabeçalho, os três itens com seus totais e subtotal 13.50 |
| Consultar orçamento 50 (não existe) | Erro: "Orçamento não encontrado." |
| Listar orçamentos, sem filtro | Lista todos, ordenados pelo número |
| Listar orçamentos, filtro por cliente 1 | Apenas os orçamentos do cliente 1 |
| Listar orçamentos, filtro por cliente 1 e vendedor 1 | Apenas os orçamentos com cliente 1 e vendedor 1 |
| Listar orçamentos, filtro por cliente 99 (não existe) | Lista vazia, sem erro |

## Fora do escopo
- Alterar ou cancelar itens e orçamentos.
- Desconto, fechamento e pagamentos.
- Baixa de estoque, por orçamento ou por qualquer outra operação.
- Movimentação de saldo de caixa ou banco.
- Transformar orçamento em venda.
- Excluir itens ou orçamentos.

## Decisões
- Campos do cabeçalho: cliente e vendedor obrigatórios; número sequencial; data de criação (resposta do usuário, visão de orçamentos).
- Preço copiado do produto na inclusão e fixo depois (resposta do usuário, visão de orçamentos).
- Total do item e subtotal com 2 casas decimais (sugestão aceita, pergunta 1).
- Quantidade aceita fração de até 2 casas decimais (sugestão aceita, pergunta 2).
- Listagem com número, data, cliente e vendedor, com filtros opcionais por cliente e por vendedor (sugestão aceita, pergunta 3). A ordenação pelo número e o filtro de cliente inexistente retornando lista vazia foram propostos nesta spec, sem discussão específica.
- Número único mesmo com criações simultâneas (sugestão aceita, pergunta 4).
- Mensagens de erro para cliente, vendedor e produto não encontrados (sugestão aceita, pergunta 5). As demais mensagens (obrigatórios, inativos, quantidade, orçamento não encontrado) não foram discutidas na entrevista e foram propostas nesta spec, para revisão.
- Número do orçamento começa em 1, como nos cadastros anteriores (proposta).
- Falha ao gravar o primeiro item: o orçamento continua existindo, sem itens, e a falha fica registrada em log. O registro serve de histórico para investigação (resposta do usuário, ajuste na implementação).
- Total e subtotal enviados na requisição são ignorados, e o sistema recalcula os valores (resposta do usuário).
- Cliente e vendedor, na consulta e na listagem, aparecem com número e nome (sugestão aceita).
- O cancelamento de itens, que também afeta o subtotal, fica para a spec de alteração e cancelamento. Nesta spec, o subtotal soma todos os itens incluídos.
- Em aberto: como cada item é identificado para ser consultado e, depois, alterado. Fica para a spec de alteração.
