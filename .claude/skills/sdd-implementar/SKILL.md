---
name: sdd-implementar
description: Implementa a spec que está em specs/em-andamento/. Ao concluir, move a pasta da spec para specs/.
argument-hint: <número da spec, opcional; ex.: 004>
disable-model-invocation: true
---

1. Procure a spec em `specs/em-andamento/`. Se $ARGUMENTS for informado, use a pasta que começa com $ARGUMENTS. Se não houver nenhuma pasta em `specs/em-andamento/`, avise e pare sem fazer nada. Se houver mais de uma pasta, ou se nenhuma bater com $ARGUMENTS, pare e avise. Leia o spec.md dela e o CLAUDE.md.
2. Se a spec não responder algo ou se contradizer, pare e pergunte. Nunca decida uma regra sozinho.
3. Implemente uma regra por vez (R1, R2...), com teste. Rode build e testes antes de seguir.
4. Cubra todos os Exemplos da spec com testes. Todo teste começa com o número da spec e da regra (ex.: S004_R2_DesativarCliente_...).
5. Ao final, rode a API e chame os endpoints com pelo menos um Exemplo da spec, mostrando a requisição e a resposta.
6. Ao concluir todas as tarefas, mova a pasta inteira da spec de `specs/em-andamento/` para `specs/`. Se a implementação for interrompida, a pasta permanece em `specs/em-andamento/`.
7. Só mova a pasta da spec de `specs/em-andamento/` para `specs/` se todas as regras estiverem implementadas, todos os testes passando e não houver pendências. Caso contrário, a pasta permanece em `specs/em-andamento/`.
8. Informe: regras implementadas, testes, decisões tomadas e pendências.

Não implemente nada que esteja em "Fora do escopo".
