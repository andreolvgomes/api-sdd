# Spec: Cadastro de produtos

## Objetivo
Permitir cadastrar, consultar, alterar e excluir produtos, cada um com código, descrição, preço de venda e status (ativo ou inativo).

## Regras
- **R1** — Um produto tem os campos código, descrição, preço de venda e status.
- **R2** — O código é obrigatório, deve ser único entre todos os produtos e aceita apenas letras e números, com no máximo 20 caracteres.
- **R3** — O código não pode ser alterado depois que o produto é criado.
- **R4** — A descrição é obrigatória, tem no máximo 100 caracteres e não pode ser composta só por espaços.
- **R5** — O preço de venda é obrigatório, aceita zero e não aceita valor negativo.
- **R6** — O preço de venda aceita no máximo 2 casas decimais.
- **R7** — O status aceita somente os valores "ativo" e "inativo".
- **R8** — Ao cadastrar sem informar o status, o produto é criado com status "ativo".
- **R9** — Cadastrar um produto com código já existente é recusado e nada é gravado.
- **R10** — A listagem mostra todos os produtos por padrão e pode ser filtrada por status "ativo" ou "inativo".
- **R11** — Consultar um produto pelo código retorna todos os seus campos.
- **R12** — Alterar um produto permite mudar descrição, preço de venda e status. Código não pode ser alterado (ver R3).
- **R13** — Excluir um produto remove o registro do banco de dados de forma definitiva. Não existe exclusão lógica.
- **R14** — Consultar, alterar ou excluir um código que não existe é recusado com a mensagem "Produto não encontrado".
- **R15** — Uma alteração que viola alguma regra de campo (R4 a R7) é recusada e nada é alterado.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar código `P001`, descrição `Caneta azul`, preço `3.50`, sem status | Criado com status `ativo` |
| Cadastrar código `P001`, descrição `Caneta vermelha`, preço `4.00` (já existe `P001`) | Recusado: "Código já cadastrado" |
| Cadastrar código `P002`, descrição `Borracha`, preço `0` | Criado com preço `0.00` |
| Cadastrar código `P003`, descrição `Lápis`, preço `-1.00` | Recusado: "Preço não pode ser negativo" |
| Cadastrar código `P004`, descrição `Lápis`, preço `2.555` | Recusado: "Preço aceita no máximo 2 casas decimais" |
| Cadastrar código `P005`, descrição `   ` (só espaços), preço `1.00` | Recusado: "Descrição é obrigatória" |
| Cadastrar código `P006` com 21 caracteres, descrição `Régua`, preço `1.00` | Recusado: "Código deve ter no máximo 20 caracteres" |
| Cadastrar código `P-007`, descrição `Régua`, preço `1.00` | Recusado: "Código aceita apenas letras e números" |
| Cadastrar com descrição de 101 caracteres | Recusado: "Descrição deve ter no máximo 100 caracteres" |
| Cadastrar sem código | Recusado: "Código é obrigatório" |
| Cadastrar com status `pausado` | Recusado: "Status deve ser ativo ou inativo" |
| Listar sem filtro, com 2 ativos e 1 inativo | Retorna os 3 produtos |
| Listar com filtro `inativo`, com 2 ativos e 1 inativo | Retorna somente o produto inativo |
| Consultar código `P001` | Retorna código, descrição, preço e status |
| Consultar código `X999` (não existe) | Recusado: "Produto não encontrado" |
| Alterar `P001`: descrição `Caneta azul fina`, preço `3.90`, status `inativo` | Produto atualizado com esses valores |
| Alterar `P001` com preço `-2.00` | Recusado: "Preço não pode ser negativo"; produto continua igual |
| Excluir `P001` | Produto removido; consultar `P001` depois retorna "Produto não encontrado" |
| Excluir `X999` (não existe) | Recusado: "Produto não encontrado" |

## Fora do escopo
- Controle de estoque
- Categorias ou grupos de produtos
- Histórico de alterações de preço
- Imagens ou anexos de produto
- Exclusão lógica ou lixeira (exclusão é sempre definitiva)
- Permissões por usuário

## Decisões
- Exclusão é física: o registro é removido do banco (resposta do usuário, pergunta 1).
- Código: obrigatório, único, até 20 caracteres alfanuméricos, não alterável (sugestão aceita).
- Descrição: obrigatória, até 100 caracteres, não pode ser só espaços (sugestão aceita).
- Preço de venda: aceita zero, até 2 casas decimais, não aceita negativo (resposta do usuário, pergunta 4).
- Status: aceita "ativo" e "inativo"; padrão "ativo"; listagem com filtro opcional por status (sugestão aceita).
