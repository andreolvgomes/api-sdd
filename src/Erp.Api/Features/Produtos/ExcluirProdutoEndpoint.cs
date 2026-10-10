using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpDelete("api/produtos/{Codigo}")]
[AllowAnonymous()]
[Tags("Produtos")]
public class ExcluirProdutoEndpoint : EndpointBase<ExcluirProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public ExcluirProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirProdutoRequest req, CancellationToken ct)
    {
        await _produtosService.ExcluirAsync(req);

        return await NoContent(ct: ct);
    }
}
