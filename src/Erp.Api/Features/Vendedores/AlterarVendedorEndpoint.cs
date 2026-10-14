using Erp.Api.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Vendedores;

[HttpPut("api/vendedores/{Vend_id}")]
[AllowAnonymous()]
[Tags("Vendedores")]
public class AlterarVendedorEndpoint : EndpointBase<AlterarVendedorRequest>
{
    private readonly IVendedoresService _vendedoresService;

    public AlterarVendedorEndpoint(IVendedoresService vendedoresService)
    {
        _vendedoresService = vendedoresService;
    }

    public override async Task<object?> ExecuteAsync(AlterarVendedorRequest req, CancellationToken ct)
    {
        return await Ok(await _vendedoresService.AlterarAsync(req), ct);
    }
}
