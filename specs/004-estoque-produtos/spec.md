# Spec: Campo estoque de produtos

## Objetivo
Permitir que cada produto tenha um campo de estoque, com o número de unidades disponíveis, que pode ser consultado pela empresa. O valor não é informado no cadastro nem na alteração do produto; ele só será alterado pelas futuras operações de entrada de produtos e de saída em vendas.

## Regras
- **R1** — Um produto tem o campo estoque, que guarda um número de unidades com até 2 casas decimais.
- **R2** — Ao cadastrar um produto, o estoque é `0.00`.
- **R3** — Os produtos já cadastrados antes desta spec ficam com estoque `0.00`.
- **R4** — Cadastrar um produto com o campo estoque informado é recusado com a mensagem "Estoque não pode ser informado nesta operação", e nada é gravado.
- **R5** — Alterar um produto com o campo estoque informado é recusado com a mensagem "Estoque não pode ser informado nesta operação", e nada é alterado.
- **R6** — Consultar um produto retorna o estoque dele.
- **R7** — A listagem de produtos mostra o estoque de cada produto.
- **R8** — Desativar ou ativar um produto não altera o estoque.
- **R9** — Excluir um produto remove o estoque junto com ele.
- **R10** — Nenhuma operação desta spec altera o estoque, além da definição de `0.00` no cadastro (R2). Alterações de estoque só existirão nas futuras operações de entrada de produtos e de saída em vendas.

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| Cadastrar código `P001`, descrição `Caneta azul`, preço `3.50`, sem estoque | Criado com estoque `0.00` |
| Cadastrar código `P002`, descrição `Borracha`, preço `1.00`, estoque `5` | Recusado: "Estoque não pode ser informado nesta operação"; nada gravado |
| Cadastrar código `P003`, descrição `Lápis`, preço `2.00`, estoque `0` | Recusado: "Estoque não pode ser informado nesta operação"; nada gravado |
| Consultar código `P001`, cadastrado antes desta spec | Retorna estoque `0.00` |
| Alterar `P001` com descrição `Caneta azul fina`, preço `3.90`, sem estoque | Atualizado; estoque continua `0.00` |
| Alterar `P001` com estoque `10` | Recusado: "Estoque não pode ser informado nesta operação"; produto continua igual, com estoque `0.00` |
| Desativar `P001` (estoque `0.00`) | Produto inativado; consultar `P001` mostra estoque `0.00` |
| Listar produtos, com `P001` e `P002` | Cada produto aparece com o seu estoque |
| Excluir `P001` | Produto removido junto com o estoque; consultar `P001` depois retorna "Produto não encontrado" |

## Fora do escopo
- Entrada de produtos (aumento do estoque)
- Saída de produtos em pedidos ou vendas (redução do estoque)
- Ajuste manual de estoque
- Histórico de movimentações de estoque
- Alerta de estoque mínimo
- Reserva de unidades

## Decisões
- Nenhuma operação de cadastro ou alteração define o estoque; o campo só pode ser consultado, e será alterado apenas pelas futuras operações de entrada e saída (resposta do usuário, pergunta 2 da entrevista atual, com a sugestão aceita).
- Estoque inicial `0.00` para produtos novos e para os já cadastrados (resposta do usuário, pergunta 1).
- Cadastro e alteração com o campo estoque informado são recusados, com a mensagem sugerida (sugestão aceita, pergunta 2).
- Consulta e listagem mostram o estoque, e ativar ou desativar não o altera (sugestão aceita, pergunta 3).
- Valores válidos para o estoque nas futuras operações: aceita negativo, até 2 casas decimais e limite máximo de 999999999,99 (sugestão aceita, pergunta 4). Essas regras não podem ser verificadas nesta spec, porque nenhuma operação aqui altera o valor; ficam registradas para as specs de entrada e saída.
- Excluir um produto remove o estoque junto, porque a exclusão é definitiva (resposta do usuário, pergunta 5).
- Esta spec substitui a spec de estoque que foi excluída pelo usuário, com o mesmo conteúdo ajustado pelas respostas desta entrevista.
