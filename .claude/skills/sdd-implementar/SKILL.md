---
name: sdd-implementar
description: Implementa a spec que está em specs/em-andamento/, seguindo o design.md quando existir. Ao concluir, move a pasta da spec para specs/.
argument-hint: <número da spec, opcional; ex.: 004>
disable-model-invocation: true
---

1. Procure a spec em `specs/em-andamento/`. Se $ARGUMENTS for informado, use a pasta que começa com $ARGUMENTS. Se não houver nenhuma pasta em `specs/em-andamento/`, avise e pare sem fazer nada. Se houver mais de uma pasta, ou se nenhuma bater com $ARGUMENTS, pare e avise. Leia o spec.md dela, o CLAUDE.md e o docs/arquitetura.md.
2. Se houver design.md na pasta da spec:
   - Se o spec.md tiver sido alterado depois do design.md (compare a data de modificação dos dois arquivos), pare e peça para rodar /sdd-design de novo.
   - Siga o design: não crie tabela, coluna, rota ou arquivo que não esteja nele. Se precisar de algo fora do design, pare e pergunte.
   Se não houver design.md, siga o CLAUDE.md e o docs/arquitetura.md para as decisões técnicas.
3. Se a spec não responder algo ou se contradizer, pare e pergunte. Nunca decida uma regra sozinho.
4. Implemente uma regra por vez (R1, R2...), com teste. Rode build e testes antes de seguir.
5. Cubra todos os Exemplos da spec com testes. Todo teste começa com o número da spec e da regra (ex.: S004_R2_DesativarCliente_...).
6. Ao final, verifique o resultado de verdade com pelo menos um Exemplo da spec, como diz a seção Verificação do design.md ou, sem design, o CLAUDE.md (ex.: numa API, chamar os endpoints e mostrar requisição e resposta). Se a verificação depender de olhar uma tela, liste o que o usuário deve conferir.
7. Só mova a pasta da spec de `specs/em-andamento/` para `specs/` se todas as regras estiverem implementadas, todos os testes passando e não houver pendências. Caso contrário, a pasta permanece em `specs/em-andamento/`.
8. Informe: regras implementadas, testes, decisões tomadas, pendências, se seguiu um design.md e se a pasta foi movida.

Não implemente nada que esteja em "Fora do escopo".