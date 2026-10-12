# Spec: Cadastro de formas de pagamento

## Objetivo
Permite cadastrar, consultar, alterar e excluir formas de pagamento. Cada forma de pagamento tem um número de identificação, uma descrição, um tipo (dinheiro, cartão de crédito, cartão de débito, pix, boleto, crediário ou outros) e uma situação (ativo ou inativo). A exclusão é definitiva.

## Regras
- **R1** — O `id` de cada forma de pagamento é gerado pelo sistema na inclusão, como número inteiro sequencial começando em 1. O usuário não informa nem altera o `id`.
- **R2** — A `descricao` é obrigatória. Antes de validar, espaços do início e do fim são removidos. Descrição vazia (ou só com espaços) retorna o erro "Descrição é obrigatória.". Descrição com menos de 3 ou mais de 100 caracteres retorna o erro "Descrição deve ter entre 3 e 100 caracteres.".
- **R3** — Não pode existir duas formas de pagamento com a mesma descrição. A comparação não diferencia maiúsculas de minúsculas. Descrição repetida, na inclusão ou na alteração, retorna o erro "Já existe uma forma de pagamento com esta descrição.". Na alteração, a própria forma de pagamento não conta como repetida.
- **R4** — O `tipo` é obrigatório e é informado por número inteiro, conforme a tabela de tipos abaixo. Número fora da tabela, ou tipo não informado, retorna o erro "Tipo inválido.". Valor que não é número inteiro (ex.: texto como `pix`) é recusado pelo formato da requisição, com resposta de requisição inválida (400) e sem a mensagem "Tipo inválido.". A mesma regra vale para o filtro de tipo na listagem. Tabela de tipos:
  - 1 — Dinheiro
  - 2 — Cartão de crédito
  - 3 — Cartão de débito
  - 4 — Pix
  - 5 — Boleto
  - 6 — Crediário
  - 7 — Outros
- **R5** — Não existe limite de formas de pagamento do mesmo tipo. Pode haver várias formas de pagamento com o mesmo tipo, desde que as descrições sejam diferentes (R3).
- **R6** — O `status` só pode ser `Ativo` ou `Inativo`, sem diferenciar maiúsculas de minúsculas (ex.: `ativo`, `ATIVO` e `atiVO` são o mesmo status). O status é sempre gravado como `Ativo` ou `Inativo`. Na inclusão, se o status não for informado, a forma de pagamento entra como `Ativo`. O status pode ser alterado a qualquer momento. Valor diferente de `Ativo` e `Inativo` (em qualquer capitalização) retorna o erro "Status inválido.".
- **R7** — Incluir uma forma de pagamento com dados válidos retorna a forma de pagamento criada, com `id`, `descricao`, `tipo` e `status`. O `tipo` é devolvido como número, conforme a tabela da R4.
- **R8** — Alterar uma forma de pagamento pelo `id` troca a `descricao`, o `tipo` e/ou o `status`, e retorna a forma de pagamento alterada. Aplicam-se R2, R3, R4 e R6 às informações alteradas. Se o `id` não existir, retorna o erro "Forma de pagamento não encontrada.".
- **R9** — Obter uma forma de pagamento pelo `id` retorna seus dados. Se o `id` não existir, retorna o erro "Forma de pagamento não encontrada.".
- **R10** — Listar sem filtro mostra todas as formas de pagamento, inclusive as inativas, ordenadas pelo `id`. Os filtros por `status` e por `tipo` são opcionais e podem ser usados juntos. O filtro por `status` não diferencia maiúsculas de minúsculas. Filtro de `status` com valor diferente de `Ativo` e `Inativo` retorna o erro "Status inválido.". Filtro de `tipo` com valor fora da tabela da R4 retorna o erro "Tipo inválido.". Se não houver formas de pagamento para o filtro, a listagem retorna uma lista vazia, sem erro.
- **R11** — Excluir uma forma de pagamento pelo `id` remove o registro definitivamente do banco de dados. Depois da exclusão, a forma de pagamento não aparece em nenhuma consulta.
- **R12** — Excluir um `id` que não existe retorna o erro "Forma de pagamento não encontrada.". Nada é removido.
- **R13** — Depois de excluída, obter ou alterar a forma de pagamento pelo mesmo `id` retorna o erro "Forma de pagamento não encontrada.".

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Incluir: descricao "Dinheiro", tipo 1, sem status | Forma de pagamento id 1, descricao "Dinheiro", tipo 1, status Ativo |
| Incluir: descricao "Pix Banco X", tipo 4, status "Inativo" | Forma de pagamento id 2, descricao "Pix Banco X", tipo 4, status Inativo |
| Incluir: descricao "Pix Banco Y", tipo 4 (mesmo tipo de id 2) | Forma de pagamento id 3 criada, com tipo 4 |
| Incluir: descricao "" (vazia) | Erro: "Descrição é obrigatória." |
| Incluir: descricao "  Pi  " (2 caracteres após remover os espaços) | Erro: "Descrição deve ter entre 3 e 100 caracteres." |
| Incluir: descricao com 101 letras "A" seguidas | Erro: "Descrição deve ter entre 3 e 100 caracteres." |
| Incluir: descricao "pix banco x" quando já existe "Pix Banco X" | Erro: "Já existe uma forma de pagamento com esta descrição." |
| Incluir: descricao "Vale", tipo 8 | Erro: "Tipo inválido." |
| Incluir: descricao "Vale", tipo "pix" (texto) | Erro 400 (requisição inválida), sem a mensagem "Tipo inválido." |
| Incluir: descricao "Vale", sem tipo | Erro: "Tipo inválido." |
| Incluir: descricao "Vale", tipo 7, status "Pendente" | Erro: "Status inválido." |
| Alterar id 1: descricao "Dinheiro em espécie" | Forma de pagamento id 1, descricao "Dinheiro em espécie", tipo 1, status Ativo |
| Alterar id 1: tipo 4 | Forma de pagamento id 1, tipo 4, descricao "Dinheiro" mantida |
| Alterar id 1: status "Inativo" | Forma de pagamento id 1, status Inativo |
| Alterar id 2: descricao "Dinheiro" (já usada pelo id 1) | Erro: "Já existe uma forma de pagamento com esta descrição." |
| Alterar id 2: tipo 9 | Erro: "Tipo inválido." |
| Alterar id 999: descricao "Cheque" | Erro: "Forma de pagamento não encontrada." |
| Obter id 2 | Forma de pagamento id 2, descricao "Pix Banco X", tipo 4, status Inativo |
| Obter id 999 | Erro: "Forma de pagamento não encontrada." |
| Listar sem filtro, com as formas de pagamento id 1 (Ativo) e id 2 (Inativo) | Lista com as 2, ordenada por id |
| Listar com status "Inativo" | Apenas a forma de pagamento id 2 |
| Listar com tipo 4 | Formas de pagamento id 2 e id 3 |
| Listar com status "Inativo" e tipo 4 | Apenas a forma de pagamento id 2 |
| Listar com status "Pendente" | Erro: "Status inválido." |
| Listar com tipo 8 | Erro: "Tipo inválido." |
| Listar com tipo "pix" (texto) | Erro 400 (requisição inválida), sem a mensagem "Tipo inválido." |
| Listar com tipo 6 quando não há crediário cadastrado | Lista vazia (não é erro) |
| Listar sem nenhuma forma de pagamento cadastrada | Lista vazia (não é erro) |
| Excluir id 2 | Forma de pagamento removida do banco; obter id 2 depois retorna "Forma de pagamento não encontrada." |
| Excluir id 999 | Erro: "Forma de pagamento não encontrada." |
| Alterar id 2 depois da exclusão | Erro: "Forma de pagamento não encontrada." |

