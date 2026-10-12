# Visão: Orçamentos

## Pedido original
/sdd-visao Orçamentos

orcamentos - cliente, vendedor
orcamentos_itens - produto
orcamentos_pagamentos - forma de pagamento, caixa

O pagamento completo deve permanecer com caixa; é uma prévia para atender
controle interno da empresa.

O registro do orçamento é salvo ao incluir o primeiro item. Depois disso,
vários outros itens serão incluídos. Cada item incluído é independente dos
outros já incluídos e deve ser salvo imediatamente, sem acumular em memória
para salvar tudo de uma única vez.

## Objetivo
Permitir montar uma prévia de orçamento para um cliente, com itens, desconto
no total e pagamentos parciais ou completos, para controle interno. A prévia
não movimenta estoque nem caixa.

## O que já existe
- Cliente, vendedor, forma de pagamento e caixa/banco: cadastros com status
  ativo/inativo (specs 001, 005, 006 e 007).
- Produto: código, preço de venda, status e estoque (specs 002 e 004). O
  estoque não é alterado por esta visão.

## Decisões confirmadas
- Cliente e vendedor são obrigatórios e precisam estar ativos.
- O registro do orçamento é criado ao incluir o primeiro item. Se salvar o
  item falhar, o orçamento continua existindo, sem itens. Isso é intencional.
- Número sequencial e data no orçamento.
- O preço do item é copiado do produto na inclusão e fica fixo até o item ser
  alterado.
- Cada item incluído é salvo na hora, independente dos outros.
- Quantidade do item deve ser maior que zero.
- Produto inativo não pode entrar como novo item. Produto sem estoque pode.
- O mesmo produto pode ser incluído várias vezes, cada vez como item
  independente.
- Cálculos no servidor: total do item = quantidade × preço; subtotal do
  orçamento = soma dos totais dos itens; total do orçamento = subtotal menos
  o desconto; saldo = total menos a soma dos pagamentos não cancelados.
- Desconto só no total, aplicado no fechamento do orçamento.
- O fechamento é só o desconto gravado, sem situação própria. O fechamento
  não trava a alteração de itens.
- Pagamentos podem ser lançados a qualquer momento depois que o orçamento
  existe, enquanto a soma dos pagamentos não passar do total do orçamento. A
  soma pode chegar exatamente ao total.
- O orçamento pode ser repetido em N pagamentos, inclusive com a mesma forma
  de pagamento.
- Pagamentos não cancelados travam a inclusão de itens, a edição e o
  cancelamento de itens e o fechamento. Como o valor a pagar depende do
  desconto e este depende do total dos itens, depois do primeiro pagamento o
  total e o desconto não podem mais mudar.
- Um pagamento pode ser cancelado (`cancelado = true`). Quando todos os
  pagamentos estiverem cancelados, as edições voltam a funcionar.
- Cadastros inativos (cliente, vendedor, forma de pagamento e caixa) não podem
  ser usados.
- O orçamento pode ser alterado a qualquer momento, exceto enquanto houver
  pagamento não cancelado.
- Item pode ser marcado como cancelado (`cancelado = true`), sem excluir do
  banco. O orçamento também pode ser marcado como cancelado.
- O caixa/banco é só referência: não movimenta saldo nesta visão. A
  movimentação só acontece quando o orçamento virar venda, fora deste escopo.

## Pontos para reflexão
### Fluxo
- Quais campos e filtros a listagem de orçamentos terá?
### Cálculos
- Ao alterar um item, o preço é atualizado sempre, ou só quando o produto do
  item for trocado?
- Quantas casas decimais têm o total do item e o subtotal?
- Itens cancelados entram no subtotal? (pergunta levada para a spec 2)
### Estados
- Enquanto houver pagamento não cancelado, a trava vale também para editar o
  cabeçalho do orçamento (cliente, vendedor)? Ou só para itens e fechamento?
- Um orçamento com pagamentos pode ser cancelado? O que acontece com os
  pagamentos?
### Consistência
- Pagamentos lançados ao mesmo tempo não podem ultrapassar o total do
  orçamento.
- Dois orçamentos criados ao mesmo tempo não podem receber o mesmo número.

## Fora do escopo
- Baixa de estoque por orçamento.
- Movimentação de saldo do caixa/banco.
- Transformar orçamento em venda.
- Desconto por item (só no total).

