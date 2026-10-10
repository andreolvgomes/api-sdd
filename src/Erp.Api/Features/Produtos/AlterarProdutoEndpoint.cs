using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;

namespace Erp.Api.Features.Produtos;

[HttpPut("api/produtos/{Codigo}")]
public class AlterarProdutoEndpoint : EndpointBase<AlterarProdutoRequest, ProdutoResponse>
{
    private readonly IProdutosService _produtosService;

    public AlterarProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecutarAsync(AlterarProdutoRequest req, CancellationToken ct)
    {
        ProdutoResponse produto = await _produtosService.AlterarAsync(req);

        return await Ok(produto, ct);
    }
}
