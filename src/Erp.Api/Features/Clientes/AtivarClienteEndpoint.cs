using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPost("api/clientes/{Id}/ativar")]
[AllowAnonymous()]
[Tags("Clientes")]
public class AtivarClienteEndpoint : EndpointBase<AtivarClienteRequest>
{
    private readonly IClientesService _clientesService;

    public AtivarClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(AtivarClienteRequest req, CancellationToken ct)
    {
        await _clientesService.AtivarAsync(req);

        return await Ok(ct);
    }
}
