using Erp.Api.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Vendedores;

[HttpGet("/api/vendedores/{Id}")]
[AllowAnonymous()]
[Tags("Vendedores")]
public class ObterVendedorEndpoint : EndpointBase<ObterVendedorRequest>
{
    private readonly IVendedoresService _vendedoresService;

    public ObterVendedorEndpoint(IVendedoresService vendedoresService)
    {
        _vendedoresService = vendedoresService;
    }

    public override async Task<object?> ExecuteAsync(ObterVendedorRequest req, CancellationToken ct)
    {
        return await Ok(await _vendedoresService.ObterAsync(req), ct);
    }
}