## Fora do escopo
- Verificar se a forma de pagamento está em uso em outros módulos antes de excluir. Isso será definido quando existirem módulos de venda.
- Inativar a forma de pagamento no lugar da exclusão definitiva.
- Exclusão em lote de formas de pagamento.
- Desfazer uma exclusão.
- Histórico de alterações da forma de pagamento.
- Regras de parcelamento, taxas, prazos de recebimento ou vínculo com conta bancária.
- Tipos além dos sete da tabela da R4.

## Decisões
- A feature cobre as operações de CRUD: incluir, alterar, obter, listar e excluir (seguindo o padrão de vendedores e produtos).
- `id` gerado pelo sistema, sequencial a partir de 1, não editável. Segue o padrão da spec de vendedores (`specs/005-cadastro-vendedores`, R1), sem confirmação específica do usuário.
- Tipo informado por número inteiro conforme a tabela da R4 (resposta do usuário: "deve ser o int, criar um enum"). A numeração de 1 a 7, seguindo a ordem da lista pedida pelo usuário, é decisão desta spec, sem confirmação específica do usuário.
- Descrição com 3 a 100 caracteres, sem repetição e sem diferenciar maiúsculas de minúsculas, com as mensagens de erro da spec de vendedores (sugestão aceita pelo usuário).
- Status com os valores `Ativo` e `Inativo`, padrão `Ativo` na inclusão, alterável a qualquer momento, sem diferenciar maiúsculas de minúsculas (sugestão aceita pelo usuário).
- Pode haver mais de uma forma de pagamento do mesmo tipo, desde que as descrições sejam diferentes (sugestão aceita pelo usuário).
- Listagem sem filtro mostra todas, inclusive inativas, ordenadas pelo `id`. Filtros opcionais por status e por tipo (sugestão aceita pelo usuário).
- Exclusão definitiva, sem verificação de uso em outros módulos. Essa verificação fica para quando existirem módulos de venda (pedido do usuário, seguindo a spec de vendedores).
- Tipo enviado como número inteiro (resposta do usuário). Texto no campo `tipo` não recebe a mensagem "Tipo inválido.": a requisição é recusada antes da regra (opção escolhida pelo usuário). Altera o Exemplo de texto `pix` que antes dizia "Tipo inválido." (R4).
- Rota da API: `/api/formas-pagamento` (resposta do usuário).
