---
name: sdd-design
description: Define o design técnico da spec em andamento (banco, rotas e arquivos) e grava em design.md.
argument-hint: <número da spec, opcional; ex.: 004>
disable-model-invocation: true
---

1. Procure a spec em specs/em-andamento/. Se $ARGUMENTS for informado, use a pasta que começa com $ARGUMENTS. Se não houver nenhuma pasta, ou se nenhuma bater com $ARGUMENTS, avise e pare. Leia o spec.md, o CLAUDE.md e o docs/arquitetura.md.
2. Se já existir design.md, mostre o que precisa mudar nele por causa da spec atual, em vez de começar do zero.
3. Proponha o design, seguindo o docs/arquitetura.md:
   - Banco: tabelas e colunas novas ou alteradas, com o script SQL.
   - Rotas: método, caminho e o que cada uma recebe e devolve.
   - Arquivos: os que serão criados e os que serão alterados.
   Se a spec não exigir nada disso, diga apenas "Sem mudanças de banco, rotas ou estrutura".
4. Pare e espere aprovação. O usuário pode mudar qualquer nome.
5. Depois de aprovado, grave em design.md na pasta da spec e mostre: "Próximo passo: /sdd-implementar NNN".

Não escreva código.
