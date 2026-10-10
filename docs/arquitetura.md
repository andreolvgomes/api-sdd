## Arquitetura

### Projetos e responsabilidades
- Erp.Core: entidades anêmicas (só propriedades, sem métodos nem validação); interfaces de repositórios e providers. Não depende de nenhum outro projeto.
- Erp.Application.Abstractions: Depende só do Core
	- Interfaces de Services *ProdutosService.cs ...
	- Casos de Uso
	- DTOs de entrada e saída
- Erp.Application: implementação dos services e dos casos de uso. Todas as regras de negócio e validações ficam aqui.
- Erp.Infra.Data: implementação dos repositórios com Dapper e SQLite.
- Erp.Infra: integrações externas (e-mail, APIs de terceiros).
- Erp.Infra.CrossCutting: implementação dos providers (data/hora, log, criptografia...).
- Erp.Infra.CrossCutting.IoC: registro de todas as dependências.
- Erp.Api: endpoints FastEndpoints. Só recebem a requisição, chamam o service e devolvem a resposta. Nenhuma regra de negócio aqui.

### Services
- Cada área tem um service (ex.: ProdutosService) com as operações da área: incluir, alterar, excluir, obter e listar. Não criar uma classe por operação.
- Consultas simples da entidade (obter, listar) retornam a entidade direto pelo service.

### Exemplo: onde fica cada parte de Produtos
- Erp.Core/Entities/Produto.cs
- Erp.Core/Repositories/IProdutoRepository.cs
- Erp.Application.Abstractions/Produtos/IProdutosService.cs, IncluirProdutoRequest.cs, AlterarProdutoRequest.cs, ExcluirProdutoRequest.cs, ObterProdutoRequest.cs, ListarProdutosRequest.cs, ProdutoResponse.cs
- Erp.Application/Produtos/ProdutosService.cs, ProdutoValidacao.cs, ProdutoMapeamento.cs
- Erp.Infra.Data/Repositories/ProdutoRepository.cs
- Erp.Infra.CrossCutting.IoC/DependencyInjection.cs (registro de produtos)
- Erp.Api/Features/Produtos/IncluirProdutoEndpoint.cs (e os demais endpoints de produtos)
- Endpoints herdam de Erp.Api/Common/EndpointBase.cs

### Rotas
- /api/produtos, /api/clientes, /api/pedidos
- Os verbos devem decorar as classe Endpoints: [HttpPost("api/produtos")]

## Endpoints
- Todo endpoint herda de EndpointBase (Api/Common) e responde só pelos helpers: Ok, Created, BadRequest, NotFound, NoContent.
- Nunca use Send.* ou HttpContext.Response direto.

## Endpoints
- Todo endpoint herda de EndpointBase (Api/Common) e responde só pelos helpers: Ok, Created, BadRequest, NotFound, NoContent.
- Nunca use Send.* ou HttpContext.Response direto.

Exemplo:
    public override async Task<object?> ExecuteAsync(GetProdutoQuery req, CancellationToken ct)
    {
        var produto = await _handler.ExecuteAsync(req);
        if (produto == null)
            return await NotFound("Produto não encontrado.", ct);

        return await Ok(produto, ct);
    }