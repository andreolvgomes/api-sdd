using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;

namespace Erp.Api.Features.Produtos;

[HttpDelete("api/produtos/{Codigo}")]
public class ExcluirProdutoEndpoint : EndpointBase<ExcluirProdutoRequest, FastEndpoints.EmptyResponse>
{
    private readonly IProdutosService _produtosService;

    public ExcluirProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecutarAsync(ExcluirProdutoRequest req, CancellationToken ct)
    {
        await _produtosService.ExcluirAsync(req);

        return await NoContent(ct);
    }
}
