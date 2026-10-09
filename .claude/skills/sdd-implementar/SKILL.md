---
name: sdd-implementar
description: Implementa uma feature a partir de specs/<nome>/spec.md.
disable-model-invocation: true
---

Feature: $ARGUMENTS

1. Leia specs/<feature>/spec.md e o CLAUDE.md.
2. Se a spec não responder algo ou se contradizer, pare e pergunte. Nunca decida uma regra sozinho.
3. Implemente uma regra por vez (R1, R2...), com teste. Rode build e testes antes de seguir.
4. Cubra todos os Exemplos da spec com testes.
5. No final, informe: regras implementadas, testes, decisões tomadas e pendências.

Não implemente nada que esteja em "Fora do escopo".