## Specs (em ordem)
- [ ] 1. Incluir itens no orçamento - cria o orçamento com o primeiro item, inclui os demais itens (cada um salvo na hora), calcula total do item e subtotal no servidor, e tem consulta do orçamento com itens e listagem de orçamentos.
- [ ] 2. Alterar e cancelar itens e orçamento - edição e cancelamento com `cancelado = true`, sem excluir do banco (depende de 1).
- [ ] 3. Fechar orçamento com desconto no total - grava o desconto e passa a mostrar desconto e total na consulta (depende de 1).
- [ ] 4. Lançar pagamentos e travas - N pagamentos, soma até o total; pagamento não cancelado trava itens e fechamento; cancelar todos os pagamentos libera; passa a mostrar pagamentos e saldo na consulta (depende de 1 e de 3).

## Pedidos para /sdd-spec

### 1. Incluir itens no orçamento
~~~
/sdd-spec Incluir itens no orçamento

Contexto da feature:
Prévia de orçamento para um cliente, usada para controle interno da empresa.
Um orçamento reúne itens de produtos e, depois, pagamentos. A prévia não
movimenta estoque nem caixa.

O que já existe:
- Cadastros de clientes, vendedores, produtos, formas de pagamento e caixas/bancos, com status ativo ou inativo.
- Produtos com preço de venda e estoque. O estoque não é alterado por esta funcionalidade.

O que esta spec entrega:
Inclusão de orçamento a partir do primeiro item: é criado o orçamento com cliente, vendedor, número sequencial e data, e o primeiro item é incluído. Depois, cada novo item é incluído, salvo na hora e independente dos outros. O total de cada item e o subtotal do orçamento são calculados no servidor. Também há consulta de um orçamento com seus itens e listagem de orçamentos.

Regras:
- Cliente e vendedor são obrigatórios e precisam estar ativos.
- Cadastro inativo não pode ser usado.
- O número do orçamento é sequencial e a data é a da criação.
- Cada item tem produto, quantidade e preço. O preço é copiado do preço de venda do produto no momento da inclusão e fica fixo depois disso.
- Quantidade deve ser maior que zero.
- Produto inativo não pode entrar como item. Produto sem estoque pode entrar.
- O mesmo produto pode ser incluído várias vezes, e cada inclusão cria um item independente.
- Total do item = quantidade × preço. Subtotal do orçamento = soma dos totais dos itens. Os dois são calculados no servidor, não pelo que é enviado na requisição.
- Cada item é salvo imediatamente, sem esperar os outros.
- Se salvar o item falhar depois que o orçamento foi criado, o orçamento continua existindo, sem o item. Orçamento sem itens é aceito.
- Consulta de um orçamento retorna o cabeçalho e os itens, cada um com seu total, e o subtotal.
- Listagem retorna os orçamentos existentes.
- Incluir item não altera o estoque do produto nem movimenta caixa ou banco.

Fora do escopo:
- Alterar ou cancelar itens e orçamentos (vem depois).
- Desconto, fechamento e pagamentos (vêm depois).
- Baixa de estoque.
- Movimentação de caixa ou banco.
- Transformar orçamento em venda.

Pontos em aberto:
- Quantas casas decimais têm o total do item e o subtotal?
- Quais campos e filtros a listagem de orçamentos terá?
- Dois orçamentos criados ao mesmo tempo não podem receber o mesmo número. Como garantir isso?
~~~

### 2. Alterar e cancelar itens e orçamento
~~~
/sdd-spec Alterar e cancelar itens e orçamento

Contexto da feature:
Prévia de orçamento para um cliente, usada para controle interno da empresa.
Um orçamento reúne itens de produtos e, depois, pagamentos. A prévia não
movimenta estoque nem caixa.

O que já existe:
- Orçamentos criados com cliente, vendedor, número, data e itens. Cada item tem produto, quantidade, preço copiado na inclusão, total do item. O orçamento tem subtotal.
- Consulta de um orçamento com seus itens e listagem de orçamentos.
- Cadastros de clientes, vendedores e produtos, com status ativo ou inativo.

O que esta spec entrega:
Alteração de itens e do orçamento a qualquer momento, e cancelamento de itens e do orçamento sem excluir nada do banco. O cancelamento de item sai do subtotal do orçamento.

Regras:
- Alterar um item grava a nova quantidade e/ou o novo produto e recalcula o total do item e o subtotal.
- Quantidade alterada deve ser maior que zero.
- Produto alterado precisa estar ativo. Produto sem estoque pode.
- Cancelar um item grava `cancelado = true`. O item continua no banco.
- Cancelar o orçamento grava `cancelado = true`. O orçamento continua no banco.
- Alterar cliente ou vendedor do orçamento exige que o novo cadastro esteja ativo.
- Não há trava nesta spec: a edição é liberada a qualquer momento. A trava de pagamentos vem em outra spec.

