# Spec: Endereços adicionais de clientes

## Objetivo
Permitir cadastrar, consultar, alterar e excluir endereços adicionais de um cliente, além do endereço principal do cadastro, para que a empresa saiba todos os locais associados a cada cliente, como entrega e cobrança.

## Regras
- **R1** — Um cliente pode ter até 10 endereços adicionais. O endereço principal do cadastro não conta nesse limite.
- **R2** — Cada endereço adicional tem um rótulo e um texto de endereço. Os dois são obrigatórios.
- **R3** — O rótulo é obrigatório, tem no máximo 30 caracteres e não pode ser composto só por espaços. Ausente ou vazio: "Rótulo é obrigatório". Mais de 30 caracteres: "Rótulo deve ter no máximo 30 caracteres".
- **R4** — O texto de endereço é obrigatório, tem no máximo 200 caracteres e não pode ser composto só por espaços. Ausente ou vazio: "Endereço é obrigatório". Mais de 200 caracteres: "Endereço deve ter no máximo 200 caracteres".
- **R5** — Cadastrar o 11º endereço adicional de um cliente é recusado com a mensagem "Cliente já possui 10 endereços adicionais", e nada é gravado.
- **R6** — Cadastrar, consultar, alterar ou excluir endereço adicional de um cliente que não existe é recusado com a mensagem "Cliente não encontrado".
- **R7** — Cadastrar um endereço adicional com dados válidos cria o endereço com um id novo e devolve o id, o rótulo e o texto de endereço. O id é gerado pelo sistema como número sequencial a partir de 1, dentro de cada cliente.
- **R8** — O endereço principal do cliente (o endereço do cadastro) não muda por causa dos endereços adicionais. Não é possível marcar um endereço adicional como principal, nem trocar o principal por um adicional.
- **R9** — Alterar um endereço adicional permite mudar rótulo e texto de endereço, com as regras R3 e R4. O id não muda. Uma alteração com campo inválido é recusada e nada é alterado.
- **R10** — Alterar ou excluir um id de endereço adicional que não existe no cliente é recusado com a mensagem "Endereço não encontrado".
- **R11** — Excluir um endereço adicional remove o endereço definitivamente. Não existe exclusão lógica. Depois da exclusão, o cliente pode cadastrar outro endereço adicional, se estiver abaixo do limite de R1.
- **R12** — Consultar um cliente retorna os campos do cadastro e a lista dos seus endereços adicionais, na ordem do id. Quando o cliente não tem endereços adicionais, a lista vem vazia, sem erro.
- **R13** — A listagem de clientes não mostra os endereços adicionais. Ela continua mostrando só os campos do cadastro.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar endereço adicional no cliente `1` com rótulo `Entrega`, texto `Av. Brasil, 200, Centro` | Criado com id `1`, rótulo `Entrega` e o texto informado |
| Cadastrar endereço adicional no cliente `1` com rótulo `Cobrança`, texto `Rua C, 3` (já existe o id 1) | Criado com id `2`; o id 1 não muda |
| Cadastrar endereço adicional sem rótulo | Recusado: "Rótulo é obrigatório"; nada gravado |
| Cadastrar endereço adicional com rótulo `   ` (só espaços) | Recusado: "Rótulo é obrigatório"; nada gravado |
| Cadastrar endereço adicional com rótulo de 31 caracteres | Recusado: "Rótulo deve ter no máximo 30 caracteres" |
| Cadastrar endereço adicional com texto vazio | Recusado: "Endereço é obrigatório" |
| Cadastrar endereço adicional com texto de 201 caracteres | Recusado: "Endereço deve ter no máximo 200 caracteres" |
| Cadastrar o 11º endereço adicional do cliente `1` | Recusado: "Cliente já possui 10 endereços adicionais"; nada gravado |
| Cadastrar endereço adicional no cliente `99` (não existe) | Recusado: "Cliente não encontrado" |
| Consultar o cliente `1`, que tem o endereço principal `Rua das Flores, 10, Centro` e os adicionais id 1 `Entrega` e id 2 `Cobrança` | Retorna os campos do cadastro e os dois adicionais, na ordem do id |
| Consultar o cliente `1`, sem endereços adicionais | Retorna os campos do cadastro com lista vazia de adicionais |
| Consultar o cliente `99` (não existe) | Recusado: "Cliente não encontrado" |
| Alterar o endereço adicional id `1` do cliente `1` com rótulo `Entrega principal` e o mesmo texto | Atualizado; o id continua `1` |
| Alterar o endereço adicional id `1` do cliente `1` com texto vazio | Recusado: "Endereço é obrigatório"; endereço continua igual |
| Alterar o endereço adicional id `99` do cliente `1` (não existe) | Recusado: "Endereço não encontrado" |
| Alterar endereço adicional do cliente `99` (não existe) | Recusado: "Cliente não encontrado" |
| Excluir o endereço adicional id `2` do cliente `1` | Endereço removido; consultar o cliente `1` não mostra mais o id 2 |
| Excluir o endereço adicional id `99` do cliente `1` (não existe) | Recusado: "Endereço não encontrado" |
| Excluir o endereço adicional de um cliente `99` (não existe) | Recusado: "Cliente não encontrado" |
| Cliente `1` com 10 adicionais excluindo um deles e cadastrando outro | Exclusão aceita; novo cadastro criado, com o limite de 10 respeitado |
| Listar clientes, com o cliente `1` que tem 2 endereços adicionais | A listagem mostra o cliente `1` só com os campos do cadastro |

## Fora do escopo
- Marcar um endereço adicional como principal, ou trocar o principal
- Status ativo ou inativo para endereço adicional
- Validação de CEP, formato de endereço ou separação em partes (rua, número, cidade)
- Busca por endereço ou filtro na listagem de clientes
- Histórico de alterações de endereços
- Coordenadas ou validação do endereço em mapas
- Vínculo de endereço com pedidos ou entregas

## Decisões
- Limite de 10 endereços adicionais por cliente, sem contar o principal (sugestão aceita, pergunta 3).
- Endereço adicional tem rótulo obrigatório de até 30 caracteres e texto de até 200 caracteres, como o principal (sugestão aceita, pergunta 1).
- O endereço principal continua sendo o campo de endereço do cadastro do cliente, intacto. Os adicionais ficam em uma lista separada, e nenhum deles pode virar principal (resposta do usuário, pergunta 2).
- Excluir endereço adicional é definitivo, e alterar é permitido (sugestão aceita, pergunta 4).
- Endereços adicionais aparecem na consulta do cliente e não aparecem na listagem (sugestão aceita, pergunta 5).
- Id do endereço adicional é sequencial a partir de 1, dentro de cada cliente, e a ordem da consulta é pelo id. Sugestão desta spec, não confirmada pelo usuário.
- Esta spec não altera as regras das specs de clientes (`specs/clientes`, `specs/001-ativar-desativar-clientes`). O endereço principal segue a regra R4 de clientes.
- Em aberto: o que acontece com os endereços adicionais quando o cliente é inativado. Sugestão: continuam guardados e visíveis na consulta, sem mudança.
