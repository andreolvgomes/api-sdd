---
name: sdd-design
description: Define o design técnico da spec em andamento (banco, rotas e arquivos) e grava em design.md.
argument-hint: <número da spec, opcional; ex.: 004>
disable-model-invocation: true
---

1. Procure a spec em specs/em-andamento/. Se $ARGUMENTS for informado, use a pasta que começa com $ARGUMENTS. Se não houver nenhuma pasta, ou se nenhuma bater com $ARGUMENTS, avise e pare. Leia o spec.md, o CLAUDE.md e o docs/arquitetura.md.
2. Se já existir design.md, mostre o que precisa mudar nele por causa da spec atual, em vez de começar do zero.
3. Grave a proposta em design.md, na pasta da spec, usando exatamente o template de .claude/skills/sdd-design/template-design.md. Seção que não se aplica ao projeto recebe "Não se aplica"; nunca remova seções.
4. Mostre exatamente neste formato e pare:

```
   Design: NNN-<nome-curto>
   Caminho: <caminho completo até o design.md>
   Link: file:///<o mesmo caminho completo, com barras />
   Para mudar: diga o que alterar.
   Próximo passo: /sdd-implementar NNN
```

   Para o caminho completo, use o diretório atual do projeto (obtido com `Get-Location`) seguido de `specs\em-andamento\NNN-<nome-curto>\design.md`.

5. Se o usuário pedir mudanças, altere o design.md e mostre o bloco de novo.

Não escreva código.
