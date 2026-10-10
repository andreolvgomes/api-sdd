# Spec: Cadastro de clientes

## Objetivo
Permitir cadastrar, consultar, alterar e excluir clientes, cada um com id, nome completo, endereço, telefone e status (ativo ou inativo), para que a empresa saiba quem são seus clientes e como encontrá-los.

## Regras
- **R1** — Um cliente tem os campos id, nome completo, endereço, telefone e status.
- **R2** — O id é gerado pelo sistema como número sequencial a partir de 1. Não pode ser informado no cadastro nem alterado depois.
- **R3** — O nome completo é obrigatório, tem no máximo 100 caracteres e não pode ser composto só por espaços.
- **R4** — O endereço é obrigatório, é um texto livre com no máximo 200 caracteres e não pode ser composto só por espaços.
- **R5** — O telefone é obrigatório, é um texto livre com no máximo 20 caracteres e não pode ser composto só por espaços. Não há validação de formato.
- **R6** — Cadastrar um cliente com dados válidos cria o cliente com um id novo e devolve todos os seus campos, incluindo o id e o status.
- **R7** — Consultar um cliente pelo id retorna todos os seus campos, incluindo o status. Isso vale para clientes ativos e inativos.
- **R8** — Consultar, alterar ou excluir um id que não existe é recusado com a mensagem "Cliente não encontrado".
- **R9** — Alterar um cliente permite mudar nome completo, endereço, telefone e status. O id não muda. As regras R3, R4 e R5 valem para os novos valores.
- **R10** — Uma alteração com algum campo inválido é recusada e nada é alterado.
- **R11** — Excluir um cliente ativo não remove o registro: muda o status para inativo. O cliente continua existindo e pode ser reativado (R9).
- **R12** — A listagem mostra por padrão só os clientes ativos, ordenados pelo nome completo. Com o filtro de status `inativo`, mostra só os inativos. Com o filtro `todos`, mostra todos os clientes. Os três casos são ordenados pelo nome completo.
- **R13** — Se não houver nenhum cliente no filtro escolhido, a listagem retorna uma lista vazia, sem erro.
- **R14** — Um cadastro ou alteração com campo obrigatório ausente ou vazio é recusado com a mensagem correspondente ao campo: "Nome completo é obrigatório", "Endereço é obrigatório" ou "Telefone é obrigatório".
- **R15** — Um campo que ultrapassa o tamanho máximo é recusado com a mensagem correspondente: "Nome completo deve ter no máximo 100 caracteres", "Endereço deve ter no máximo 200 caracteres" ou "Telefone deve ter no máximo 20 caracteres".
- **R16** — Um cliente nasce ativo. No cadastro, o status é opcional; sem ela, o cliente é criado como ativo.
- **R17** — O status informada no cadastro ou na alteração aceita somente os valores "ativo" e "inativo". Outro valor é recusado com a mensagem "Status deve ser ativo ou inativo", e nada é gravado ou alterado.
- **R18** — O filtro de status da listagem aceita somente os valores "ativo", "inativo" e "todos". Outro valor é recusado com a mensagem "Status deve ser ativo, inativo ou todos".
- **R19** — Excluir um cliente que já está inativo é recusado com a mensagem "Cliente já está inativo", e nada é alterado.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar nome `Maria Silva`, endereço `Rua das Flores, 10, Centro`, telefone `11 91234-5678` | Criado com id `1`, status `ativo` e os três campos informados |
| Cadastrar nome `João Souza`, endereço `Av. Brasil, 200`, telefone `21 99876-5432` (já existe cliente id 1) | Criado com id `2`, status `ativo`, sem alterar o cliente 1 |
| Cadastrar nome `Ana Lima`, endereço `Rua C, 3`, telefone `31 90000-0003`, status `inativo` | Criado com id `3` e status `inativo` |
| Cadastrar com status `pausado` | Recusado: "Status deve ser ativo ou inativo" |
| Cadastrar sem nome completo | Recusado: "Nome completo é obrigatório" |
| Cadastrar com nome `   ` (só espaços) | Recusado: "Nome completo é obrigatório" |
| Cadastrar com nome de 101 caracteres | Recusado: "Nome completo deve ter no máximo 100 caracteres" |
| Cadastrar com endereço vazio | Recusado: "Endereço é obrigatório" |
| Cadastrar com endereço de 201 caracteres | Recusado: "Endereço deve ter no máximo 200 caracteres" |
| Cadastrar com telefone vazio | Recusado: "Telefone é obrigatório" |
| Cadastrar com telefone de 21 caracteres | Recusado: "Telefone deve ter no máximo 20 caracteres" |
| Cadastrar com telefone `abc` | Criado; o formato não é validado |
| Consultar id `1` | Retorna id, nome, endereço, telefone e status |
| Consultar id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Alterar id `1` com nome `Maria Silva Santos` e mesmo endereço, telefone e status | Atualizado; o id continua `1` |
| Alterar id `1` com endereço vazio | Recusado: "Endereço é obrigatório"; cliente continua igual |
| Alterar id `1` com status `pausado` | Recusado: "Status deve ser ativo ou inativo"; cliente continua igual |
| Alterar id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Alterar id `1`, que está inativo, com status `ativo` | Reativado; o id continua `1` |
| Listar sem nenhum cliente cadastrado | Retorna lista vazia |
| Listar sem filtro, com `Maria Silva` (ativo), `João Souza` (inativo) e `Ana Lima` (ativo) | Retorna na ordem: Ana Lima, Maria Silva |
| Listar com filtro `inativo`, com os mesmos três clientes | Retorna somente João Souza |
| Listar com filtro `todos`, com os mesmos três clientes | Retorna na ordem: Ana Lima, João Souza, Maria Silva |
| Listar com filtro `pausado` | Recusado: "Status deve ser ativo, inativo ou todos" |
| Excluir id `1` (ativo) | Cliente inativado; consultar id `1` depois retorno status `inativo` |
| Excluir id `1` quando ele já está inativo | Recusado: "Cliente já está inativo" |
| Excluir id `99` (não existe) | Recusado: "Cliente não encontrado" |

