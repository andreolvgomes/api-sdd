# Spec: Cadastro de vendedores

## Objetivo
Permite cadastrar, consultar, alterar e excluir vendedores. Cada vendedor tem um número de identificação, um nome e uma situação (ativo ou inativo). A exclusão é definitiva.

## Regras
- **R1** — O `id` de cada vendedor é gerado pelo sistema na inclusão, como número inteiro sequencial começando em 1. O usuário não informa nem altera o `id`.
- **R2** — O `nome` é obrigatório. Antes de validar, espaços do início e do fim são removidos. Nome vazio (ou só com espaços) retorna o erro "Nome é obrigatório.". Nome com menos de 3 ou mais de 100 caracteres retorna o erro "Nome deve ter entre 3 e 100 caracteres.".
- **R3** — Não pode existir dois vendedores com o mesmo nome. A comparação não diferencia maiúsculas de minúsculas. Nome repetido, na inclusão ou na alteração, retorna o erro "Já existe um vendedor com este nome.". Na alteração, o próprio vendedor não conta como repetido.
- **R4** — O `status` só pode ser `Ativo` ou `Inativo`, sem diferenciar maiúsculas de minúsculas (ex.: `ativo`, `ATIVO` e `atiVO` são o mesmo status). O status é sempre gravado como `Ativo` ou `Inativo`. Na inclusão, se o status não for informado, o vendedor entra como `Ativo`. O status pode ser alterado a qualquer momento. Valor diferente de `Ativo` e `Inativo` (em qualquer capitalização) retorna o erro "Status inválido.".
- **R5** — Incluir um vendedor com dados válidos retorna o vendedor criado, com `id`, `nome` e `status`.
- **R6** — Alterar um vendedor pelo `id` troca o `nome` e/ou o `status` e retorna o vendedor alterado. Aplicam-se R2, R3 e R4 às informações alteradas. Se o `id` não existir, retorna o erro "Vendedor não encontrado.".
- **R7** — Obter um vendedor pelo `id` retorna seus dados. Se o `id` não existir, retorna o erro "Vendedor não encontrado.".
- **R8** — Listar sem filtro mostra todos os vendedores, inclusive os inativos, ordenados pelo `id`. O filtro por `status` é opcional: quando informado, retorna apenas os vendedores com aquele status, sem diferenciar maiúsculas de minúsculas. Filtro com valor diferente de `Ativo` e `Inativo` retorna o erro "Status inválido.". Se não houver vendedores, a listagem retorna uma lista vazia, sem erro.
- **R9** — Excluir um vendedor pelo `id` remove o registro definitivamente do banco de dados. Depois da exclusão, o vendedor não aparece em nenhuma consulta.
- **R10** — Excluir um `id` que não existe retorna o erro "Vendedor não encontrado.". Nada é removido.
- **R11** — Depois de excluído, obter ou alterar o vendedor pelo mesmo `id` retorna o erro "Vendedor não encontrado.".

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Incluir: nome "Ana Souza", sem status | Vendedor id 1, nome "Ana Souza", status Ativo |
| Incluir: nome "Carlos Lima", status "Inativo" | Vendedor id 2, nome "Carlos Lima", status Inativo |
| Incluir: nome "" (vazio) | Erro: "Nome é obrigatório." |
| Incluir: nome "  Al  " (2 caracteres após remover os espaços) | Erro: "Nome deve ter entre 3 e 100 caracteres." |
| Incluir: nome com 101 letras "A" seguidas | Erro: "Nome deve ter entre 3 e 100 caracteres." |
| Incluir: nome "ana souza" quando já existe "Ana Souza" | Erro: "Já existe um vendedor com este nome." |
| Incluir: nome "Pedro Alves", status "Pendente" | Erro: "Status inválido." |
| Alterar id 1: nome "Ana Paula Souza" | Vendedor id 1, nome "Ana Paula Souza", status Ativo |
| Alterar id 1: status "Inativo" | Vendedor id 1, status Inativo |
| Alterar id 2: nome "Ana Souza" (já usado pelo id 1) | Erro: "Já existe um vendedor com este nome." |
| Alterar id 999: nome "Marcos Reis" | Erro: "Vendedor não encontrado." |
| Obter id 2 | Vendedor id 2, nome "Carlos Lima", status Inativo |
| Obter id 999 | Erro: "Vendedor não encontrado." |
| Listar sem filtro, com os vendedores id 1 (Ativo) e id 2 (Inativo) | Lista com os 2 vendedores, ordenada por id |
| Listar com status "Inativo" | Apenas o vendedor id 2 |
| Listar com status "Pendente" | Erro: "Status inválido." |
| Listar sem nenhum vendedor cadastrado | Lista vazia (não é erro) |
| Excluir id 2 | Vendedor removido do banco; obter id 2 depois retorna "Vendedor não encontrado." |
| Excluir id 999 | Erro: "Vendedor não encontrado." |
| Alterar id 2 depois da exclusão | Erro: "Vendedor não encontrado." |

## Fora do escopo
- Verificar se o vendedor está em uso em outros módulos antes de excluir. Isso será definido quando existirem módulos de venda.
- Definir quais módulos usam vendedor e como esse vínculo é registrado.
- Exclusão em lote de vendedores.
- Desfazer uma exclusão.
- Histórico de alterações do vendedor.
- Controle de comissão, metas ou dados pessoais além de nome e status.

## Decisões
- A feature cobre as operações de CRUD: incluir, alterar, obter, listar e excluir (resposta do usuário).
- `id` gerado pelo sistema, sequencial a partir de 1, não editável (resposta do usuário).
- Status com os valores `Ativo` e `Inativo`, padrão `Ativo` na inclusão, alterável a qualquer momento (resposta do usuário).
- Status não diferencia maiúsculas de minúsculas: `ATIVO`, `ativo`, `Ativo` e `atiVO` são o mesmo status (resposta do usuário). Altera a R4 e a R8 desta mesma spec, que está em andamento.
- Nome com 3 a 100 caracteres, sem repetição e sem diferenciar maiúsculas de minúsculas, e mensagens de erro conforme a sugestão aceita pelo usuário.
- Exclusão definitiva, sem verificação de uso em outros módulos. Essa verificação fica para quando existirem módulos de venda (resposta do usuário).
- Listagem sem filtro mostra todos, inclusive inativos (resposta do usuário: "mostra tudo"). O filtro por status é opcional (decisão de implementação de R8).
