## Arquitetura

### Projetos e responsabilidades
- Erp.Core: entidades anêmicas (só propriedades, sem métodos nem validação); interfaces de repositórios e providers. Não depende de nenhum outro projeto.
- Erp.Application.Abstractions: depende só do Core.
	- Interfaces dos services (ex.: IProdutosService.cs)
	- Casos de uso
	- DTOs de entrada e saída, na subpasta Dtos de cada área
- Erp.Application: implementação dos services e dos casos de uso. Todas as regras de negócio e validações ficam aqui.
- Erp.Infra.Data: implementação dos repositórios com Dapper e SQLite.
- Erp.Infra: integrações externas (e-mail, APIs de terceiros).
- Erp.Infra.CrossCutting: implementação dos providers (data/hora, log, criptografia...).
- Erp.Infra.CrossCutting.IoC: registro de todas as dependências.
- Erp.Api: endpoints FastEndpoints. Só recebem a requisição, chamam o service e devolvem a resposta. Nenhuma regra de negócio aqui.

### Services
- Cada área tem um service (ex.: ProdutosService) com as operações da área: incluir, alterar, excluir, obter e listar. Não criar uma classe por operação.
- Consultas simples da entidade (obter, listar) retornam a entidade direto pelo service.

### DTOs
- Ficam em Erp.Application.Abstractions/{Area}/Dtos/, nunca no mesmo arquivo de services, endpoints ou repositórios.
- Uma classe por arquivo.
- Só propriedades: sem métodos, sem validação, sem regra.
- Nomes:
	- Entrada: {Operação}{Entidade}Request (ex.: IncluirProdutoRequest, AlterarProdutoRequest, ListarProdutosRequest)
	- Saída: {Entidade}Response (ex.: ProdutoResponse)
- Os endpoints usam os Requests de Abstractions; não criam classes de request próprias.
- A conversão entre entidade e DTO fica em Erp.Application/{Area}/{Entidade}Mapeamento.cs.

Exemplo:

    // Erp.Application.Abstractions/Produtos/Dtos/IncluirProdutoRequest.cs
    namespace Erp.Application.Abstractions.Produtos.Dtos;

    public class IncluirProdutoRequest
    {
        public string Codigo { get; set; } = string.Empty;
    }

    // Erp.Application.Abstractions/Produtos/Dtos/ProdutoResponse.cs
    namespace Erp.Application.Abstractions.Produtos.Dtos;

    public class ProdutoResponse
    {
        public int Id { get; set; }
    }

### Exemplo: onde fica cada parte de Produtos

    Erp.Core/
      Entities/Produto.cs
      Repositories/IProdutoRepository.cs
    Erp.Application.Abstractions/
      Produtos/
        IProdutosService.cs
        Dtos/
          IncluirProdutoRequest.cs
          ProdutoResponse.cs
    Erp.Application/
      Produtos/
        ProdutosService.cs
        ProdutoValidacao.cs
        ProdutoMapeamento.cs
    Erp.Infra.Data/
      Repositories/ProdutoRepository.cs
    Erp.Infra.CrossCutting.IoC/
      DependencyInjection.cs (registro de produtos)
    Erp.Api/
      Common/EndpointBase.cs
      Features/Produtos/
        IncluirProdutoEndpoint.cs
        (e os demais endpoints de produtos)

### Rotas
- /api/produtos, /api/clientes, /api/pedidos
- Os verbos decoram as classes de endpoint, sem barra inicial: [HttpPost("api/produtos")]

## Endpoints
- Todo endpoint herda de EndpointBase (Api/Common) e responde só pelos helpers: Ok, Created, BadRequest, NotFound, NoContent.
- Nunca use Send.* ou HttpContext.Response direto.

Exemplo:

    public override async Task<object?> ExecuteAsync(ObterProdutoRequest req, CancellationToken ct)
    {
        var produto = await _produtosService.ObterAsync(req);
        if (produto == null)
            return await NotFound("Produto não encontrado.", ct);

        return await Ok(produto, ct);
    }