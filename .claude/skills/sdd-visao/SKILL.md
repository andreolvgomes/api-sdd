---
name: sdd-visao
description: Transforma uma ideia grande em um documento de visão com a lista ordenada de specs. Use apenas quando o pedido não cabe em uma única spec (várias specs, vários módulos, integração nova ou semanas de trabalho). Não cria specs nem escreve código.
---

# /sdd-visao

Objetivo: dar um norte para uma ideia grande, dividindo-a em specs pequenas,
em ordem, cada uma testável sozinha.

Esta skill NÃO cria specs e NÃO escreve código.

## Regras gerais
- Baseie-se apenas no que o usuário escreveu e no que ele confirmou.
- Tudo que você sugerir (divisão, regras, comportamentos) é proposta: apresente
  como pergunta e só registre no documento após o usuário confirmar.
- O que não foi dito nem confirmado vai para "Questões em aberto", nunca é
  assumido.
- Use linguagem simples, em português.

## Passos

1. **Registrar o pedido original**
   Guarde o texto do usuário exatamente como ele escreveu.

2. **Entender**
   Se algo importante para a divisão não estiver claro (objetivo, escopo, o que
   fica de fora), pergunte. Uma pergunta de cada vez, no máximo 5.
   Se estiver tudo claro, siga direto.

3. **Mapear o impacto no código**
   Leia apenas o necessário do código existente para identificar módulos,
   entidades, rotas e cadastros envolvidos.
   Das specs concluídas, olhe só os nomes das pastas, a menos que precise de
   algum detalhe.

4. **Propor a divisão**
   Monte uma lista ordenada de specs. Para cada item: título curto, uma frase
   do que entrega e de qual item anterior depende (se depender).
   Critério de corte: a menor parte que já funciona e pode ser testada sozinha.
   O primeiro item deve ser a base dos demais.
   Apresente a lista ao usuário e ajuste até ele confirmar.

5. **Salvar**
   Salve em `specs/_visoes/<tema-em-kebab-case>.md`, usando o modelo abaixo.
   Se já existir visão para o mesmo tema, atualize-a em vez de criar outra.

6. **Encerrar**
   Informe o caminho do arquivo e sugira o comando `/sdd-spec` para o primeiro
   item pendente. Não crie a spec.

## Modelo do documento

```markdown
# Visão: <Tema>

## Pedido original
<texto do usuário, sem alterações>

## Objetivo
<o que se quer alcançar, em poucas linhas>

## Fora do escopo
- ...

## Impacto no sistema
- <módulos, entidades, rotas e cadastros afetados>

## Decisões confirmadas
- ...

## Questões em aberto
- ...

## Specs (em ordem)
- [ ] 1. <Título> - <o que entrega>
- [ ] 2. <Título> - <o que entrega> (depende de 1)
```

## Depois de criada
- A lista é um norte, não um contrato. Pode ser alterada a qualquer momento
  rodando `/sdd-visao <tema>` novamente.
- Quando a spec de um item for criada, anote o número dela ao lado do item
  (ex.: `- [ ] 1. Montagem do orçamento - ... (spec 012)`).
- Quando a spec for concluída, marque o item com `[x]`.
