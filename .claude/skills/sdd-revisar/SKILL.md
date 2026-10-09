---
name: sdd-revisar
description: Confere se o código atende todas as regras de specs/<nome>/spec.md.
disable-model-invocation: true
---

Leia specs/$ARGUMENTS/spec.md e o código do projeto.
Você não escreveu este código. Não confie nos testes nem nos comentários: confira no código.

Para cada regra (R1, R2...), responda numa tabela:
| Regra | Atendida? | Onde no código | Teste que cobre | Observação |

Depois rode o programa com cada Exemplo da spec e compare a saída com o resultado esperado.
Não corrija nada. Só aponte o que falta ou está diferente da spec.