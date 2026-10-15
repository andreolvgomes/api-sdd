using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPost("api/clientes/{Clie_id}/desativar")]
[AllowAnonymous()]
[Tags("Clientes")]
public class DesativarClienteEndpoint : EndpointBase<DesativarClienteRequest>
{
    private readonly IClientesService _clientesService;

    public DesativarClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(DesativarClienteRequest req, CancellationToken ct)
    {
        return await NoContent(await _clientesService.DesativarAsync(req), ct);
    }
}
