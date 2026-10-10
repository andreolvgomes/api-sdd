using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpGet("api/produtos/{Codigo}")]
[AllowAnonymous()]
public class ObterProdutoEndpoint : EndpointBase<ObterProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public ObterProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(ObterProdutoRequest req, CancellationToken ct)
    {
        return await Ok(await _produtosService.ObterAsync(req), ct);
    }
}
