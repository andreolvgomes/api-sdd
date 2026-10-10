# Spec: Cadastro de clientes

## Objetivo
Permitir cadastrar, consultar, alterar e excluir clientes, cada um com id, nome completo, endereço e telefone, para que a empresa saiba quem são seus clientes e como encontrá-los.

## Regras
- **R1** — Um cliente tem os campos id, nome completo, endereço e telefone.
- **R2** — O id é gerado pelo sistema como número sequencial a partir de 1. Não pode ser informado no cadastro nem alterado depois.
- **R3** — O nome completo é obrigatório, tem no máximo 100 caracteres e não pode ser composto só por espaços.
- **R4** — O endereço é obrigatório, é um texto livre com no máximo 200 caracteres e não pode ser composto só por espaços.
- **R5** — O telefone é obrigatório, é um texto livre com no máximo 20 caracteres e não pode ser composto só por espaços. Não há validação de formato.
- **R6** — Cadastrar um cliente com dados válidos cria o cliente com um id novo e devolve todos os seus campos, incluindo o id.
- **R7** — Consultar um cliente pelo id retorna todos os seus campos.
- **R8** — Consultar, alterar ou excluir um id que não existe é recusado com a mensagem "Cliente não encontrado".
- **R9** — Alterar um cliente permite mudar nome completo, endereço e telefone. O id não muda. As regras R3, R4 e R5 valem para os novos valores.
- **R10** — Uma alteração com algum campo inválido é recusada e nada é alterado.
- **R11** — Excluir um cliente remove o registro de forma definitiva. Não existe exclusão lógica.
- **R12** — A listagem mostra todos os clientes ordenados pelo nome completo, sem filtro.
- **R13** — Se não houver nenhum cliente cadastrado, a listagem retorna uma lista vazia, sem erro.
- **R14** — Um cadastro ou alteração com campo obrigatório ausente ou vazio é recusado com a mensagem correspondente ao campo: "Nome completo é obrigatório", "Endereço é obrigatório" ou "Telefone é obrigatório".
- **R15** — Um campo que ultrapassa o tamanho máximo é recusado com a mensagem correspondente: "Nome completo deve ter no máximo 100 caracteres", "Endereço deve ter no máximo 200 caracteres" ou "Telefone deve ter no máximo 20 caracteres".

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar nome `Maria Silva`, endereço `Rua das Flores, 10, Centro`, telefone `11 91234-5678` | Criado com id `1` e os três campos informados |
| Cadastrar nome `João Souza`, endereço `Av. Brasil, 200`, telefone `21 99876-5432` (já existe cliente id 1) | Criado com id `2`, sem alterar o cliente 1 |
| Cadastrar sem nome completo | Recusado: "Nome completo é obrigatório" |
| Cadastrar com nome `   ` (só espaços) | Recusado: "Nome completo é obrigatório" |
| Cadastrar com nome de 101 caracteres | Recusado: "Nome completo deve ter no máximo 100 caracteres" |
| Cadastrar com endereço vazio | Recusado: "Endereço é obrigatório" |
| Cadastrar com endereço de 201 caracteres | Recusado: "Endereço deve ter no máximo 200 caracteres" |
| Cadastrar com telefone vazio | Recusado: "Telefone é obrigatório" |
| Cadastrar com telefone de 21 caracteres | Recusado: "Telefone deve ter no máximo 20 caracteres" |
| Cadastrar com telefone `abc` | Criado; o formato não é validado |
| Consultar id `1` | Retorna id, nome, endereço e telefone |
| Consultar id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Alterar id `1` com nome `Maria Silva Santos` e mesmo endereço e telefone | Atualizado; o id continua `1` |
| Alterar id `1` com endereço vazio | Recusado: "Endereço é obrigatório"; cliente continua igual |
| Alterar id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Listar sem nenhum cliente cadastrado | Retorna lista vazia |
| Listar com `Maria Silva`, `João Souza` e `Ana Lima` cadastrados | Retorna na ordem: Ana Lima, João Souza, Maria Silva |
| Excluir id `1` | Cliente removido; consultar id `1` depois retorna "Cliente não encontrado" |
| Excluir id `99` (não existe) | Recusado: "Cliente não encontrado" |

## Fora do escopo
- Vínculo entre cliente e pedidos ou vendas
- Validação de formato do telefone (DDD, quantidade de dígitos, máscara)
- Endereço separado em partes (rua, número, cidade, CEP)
- Busca ou filtro na listagem
- Documento do cliente (CPF, CNPJ)
- Exclusão lógica ou lixeira (exclusão é sempre definitiva)
- Histórico de alterações de cadastro
- Nomes repetidos são permitidos; cada cliente é identificado pelo id

## Decisões
- Id: gerado automaticamente, número sequencial, não alterável (resposta do usuário, pergunta 1).
- Nome completo: obrigatório, até 100 caracteres, não pode ser só espaços (sugestão aceita).
- Endereço: texto livre, obrigatório, até 200 caracteres (sugestão aceita).
- Telefone: texto livre, obrigatório, com tamanho máximo (resposta do usuário, pergunta 4). O limite de 20 caracteres é sugestão desta spec, porque o usuário não informou um número. Pode ser ajustado.
- Listagem e exclusão: listagem de todos os clientes ordenada por nome, sem filtro; exclusão definitiva e sem bloqueio (sugestão aceita, pergunta 5).
