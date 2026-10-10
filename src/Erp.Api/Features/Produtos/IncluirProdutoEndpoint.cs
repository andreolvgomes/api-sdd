using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;

namespace Erp.Api.Features.Produtos;

[HttpPost("api/produtos")]
public class IncluirProdutoEndpoint : EndpointBase<IncluirProdutoRequest, ProdutoResponse>
{
    private readonly IProdutosService _produtosService;

    public IncluirProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecutarAsync(IncluirProdutoRequest req, CancellationToken ct)
    {
        ProdutoResponse produto = await _produtosService.IncluirAsync(req);

        return await Created(produto, ct);
    }
}
