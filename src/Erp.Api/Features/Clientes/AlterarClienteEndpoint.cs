using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPut("api/clientes/{Id}")]
[AllowAnonymous()]
public class AlterarClienteEndpoint : EndpointBase<AlterarClienteRequest>
{
    private readonly IClientesService _clientesService;

    public AlterarClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(AlterarClienteRequest req, CancellationToken ct)
    {
        return await Ok(await _clientesService.AlterarAsync(req), ct);
    }
}
