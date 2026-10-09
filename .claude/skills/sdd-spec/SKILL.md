---
name: sdd-spec
description: Entrevista o usuário sobre a ideia de uma feature e escreve a especificação em specs/<nome-curto>/spec.md. Só roda por invocação manual (/sdd-spec).
argument-hint: <ideia da feature>
disable-model-invocation: true
---

# Especificação de feature (SDD)

Ideia recebida: $ARGUMENTS

Siga as etapas em ordem. Não pule etapas e não escreva a spec antes da resposta do usuário.

## Etapa 0 — Entrada vazia

Se a ideia acima estiver vazia, peça ao usuário que descreva a feature em uma ou duas frases e pare.

## Etapa 1 — Ler a ideia e o CLAUDE.md

1. Leia a ideia acima com atenção.
2. Leia `CLAUDE.md` na raiz do projeto, se existir. Use-o apenas para entender o contexto do produto e as convenções de negócio. Se não existir, diga isso em uma linha e siga.
3. Liste para você mesmo o que está **ambíguo** ou **faltando** na ideia: termos vagos, valores não definidos, casos de erro não mencionados, limites, quem usa, o que acontece em cada situação anormal.

Não liste tecnologias, bibliotecas ou arquitetura. Esta spec não trata disso.

## Etapa 2 — Perguntar (no máximo 5)

Faça **no máximo 5 perguntas numeradas, todas de uma vez**. Para cada pergunta, dê **uma resposta sugerida** entre parênteses ou em linha separada, para o usuário poder apenas confirmar.

Formato:

```
1. <pergunta>? Sugestão: <resposta sugerida>
2. <pergunta>? Sugestão: <resposta sugerida>
...
```

Priorize as perguntas que mudam o comportamento observável (valores válidos, erros, limites, resultado esperado). Não pergunte detalhes de implementação.

Depois das perguntas, **pare e espere a resposta**. Não escreva o arquivo nesta etapa.

## Etapa 3 — Escrever a spec

Só depois que o usuário responder (mesmo que seja "pode seguir com as sugestões"):

1. Defina `<nome-curto>`: minúsculo, sem acentos, palavras separadas por hífen, 2 a 4 palavras (ex.: `soma-com-historico`).
2. Crie `specs/<nome-curto>/spec.md` na raiz do projeto.
3. Use exatamente esta estrutura:

```markdown
# Spec: <título da feature>

## Objetivo
<Uma ou duas frases: o que a feature faz e para quem. Sem tecnologia.>

## Regras
- **R1** — <regra objetiva e testável>
- **R2** — <...>

## Exemplos
| Entrada | Resultado |
|---------|-----------|
| <entrada concreta> | <resultado concreto> |

## Fora do escopo
- <o que a feature explicitamente não faz>

## Decisões
- <decisão tomada na entrevista, com o motivo quando houver>
```

### Regras de conteúdo

- **Sem tecnologia e sem código.** Não cite linguagens, frameworks, bancos, APIs, formatos de arquivo técnicos, nomes de classes ou funções, nem trechos de código. Descreva comportamento observável.
- **Regras numeradas** (R1, R2, R3...) e cada uma deve ser verificável: alguém consegue dizer se foi cumprida ou não.
- **Todo caso de erro aparece em Regras e em Exemplos.** Entrada inválida, valor fora do limite, operação impossível, estado vazio: cada um precisa de uma regra `R<n>` que diga o comportamento esperado e de pelo menos uma linha na tabela de Exemplos com a entrada e a mensagem/resultado de erro.
- **Exemplos** são concretos: valores reais, não descrições genéricas. Inclua casos normais e de borda.
- **Fora do escopo** lista o que foi excluído de propósito, para evitar expansão.
- **Decisões** registra as respostas do usuário que fixaram comportamento e as respostas sugeridas que ele aceitou. Se algo ficou em aberto, escreva como "Em aberto: ..." em vez de inventar.

## Etapa 4 — Confirmar

Ao final, diga em uma linha o caminho do arquivo criado e quantas regras e exemplos ele tem. Não repita o conteúdo da spec no chat.