## Fora do escopo
- Vínculo entre cliente e pedidos ou vendas
- Validação de formato do telefone (DDD, quantidade de dígitos, máscara)
- Endereço separado em partes (rua, número, cidade, CEP)
- Busca por nome ou texto na listagem (o filtro é só por status)
- Documento do cliente (CPF, CNPJ)
- Exclusão definitiva de cliente pela API
- Histórico de alterações de cadastro
- Nomes repetidos são permitidos; cada cliente é identificado pelo id

## Decisões
- Id: gerado automaticamente, número sequencial, não alterável (resposta do usuário, pergunta 1).
- Nome completo: obrigatório, até 100 caracteres, não pode ser só espaços (sugestão aceita).
- Endereço: texto livre, obrigatório, até 200 caracteres (sugestão aceita).
- Telefone: texto livre, obrigatório, com tamanho máximo (resposta do usuário, pergunta 4). O limite de 20 caracteres é sugestão desta spec, porque o usuário não informou um número. Pode ser ajustado.
- Listagem: ordenada por nome completo (sugestão aceita, pergunta 5). Por padrão mostra só os ativos.
- Status incluído depois da primeira versão, com o mesmo comportamento de produtos: nasce ativo, a alteração pode mudar o status, e a listagem tem filtro por status.
- Exclusão de cliente inativa o cliente em vez de remover, e a reativação é feita pela alteração do status. Esse comportamento foi pedido pelo usuário.
- Em aberto: em produtos a exclusão é definitiva (spec de produtos, R13). Clientes passam a ser inativados. Confirmar se produtos deve mudar para o mesmo modelo.
- Sugestão desta spec, não confirmada pelo usuário: excluir um cliente já inativo é recusado (R19).
