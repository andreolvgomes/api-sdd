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
