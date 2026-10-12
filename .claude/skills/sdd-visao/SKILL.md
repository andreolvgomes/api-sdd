---
name: sdd-visao
description: Explora uma ideia grande, levanta os pontos que o usuário precisa pensar e divide em uma lista ordenada de specs. Use apenas quando o pedido não cabe em uma única spec (várias specs, vários módulos, integração nova ou semanas de trabalho). Não cria specs nem escreve código.
disable-model-invocation: true
---

# /sdd-visao

Objetivo: ajudar o usuário a pensar sobre uma ideia grande antes de
implementar. A skill explora a feature, levanta pontos para reflexão,
registra as decisões que o usuário tomar e divide o trabalho em specs
pequenas, em ordem.

Esta skill NÃO cria specs e NÃO escreve código.

## Regras gerais
- O ponto de partida é o texto do usuário, sem alterações.
- Separe sempre:
  - **Decisões confirmadas**: só o que o usuário escreveu ou confirmou.
  - **Pontos para reflexão**: o que você levantou e o usuário ainda não decidiu.
- Nunca mova um ponto para "Decisões confirmadas" sem o usuário confirmar.
- Use linguagem simples, em português.

## Passos

1. **Registrar o pedido original**
   Guarde o texto do usuário exatamente como ele escreveu.

2. **Mapear o que já existe**
   Leia apenas o necessário do código para identificar módulos, entidades,
   rotas e cadastros que a feature vai usar ou alterar.
   Das specs concluídas, olhe só os nomes das pastas, a menos que precise de
   algum detalhe.

3. **Explorar a feature**
   Com base no pedido e no código, levante os pontos que o usuário precisa
   pensar. Use as categorias abaixo quando fizerem sentido para a feature:
   - **Fluxo**: como a feature é usada do início ao fim.
   - **Dados**: entidades e campos prováveis, o que é copiado de outros
     cadastros e o que é só referência.
   - **Cálculos**: totais, descontos, arredondamentos, quem calcula.
   - **Estados**: situações por que o registro passa e o que pode ser feito
     em cada uma.
   - **Casos de borda**: o que acontece quando algo sai do caminho normal.
   - **Consistência**: transações, concorrência, o que não pode ficar pela
     metade.
   - **Riscos**: o que pode dar problema depois se não for pensado agora.

   Para cada ponto, escreva como pergunta, com opções e o impacto de cada
   uma quando houver mais de um caminho. Não decida pelo usuário.

4. **Apresentar e discutir**
   Mostre os pontos para o usuário. Deixe que ele responda os que quiser,
   na ordem que quiser. O que ele decidir vai para "Decisões confirmadas";
   o que ele não responder continua em "Pontos para reflexão".
   Não é necessário resolver tudo agora: muitos pontos serão decididos
   na spec correspondente.

5. **Propor a divisão em specs**
   Primeiro, mostre ao usuário só a lista ordenada (título e uma frase do
   que cada item entrega) e ajuste até ele confirmar a divisão.

   Depois, escreva para cada item um **pedido completo para a `/sdd-spec`**.
   Cada pedido será copiado para uma sessão nova da IA, que não conhece a
   visão nem os outros itens. Por isso, cada pedido deve conter:
   - **Contexto da feature**: o que é a feature como um todo e para que
     serve, em poucas linhas.
   - **O que já existe**: o que estará pronto no sistema quando este item
     for implementado (cadastros existentes e o que os itens anteriores
     entregam), descrito como funcionalidade, sem citar números de itens.
   - **O que esta spec entrega**: descrição do comportamento esperado.
   - **Regras**: todas as decisões confirmadas que se aplicam, por extenso.
   - **Fora do escopo**: o que NÃO deve ser feito agora, incluindo o que
     os próximos itens vão fazer, para a IA não adiantar trabalho.
   - **Pontos em aberto**: os pontos para reflexão deste item, como
     perguntas.

   Regras para os pedidos:
   - Nunca escreva "mesmas regras do item X", "ver item X" ou "conforme a
     visão": repita o que for necessário.
   - Se dois itens repetem quase todas as regras e diferem em pouco,
     junte-os em um só.
   - Regra que só passa a valer quando outro item existir (ex.: uma trava
     criada por pagamentos) pertence ao item que cria essa regra, não ao
     item que será afetado por ela.

   Critério de corte: a menor parte que já funciona e pode ser testada
   sozinha. O primeiro item deve ser a base dos demais.
   Ajuste a lista até o usuário confirmar.

6. **Salvar**
   Salve em `specs/_visoes/<tema-em-kebab-case>.md`, usando o modelo abaixo.
   Se já existir visão para o mesmo tema, atualize-a em vez de criar outra.

7. **Encerrar**
   Informe o caminho do arquivo e sugira o comando `/sdd-spec` para o
   primeiro item. Não crie a spec.

## Modelo do documento

```markdown
# Visão: <Tema>

## Pedido original
<texto do usuário, sem alterações>

## Objetivo
<o que se quer alcançar, em poucas linhas>

## O que já existe
- <cadastros, entidades e rotas que serão usados ou alterados>

## Decisões confirmadas
- ...

## Pontos para reflexão
### Fluxo
- <pergunta> — opções: A (impacto) / B (impacto)
### Dados
- ...
### Cálculos
- ...
### Estados
- ...
### Casos de borda
- ...
### Consistência
- ...
### Riscos
- ...

## Fora do escopo
- ...

## Specs (em ordem)
- [ ] 1. <Título> - <o que entrega>
- [ ] 2. <Título> - <o que entrega>

## Pedidos para /sdd-spec

### 1. <Título>
~~~
/sdd-spec <Título>

Contexto da feature:
<o que é a feature e para que serve>

O que já existe:
- <cadastros e funcionalidades prontos neste momento>

O que esta spec entrega:
<comportamento esperado>

Regras:
- <decisão por extenso>

Fora do escopo:
- <o que não fazer agora, inclusive o que virá depois>

Pontos em aberto:
- <pergunta>
~~~

### 2. <Título>
~~~
/sdd-spec <Título>
...mesma estrutura, completa, sem referenciar o item 1...
~~~
```

## Depois de criada
- O documento é um norte, não um contrato. Pode ser atualizado a qualquer
  momento rodando `/sdd-visao <tema>` novamente, por exemplo para registrar
  novas decisões.
- Esta skill não tem ligação com as outras skills do kit. O usuário copia
  o pedido pronto do item desejado e cola na `/sdd-spec`, e marca o
  andamento na lista à mão (ex.: `- [x] 1. Montagem do orçamento - ... (spec 012)`).
- Se decisões mudarem depois que alguns itens já foram implementados, rode a
  skill de novo: ela atualiza os pedidos dos itens ainda não implementados.
