using Erp.Api.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Vendedores;

[HttpPost("/api/vendedores")]
[AllowAnonymous()]
[Tags("Vendedores")]
public class IncluirVendedorEndpoint : EndpointBase<IncluirVendedorRequest>
{
    private readonly IVendedoresService _vendedoresService;

    public IncluirVendedorEndpoint(IVendedoresService vendedoresService)
    {
        _vendedoresService = vendedoresService;
    }

    public override async Task<object?> ExecuteAsync(IncluirVendedorRequest req, CancellationToken ct)
    {
        return await Created(await _vendedoresService.IncluirAsync(req), ct);
    }
}
