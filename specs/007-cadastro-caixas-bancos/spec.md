# Spec: Cadastro de caixas e bancos

## Objetivo
Permite cadastrar, consultar, alterar e excluir caixas e bancos. Cada caixa ou banco tem um número de identificação, uma descrição e uma situação (ativo ou inativo). A exclusão é definitiva.

## Regras
- **R1** — O `id` de cada caixa ou banco é gerado pelo sistema na inclusão, como número inteiro sequencial começando em 1. O usuário não informa nem altera o `id`.
- **R2** — A `descricao` é obrigatória. Antes de validar, espaços do início e do fim são removidos. Descrição vazia (ou só com espaços) retorna o erro "Descrição é obrigatória.". Descrição com menos de 3 ou mais de 100 caracteres retorna o erro "Descrição deve ter entre 3 e 100 caracteres.".
- **R3** — Não pode existir dois caixas ou bancos com a mesma descrição. A comparação não diferencia maiúsculas de minúsculas. Descrição repetida, na inclusão ou na alteração, retorna o erro "Já existe um caixa ou banco com esta descrição.". Na alteração, o próprio caixa ou banco não conta como repetido.
- **R4** — O `status` só pode ser `Ativo` ou `Inativo`, sem diferenciar maiúsculas de minúsculas (ex.: `ativo`, `ATIVO` e `atiVO` são o mesmo status). O status é sempre gravado como `Ativo` ou `Inativo`. Na inclusão, se o status não for informado, o caixa ou banco entra como `Ativo`. O status pode ser alterado a qualquer momento. Valor diferente de `Ativo` e `Inativo` (em qualquer capitalização) retorna o erro "Status inválido.".
- **R5** — Incluir um caixa ou banco com dados válidos retorna o registro criado, com `id`, `descricao` e `status`.
- **R6** — Alterar um caixa ou banco pelo `id` troca a `descricao` e/ou o `status` e retorna o registro alterado. Aplicam-se R2, R3 e R4 às informações alteradas. Se o `id` não existir, retorna o erro "Caixa ou banco não encontrado.".
- **R7** — Obter um caixa ou banco pelo `id` retorna seus dados. Se o `id` não existir, retorna o erro "Caixa ou banco não encontrado.".
- **R8** — Listar sem filtro mostra todos os caixas e bancos, inclusive os inativos, ordenados pelo `id`. O filtro por `status` é opcional: quando informado, retorna apenas os registros com aquele status, sem diferenciar maiúsculas de minúsculas. Filtro com valor diferente de `Ativo` e `Inativo` retorna o erro "Status inválido.". Se não houver caixas ou bancos, a listagem retorna uma lista vazia, sem erro.
- **R9** — Excluir um caixa ou banco pelo `id` remove o registro definitivamente do banco de dados. Depois da exclusão, ele não aparece em nenhuma consulta.
- **R10** — Excluir um `id` que não existe retorna o erro "Caixa ou banco não encontrado.". Nada é removido.
- **R11** — Depois de excluído, obter ou alterar o caixa ou banco pelo mesmo `id` retorna o erro "Caixa ou banco não encontrado.".

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Incluir: descricao "Caixa Loja Centro", sem status | Caixa ou banco id 1, descricao "Caixa Loja Centro", status Ativo |
| Incluir: descricao "Banco do Brasil", status "Inativo" | Caixa ou banco id 2, descricao "Banco do Brasil", status Inativo |
| Incluir: descricao "" (vazia) | Erro: "Descrição é obrigatória." |
| Incluir: descricao "  Ca  " (2 caracteres após remover os espaços) | Erro: "Descrição deve ter entre 3 e 100 caracteres." |
| Incluir: descricao com 101 letras "A" seguidas | Erro: "Descrição deve ter entre 3 e 100 caracteres." |
| Incluir: descricao com 100 letras "A" seguidas | Caixa ou banco criado, com descricao de 100 caracteres |
| Incluir: descricao "caixa loja centro" quando já existe "Caixa Loja Centro" | Erro: "Já existe um caixa ou banco com esta descrição." |
| Incluir: descricao "Caixa Filial", status "Pendente" | Erro: "Status inválido." |
| Alterar id 1: descricao "Caixa Loja Centro 2" | Caixa ou banco id 1, descricao "Caixa Loja Centro 2", status Ativo |
| Alterar id 1: status "Inativo" | Caixa ou banco id 1, status Inativo |
| Alterar id 2: descricao "Caixa Loja Centro" (já usada pelo id 1) | Erro: "Já existe um caixa ou banco com esta descrição." |
| Alterar id 1: descricao "CAIXA LOJA CENTRO" (a mesma, com outra caixa) | Caixa ou banco id 1 alterado, sem erro |
| Alterar id 999: descricao "Caixa Filial" | Erro: "Caixa ou banco não encontrado." |
| Obter id 2 | Caixa ou banco id 2, descricao "Banco do Brasil", status Inativo |
| Obter id 999 | Erro: "Caixa ou banco não encontrado." |
| Listar sem filtro, com os registros id 1 (Ativo) e id 2 (Inativo) | Lista com os 2 registros, ordenada por id |
| Listar com status "Inativo" | Apenas o registro id 2 |
| Listar com status "iNaTiVo" | Apenas o registro id 2 |
| Listar com status "Pendente" | Erro: "Status inválido." |
| Listar sem nenhum caixa ou banco cadastrado | Lista vazia (não é erro) |
| Excluir id 2 | Registro removido do banco; obter id 2 depois retorna "Caixa ou banco não encontrado." |
| Excluir id 999 | Erro: "Caixa ou banco não encontrado." |
| Alterar id 2 depois da exclusão | Erro: "Caixa ou banco não encontrado." |

## Fora do escopo
- Campos além de `id`, `descricao` e `status`, como saldo, agência, conta ou moeda.
- Movimentações financeiras, saldo ou extrato de caixa ou banco.
- Verificar se o caixa ou banco está em uso em outros módulos antes de excluir. Isso será definido quando existirem módulos que usem caixas ou bancos.
- Inativar no lugar da exclusão definitiva.
- Exclusão em lote.
- Desfazer uma exclusão.
- Histórico de alterações do caixa ou banco.

## Decisões
- A feature cobre as operações de CRUD: incluir, alterar, obter, listar e excluir (seguindo o padrão de formas de pagamento e vendedores).
- Campos: `id`, `descricao` e `status`, apenas (resposta do usuário).
- `id` gerado pelo sistema, sequencial a partir de 1, não editável (padrão de vendedores e formas de pagamento, aceito pelo usuário).
- Descrição obrigatória, com 3 a 100 caracteres, sem repetição e sem diferenciar maiúsculas de minúsculas (sugestão aceita pelo usuário).
- Status `Ativo` ou `Inativo`, sem diferenciar maiúsculas de minúsculas, padrão `Ativo` na inclusão, alterável a qualquer momento (sugestão aceita pelo usuário).
- Exclusão definitiva, sem verificação de uso em outros módulos (sugestão aceita pelo usuário).
- Listagem sem filtro mostra todos, inclusive inativos, ordenados pelo `id`. O filtro por status é opcional (sugestão aceita pelo usuário).
- Rota da API: `/api/caixas-bancos` (resposta do usuário).
- Área no Swagger: `Caixas/Bancos` (resposta do usuário).
