using Erp.Api.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Vendedores;

[HttpGet("/api/vendedores")]
[AllowAnonymous()]
[Tags("Vendedores")]
public class ListarVendedoresEndpoint : EndpointBase<ListarVendedoresRequest>
{
    private readonly IVendedoresService _vendedoresService;

    public ListarVendedoresEndpoint(IVendedoresService vendedoresService)
    {
        _vendedoresService = vendedoresService;
    }

    public override async Task<object?> ExecuteAsync(ListarVendedoresRequest req, CancellationToken ct)
    {
        return await Ok(await _vendedoresService.ListarAsync(req), ct);
    }
}
