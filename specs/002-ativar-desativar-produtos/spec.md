# Spec: Ativar e desativar produtos

## Objetivo
Permitir ativar e desativar produtos, sem apagar o cadastro, para que a empresa controle quais produtos estão disponíveis, com o mesmo comportamento de ativar e desativar clientes.

## Regras
- **R1** — Desativar um produto ativo muda o status dele para `inativo`. O registro continua existindo.
- **R2** — Desativar um produto que já está inativo é recusado com a mensagem "Produto já está inativo", e nada é alterado.
- **R3** — Ativar um produto inativo muda o status dele para `ativo`.
- **R4** — Ativar um produto que já está ativo é recusado com a mensagem "Produto já está ativo", e nada é alterado.
- **R5** — Desativar ou ativar um código que não existe é recusado com a mensagem "Produto não encontrado".
- **R6** — Desativar e ativar, quando dão certo, respondem somente com confirmação de sucesso, sem devolver os dados do produto.
- **R7** — Desativar e ativar não exigem motivo e não registram quem fez a mudança.
- **R8** — Ativar e desativar são ações separadas da exclusão. A exclusão continua definitiva, como na spec de produtos (R13), e vale para produtos ativos e inativos.
- **R9** — A alteração de status pela alteração completa do produto (spec de produtos, R12) continua valendo. Ativar e desativar são um atalho para a mesma mudança de status.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Desativar o produto `P001` (ativo) | Confirmação de sucesso, sem dados do produto; consultar `P001` depois mostra status `inativo` |
| Desativar o produto `P001` (já inativo) | Recusado: "Produto já está inativo"; produto continua inativo |
| Ativar o produto `P001` (inativo) | Confirmação de sucesso, sem dados do produto; consultar `P001` depois mostra status `ativo` |
| Ativar o produto `P001` (já ativo) | Recusado: "Produto já está ativo"; produto continua ativo |
| Desativar o código `X999` (não existe) | Recusado: "Produto não encontrado" |
| Ativar o código `X999` (não existe) | Recusado: "Produto não encontrado" |
| Excluir o produto `P001` (inativo) | Produto removido; consultar `P001` depois retorna "Produto não encontrado" |
| Alterar `P001` (inativo) com status `ativo` | Produto reativado com os demais campos mantidos |
| Listar com filtro `inativo`, com `P001` inativo e `P002` ativo | Retorna somente `P001` |

## Fora do escopo
- Histórico de quem ativou ou desativou um produto
- Motivo de desativação
- Exclusão lógica ou lixeira, com recuperação de um produto excluído
- Trocar a exclusão definitiva de produtos por inativação (ver Em aberto)
- Bloqueio de ativar ou desativar produto com vínculos em outros módulos

## Decisões
- Ativar e desativar seguem a spec de clientes (`specs/001-ativar-desativar-clientes`): mesmas regras e mesmo formato de resposta, trocando "Cliente" por "Produto" nas mensagens. Sugestão desta spec, não confirmada pelo usuário.
- Desativar um produto já inativo é recusado, e ativar um produto já ativo também (R2 e R4), como na spec de clientes. Sugestão desta spec, não confirmada pelo usuário.
- A exclusão de produto não muda: continua definitiva (spec de produtos, R13). Este pedido trata só de ativar e desativar.
- Em aberto: a spec de clientes pergunta se produtos deve passar a usar inativação no lugar da exclusão definitiva. Esta spec não decide isso e mantém a exclusão definitiva.
