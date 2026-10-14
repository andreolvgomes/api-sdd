Status: rascunho

# Design: 012-bloqueio-clientes

## Resumo
Cada cliente ganha uma coluna de nível de bloqueio, com valor padrão "liberado". O lançamento de pagamento de orçamento consulta esse nível e recusa formas que não sejam Dinheiro ou Pix quando o cliente está bloqueado. O bloqueio é alterado por um endpoint próprio, no mesmo padrão de ativar e desativar.

## Dados
Tabela alterada: `cliente`, com a coluna nova `clie_bloqueio`, texto, obrigatória, padrão `liberado`. Valores aceitos: `liberado` e `bloqueado`.

Script SQL:

```sql
alter table cliente add column clie_bloqueio text not null default 'liberado';
```

No `create table cliente` também entra `clie_bloqueio text not null default 'liberado'`, para banco novo.

Migração dos dados existentes: nenhuma linha precisa ser recalculada. O padrão `liberado` vale para todos os clientes que já existem, o que é o comportamento pedido pela spec (R1). No `Banco.CriarTabelasAsync`, a coluna é adicionada só quando não existe, no mesmo padrão já usado para os totais do orçamento.

## Contratos

| Método | Caminho | Recebe | Devolve |
|---|---|---|---|
| PUT | `/api/clientes/{Clie_id}/bloqueio` | `{ "clie_bloqueio": "bloqueado" }` | 200 com o cliente, incluindo `clie_bloqueio`. 404 se o cliente não existe. 400 se o nível não é `liberado` ou `bloqueado`. |
| GET | `/api/clientes/{Clie_id}` | — | Cliente detalhado, incluindo `clie_bloqueio`. |
| GET | `/api/clientes` | — | Lista de clientes, incluindo `clie_bloqueio`. |
| POST | `/api/clientes` | sem mudança | 201, cliente criado com `clie_bloqueio` = `liberado`. |
| POST | `/api/orcamentos/{Orca_id}/pagamentos` | sem mudança | Sem mudança de contrato. Passa a devolver 400 com a mensagem "Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)" quando a forma de pagamento não é permitida para o cliente bloqueado. |

## Interface
Não se aplica. Não há tela nesta versão: a alteração é feita pela API.

## Arquivos
**Novos:**
- `Erp.Application.Abstractions/Clientes/Dtos/AlterarBloqueioClienteRequest.cs`: entrada com `Clie_id` e `Clie_bloqueio`.
- `Erp.Api/Features/Clientes/AlterarBloqueioClienteEndpoint.cs`: endpoint `PUT /api/clientes/{Clie_id}/bloqueio`.
- `tests/Erp.Api.Tests/S013BloqueioClientesTests.cs`: testes das regras R1 a R10.

**Alterados:**
- `Erp.Core/Entities/Cliente.cs`: nova propriedade `Clie_bloqueio`.
- `Erp.Infra.Data/Banco.cs`: coluna no `create table cliente` e migração que adiciona a coluna quando falta.
- `Erp.Infra.Data/Repositories/ClienteRepository.cs`: lê e grava `clie_bloqueio`.
- `Erp.Application.Abstractions/Clientes/IClientesService.cs`: novo método `AlterarBloqueioAsync`.
- `Erp.Application.Abstractions/Clientes/Dtos/ClienteResponse.cs` e `ClienteDetalheResponse.cs`: nova propriedade `Clie_bloqueio`.
- `Erp.Application/Clientes/ClientesService.cs`: `AlterarBloqueioAsync`, e clientes novos entram como `liberado`.
- `Erp.Application/Clientes/ClienteValidacao.cs`: validação do nível (`liberado` ou `bloqueado`).
- `Erp.Application/Clientes/ClienteMapeamento.cs`: devolve `Clie_bloqueio`.
- `Erp.Application/Orcamentos/OrcamentoValidacao.cs`: regra de forma permitida para cliente bloqueado (Dinheiro e Pix).
- `Erp.Application/Orcamentos/OrcamentosService.cs`: checagem em `LancarPagamentoAsync`, depois de validar a forma e o caixa e antes de gravar.

## Regras → onde
| Regra | Onde é aplicada |
|---|---|
| R1 | `Banco.cs` (padrão `liberado`); `ClientesService.IncluirAsync` (cliente novo entra como `liberado`) |
| R2 | `AlterarBloqueioClienteEndpoint` (PUT) → `ClientesService.AlterarBloqueioAsync` |
| R3 | `ClientesService.AlterarBloqueioAsync` (404 antes de gravar) |
| R4 | `OrcamentoValidacao` (regra não se aplica a cliente liberado) e `OrcamentosService.LancarPagamentoAsync` |
| R5 | `OrcamentoValidacao` (lista de formas permitidas: Dinheiro e Pix) |
| R6 | `OrcamentosService.LancarPagamentoAsync`, antes de gravar |
| R7 | `ErroDeNegocio` do tipo Validacao, devolvido como 400 |
| R8 | Nenhuma mudança: o bloqueio só é checado no lançamento, e pagamentos antigos não são revalidados |
| R9 | Nenhuma mudança: `IncluirItem`, `AlterarItem`, `FecharAsync` e `CancelarPagamentoAsync` não chamam a checagem |
| R10 | Nenhuma mudança: ao desbloquear, o próximo lançamento passa pela checagem com o nível novo |

## Decisões técnicas
- Endpoint próprio para o bloqueio (`PUT /api/clientes/{Clie_id}/bloqueio`), no lugar de um campo no PUT de cliente. Alternativa descartada: campo `clie_bloqueio` no PUT atual, que exige nome, endereço e telefone, então enviar só o nível não funcionaria sem mudar a regra de obrigatoriedade já existente.
- Permissão restrita a Dinheiro (tipo 1) e Pix (tipo 4), pelo campo `Tipo` da forma de pagamento. Alternativa descartada: marcar cada forma com um flag próprio "à vista", que duplicaria o que o tipo já informa.
- A checagem fica no serviço de orçamentos, e não no endpoint, porque é regra de negócio.

## Impacto
- Clientes existentes ficam `liberado` e não mudam de comportamento.
- Quebra de contrato: nenhuma. Os campos novos são adicionados às respostas de cliente, e quem ignora campos desconhecidos não é afetado.
- Pagamentos já lançados não são alterados quando o cliente é bloqueado (R8).
- Ponto em aberto: o tipo `Outros` (7) não está na lista da spec. Como R5 só permite Dinheiro e Pix, ele fica bloqueado para cliente bloqueado. Confirme se é esse o comportamento desejado.

## Verificação
- Testes automatizados em `S013BloqueioClientesTests.cs`, com uma chamada à API para cada linha da tabela de Exemplos da spec: cliente liberado e bloqueado com cada forma de pagamento, bloqueio de cliente inexistente, nível inválido, pagamento antigo mantido após o bloqueio, e desbloqueio seguido de pagamento com cartão.
- Teste de migração em `BancoTests.cs`: banco antigo sem a coluna ganha `clie_bloqueio` com valor `liberado`.
- Suíte completa de testes continua passando (386 testes existentes e os novos).
- Conferência manual: não se aplica nesta versão, pois não há tela.
