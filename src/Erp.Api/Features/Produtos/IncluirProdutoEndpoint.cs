using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpPost("api/produtos")]
[AllowAnonymous()]
public class IncluirProdutoEndpoint : EndpointBase<IncluirProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public IncluirProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(IncluirProdutoRequest req, CancellationToken ct)
    {
        return await Created(await _produtosService.IncluirAsync(req), ct);
    }
}
