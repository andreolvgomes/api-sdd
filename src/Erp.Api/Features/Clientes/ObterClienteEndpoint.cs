using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpGet("api/clientes/{Id}")]
[AllowAnonymous()]
public class ObterClienteEndpoint : EndpointBase<ObterClienteRequest>
{
    private readonly IClientesService _clientesService;

    public ObterClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(ObterClienteRequest req, CancellationToken ct)
    {
        return await Ok(await _clientesService.ObterAsync(req), ct);
    }
}
