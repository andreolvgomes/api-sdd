---
name: sdd-spec
description: Entrevista o usuário sobre a ideia de uma feature e escreve a especificação em specs/em-andamento/NNN-<nome-curto>/spec.md. Só roda por invocação manual (/sdd-spec).
argument-hint: <ideia da feature>
disable-model-invocation: true
---

# Especificação de feature (SDD)

Ideia recebida: $ARGUMENTS

Siga as etapas em ordem. Não pule etapas e não escreva a spec antes da resposta do usuário.

## Etapa 0 — Entrada vazia

Se a ideia acima estiver vazia, peça ao usuário que descreva a feature em uma ou duas frases e pare.

## Etapa 0.5 — Spec ativa

1. Verifique se existe alguma pasta em `specs/em-andamento/`. Se a pasta `specs/em-andamento/` não existir, crie-a.
2. Se não existir nenhuma pasta dentro de `specs/em-andamento/`, siga para a Etapa 1.
3. Se existir uma pasta (NNN-titulo) em `specs/em-andamento/`:
      - Se o pedido for outra funcionalidade, pare e responda exatamente: "A spec NNN (titulo) ainda não foi concluída. Conclua com /sdd-design e /sdd-implementar antes de criar uma nova spec."
   - Se o pedido for um ajuste ou complemento da spec ativa, siga para a Etapa 1. Na Etapa 3, edite o `spec.md` dessa spec em vez de criar uma nova.
   - Se não estiver claro se é ajuste ou outra funcionalidade, pergunte ao usuário antes de seguir.

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

1. Defina `<nome-curto>`: minúsculo, sem acentos, palavras separadas por hífen, 2 a 4 palavras (ex.: `situacao-clientes`).
2. Defina `NNN` executando, na raiz do projeto:
   `powershell -ExecutionPolicy Bypass -File .claude/skills/sdd-spec/proximo-numero.ps1`
   Use exatamente o número que ele retornar. Não calcule o número por conta própria.
3. Se não houver spec ativa, crie `specs/em-andamento/NNN-<nome-curto>/spec.md` na raiz do projeto. Se houver spec ativa e o pedido for um ajuste ou complemento, edite o `spec.md` dela, sem mudar o número nem o nome da pasta.
4. Use exatamente esta estrutura:

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
- **Specs concluídas não são editadas.** Ficam direto em `specs/`. Mudança em algo já concluído vira uma nova spec, com o próximo número, citando a spec anterior em Decisões (ex.: "Altera a R3 da spec 001").
- **Spec ativa pode ser editada** quando o pedido for um ajuste ou complemento. Registre em Decisões o que mudou e o motivo.
- Se a spec ativa já tiver design.md e o ajuste mudar regras, avise no final: "O design.md precisa ser revisto: rode /sdd-design NNN."

## Etapa 4 — Confirmar
Ao final, mostre exatamente neste formato, sem repetir o conteúdo da spec:

```
Spec: NNN-<nome-curto>
Caminho: <caminho completo até o spec.md>
Regras: <quantidade> | Exemplos: <quantidade>
Próximo passo: /sdd-design NNN
```