Fora do escopo:
- Fechamento e desconto (vem depois).
- Pagamentos e travas (vêm depois).
- Excluir itens ou orçamentos.
- Baixa de estoque e movimentação de caixa ou banco.

Pontos em aberto:
- Itens cancelados entram no subtotal do orçamento?
- Ao alterar um item, o preço é atualizado sempre, ou só quando o produto do item for trocado?
- Um item cancelado pode voltar a ficar ativo?
- Um orçamento cancelado aceita novos itens ou alterações?
~~~

### 3. Fechar orçamento com desconto no total
~~~
/sdd-spec Fechar orçamento com desconto no total

Contexto da feature:
Prévia de orçamento para um cliente, usada para controle interno da empresa.
Um orçamento reúne itens de produtos e, depois, pagamentos. O desconto é
dado no total do orçamento, e o valor a pagar depende dele. A prévia não
movimenta estoque nem caixa.

O que já existe:
- Orçamentos com itens, que podem ser alterados e cancelados.
- Subtotal do orçamento, calculado a partir dos itens.
- Cadastros de clientes, vendedores, produtos, formas de pagamento e caixas/bancos.

O que esta spec entrega:
Gravação do desconto no total do orçamento, feita entre a conclusão dos itens e o lançamento dos pagamentos. O fechamento é só esse desconto gravado: o orçamento não recebe uma situação própria, e o fechamento não trava a alteração de itens. A consulta do orçamento passa a mostrar o desconto e o total.

Regras:
- O desconto é aplicado ao total do orçamento, não a cada item.
- Total do orçamento = subtotal menos o desconto.
- O fechamento não trava a alteração de itens.

Fora do escopo:
- Pagamentos e travas (vêm depois).
- Desconto por item.
- Baixa de estoque e movimentação de caixa ou banco.

Pontos em aberto:
- O desconto é informado em valor ou em percentual?
- Pode-se gravar outro desconto depois do primeiro?
- Fechar um orçamento sem itens ativos (todos cancelados ou nenhum incluído) é permitido?
~~~

### 4. Lançar pagamentos e travas
~~~
/sdd-spec Lançar pagamentos e travas

Contexto da feature:
Prévia de orçamento para um cliente, usada para controle interno da empresa.
Um orçamento reúne itens de produtos e, depois, pagamentos. O valor a pagar
depende do desconto, que depende do total dos itens. A prévia não movimenta
estoque nem caixa.

O que já existe:
- Orçamentos com itens, que podem ser alterados e cancelados.
- Subtotal e desconto gravado, com total do orçamento.
- Cadastros de formas de pagamento e caixas/bancos, com status ativo ou inativo.

O que esta spec entrega:
Lançamento de pagamentos em um orçamento, com a soma limitada ao total. Pagamento não cancelado trava a inclusão, a edição e o cancelamento de itens e o fechamento. Pagamentos cancelados não travam nada. A consulta do orçamento passa a mostrar os pagamentos e o saldo.

Regras:
- Um pagamento pode ser lançado a qualquer momento depois que o orçamento existe.
- Cada pagamento tem forma de pagamento (ativa), caixa/banco (ativo) e valor.
- O orçamento pode ter N pagamentos, inclusive com a mesma forma de pagamento.
- A soma dos pagamentos não cancelados não pode passar do total do orçamento. A soma pode chegar exatamente ao total.
- Saldo = total do orçamento menos a soma dos pagamentos não cancelados.
- Enquanto houver pelo menos um pagamento não cancelado, não é possível incluir item, editar ou cancelar item, nem gravar o desconto do fechamento.
- Um pagamento pode ser cancelado (`cancelado = true`), sem excluir do banco.
- Quando todos os pagamentos estiverem cancelados, as travas saem e as edições voltam a funcionar.
- O caixa/banco é só referência: lançar pagamento não movimenta saldo do caixa ou banco.

Fora do escopo:
- Transformar orçamento em venda.
- Movimentação de saldo do caixa ou banco.
- Baixa de estoque.
- Excluir pagamentos.

Pontos em aberto:
- Enquanto houver pagamento não cancelado, a trava também vale para editar o cabeçalho (cliente, vendedor)?
- Um orçamento com pagamentos pode ser cancelado? O que acontece com os pagamentos?
- Pagamentos lançados ao mesmo tempo não podem ultrapassar o total. Como garantir isso?
~~~
