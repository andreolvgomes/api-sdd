using Erp.Api.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Vendedores;

[HttpDelete("api/vendedores/{Vend_id}")]
[AllowAnonymous()]
[Tags("Vendedores")]
public class ExcluirVendedorEndpoint : EndpointBase<ExcluirVendedorRequest>
{
    private readonly IVendedoresService _vendedoresService;

    public ExcluirVendedorEndpoint(IVendedoresService vendedoresService)
    {
        _vendedoresService = vendedoresService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirVendedorRequest req, CancellationToken ct)
    {
        await _vendedoresService.ExcluirAsync(req);

        return await NoContent(ct: ct);
    }
}
