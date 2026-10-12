---
name: sdd-refatorar
description: Refatora o código sem mudar comportamento.
argument-hint: <o que refatorar>
disable-model-invocation: true
---

Refatoração pedida: $ARGUMENTS

1. Confira com `git status` se não há alterações pendentes. Se houver, pare e peça para o usuário fazer commit antes.
2. Leia o CLAUDE.md e o docs/arquitetura.md. A refatoração deve deixar o código seguindo esses arquivos.
3. Rode os testes e anote o resultado. Se algum falhar, pare e avise.
4. Mostre o plano: arquivos que vão mudar, o que vai para onde, em no máximo 10 linhas. Pare e espere aprovação.
5. Execute só o plano aprovado. Nenhuma regra de negócio muda.
6. Rode os testes de novo e chame os endpoints com um Exemplo de alguma spec. O resultado deve ser o mesmo do passo 3.
7. Informe:
   - o que mudou;
   - se algum teste precisou ser alterado e por quê (mudança só de `using` ou namespace não conta);
   - o resumo do `git diff --stat`.

Não altere specs. Não acrescente funcionalidade.
