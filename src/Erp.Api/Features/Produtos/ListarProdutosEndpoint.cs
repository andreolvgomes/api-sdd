using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpGet("api/produtos")]
[AllowAnonymous()]
[Tags("Produtos")]
public class ListarProdutosEndpoint : EndpointBase<ListarProdutosRequest>
{
    private readonly IProdutosService _produtosService;

    public ListarProdutosEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(ListarProdutosRequest req, CancellationToken ct)
    {
        return await Ok(await _produtosService.ListarAsync(req), ct);
    }
}
