using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Abstractions.Produtos.Dtos;
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
        return await Response(await _produtosService.ListarAsync(req), ct);
    }
}
