using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Core.Entities;

namespace Erp.Api.Features.Produtos;

[HttpGet("api/produtos/{Codigo}")]
public class ObterProdutoEndpoint : EndpointBase<ObterProdutoRequest, Produto>
{
    private readonly IProdutosService _produtosService;

    public ObterProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecutarAsync(ObterProdutoRequest req, CancellationToken ct)
    {
        Produto produto = await _produtosService.ObterAsync(req);

        return await Ok(produto, ct);
    }
}
