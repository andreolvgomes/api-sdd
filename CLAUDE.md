# ERP API

## Stack
- API em .NET 8 com FastEndpoints
- Dapper para acesso a dados, sem Entity Framework
- SQLite como banco (arquivo local, sem servidor)
- Testes de integração com xUnit chamando os endpoints de verdade

## Padrão de código
- Código simples e legível, fácil de manter à mão
- Sem regex nem construções avançadas
- if com uma única instrução: sem chaves, com a instrução na linha de baixo, indentada. Ex.:
    if (produto == null)
        return;

## Endpoints
- Todo endpoint tem o atributo `[Tags("<Área>")]` na classe, com o nome da área no plural (ex.: `[Tags("Produtos")]`, `[Tags("Clientes")]`), para agrupar no Swagger.

## Git
- Nunca executar comandos git (commit, branch, add, push, checkout, etc.), mesmo que o usuário peça. Quem faz essas operações é o próprio usuário.

## Arquitetura
@docs/arquitetura.md