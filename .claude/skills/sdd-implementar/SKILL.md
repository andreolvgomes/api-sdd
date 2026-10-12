---
name: sdd-implementar
description: Implementa uma spec de specs/NNN-nome-curto/spec.md.
argument-hint: <número da spec, ex.: 004>
disable-model-invocation: true
---

1. Encontre em specs/ a pasta que começa com $ARGUMENTS e leia o spec.md dela e o CLAUDE.md. Se não houver exatamente uma pasta com esse número, pare e avise.
2. Se a spec não responder algo ou se contradizer, pare e pergunte. Nunca decida uma regra sozinho.
3. Implemente uma regra por vez (R1, R2...), com teste. Rode build e testes antes de seguir.
4. Cubra todos os Exemplos da spec com testes. Todo teste começa com o número da spec e da regra (ex.: S004_R2_DesativarCliente_...).
5. Ao final, rode a API e chame os endpoints com pelo menos um Exemplo da spec, mostrando a requisição e a resposta.
6. Informe: regras implementadas, testes, decisões tomadas e pendências.

Não implemente nada que esteja em "Fora do escopo".