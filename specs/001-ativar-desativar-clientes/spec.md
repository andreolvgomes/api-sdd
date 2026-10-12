# Spec: Ativar, desativar e excluir clientes

## Objetivo
Permitir ativar e desativar clientes, sem apagar o cadastro, e excluir definitivamente um cliente cadastrado por engano, para que a empresa controle quem está ativo e corrija erros de cadastro.

## Regras
- **R1** — Um cliente tem status, que pode ser somente `ativo` ou `inativo`.
- **R2** — Um cliente é criado com status `ativo`, a menos que outro status válido seja informado no cadastro.
- **R3** — Um status diferente de `ativo` e `inativo`, no cadastro ou na alteração, é recusado com a mensagem "Status deve ser ativo ou inativo", e nada é gravado.
- **R4** — Desativar um cliente ativo muda o status dele para `inativo`. O registro continua existindo.
- **R5** — Desativar um cliente que já está inativo é recusado com a mensagem "Cliente já está inativo", e nada é alterado.
- **R6** — Ativar um cliente inativo muda o status dele para `ativo`.
- **R7** — Ativar um cliente que já está ativo é recusado com a mensagem "Cliente já está ativo", e nada é alterado.
- **R8** — Desativar ou ativar um id que não existe é recusado com a mensagem "Cliente não encontrado".
- **R9** — Desativar e ativar, quando dão certo, respondem somente com confirmação de sucesso, sem devolver os dados do cliente.
- **R10** — Desativar e ativar não exigem motivo e não registram quem fez a mudança.
- **R11** — Excluir remove o cliente definitivamente do banco de dados. Não existe exclusão lógica: o cliente excluído não pode ser consultado nem reativado.
- **R12** — Excluir vale para clientes ativos e inativos, e para clientes recém-cadastrados, sem restrição de tempo desde o cadastro.
- **R13** — Excluir um id que não existe é recusado com a mensagem "Cliente não encontrado".

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar nome `Maria Silva`, endereço `Rua das Flores, 10, Centro`, telefone `11 91234-5678` | Criado com status `ativo` |
| Cadastrar nome `Ana Lima`, endereço `Rua C, 3`, telefone `31 90000-0003`, status `inativo` | Criado com status `inativo` |
| Cadastrar nome `João Souza`, endereço `Av. Brasil, 200`, telefone `21 99876-5432`, status `registrado` | Recusado: "Status deve ser ativo ou inativo"; nada gravado |
| Desativar o cliente `1` (ativo) | Confirmação de sucesso, sem dados do cliente; consultar o cliente `1` depois mostra status `inativo` |
| Desativar o cliente `1` (já inativo) | Recusado: "Cliente já está inativo"; cliente continua inativo |
| Ativar o cliente `1` (inativo) | Confirmação de sucesso, sem dados do cliente; consultar o cliente `1` depois mostra status `ativo` |
| Ativar o cliente `1` (já ativo) | Recusado: "Cliente já está ativo"; cliente continua ativo |
| Desativar o id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Ativar o id `99` (não existe) | Recusado: "Cliente não encontrado" |
| Excluir o cliente `1` (ativo, cadastrado por engano minutos atrás) | Cliente removido; consultar o cliente `1` depois retorna "Cliente não encontrado" |
| Excluir o cliente `2` (inativo) | Cliente removido; consultar o cliente `2` depois retorna "Cliente não encontrado" |
| Excluir o id `99` (não existe) | Recusado: "Cliente não encontrado" |

## Fora do escopo
- Histórico de quem ativou ou desativou um cliente
- Motivo de desativação
- Exclusão lógica ou lixeira, com recuperação de um cliente excluído
- Status além de `ativo` e `inativo`
- Bloqueio de exclusão quando o cliente tiver vínculos com outros módulos (ver Em aberto)

## Decisões
- Status: somente `ativo` e `inativo`, sem novo status para cadastro recente (resposta do usuário, pergunta 1).
- Exclusão é definitiva: remove o registro do banco de dados. Substitui a regra de exclusão por inativação que estava na spec de clientes (resposta do usuário, pergunta 3 da entrevista anterior).
- Desativar e ativar são ações separadas da exclusão, e respondem só com confirmação de sucesso (resposta do usuário, perguntas 4 e 5).
- Mensagens de recusa de desativar e ativar aceitas como sugeridas: "Cliente já está inativo" e "Cliente já está ativo" (sugestões aceitas, perguntas 1 e 2).
- Cliente recém-cadastrado por engano pode ser excluído sem restrição (resposta do usuário, pergunta 1 da entrevista atual).
- Em aberto: bloqueio da exclusão quando o cliente tiver vínculos com registros de outros módulos (tabelas com chave estrangeira). A mensagem sugerida é "Cliente possui vínculos e não pode ser excluído". Nenhum módulo referencia clientes hoje, então a regra fica para quando um módulo desse tipo existir.
