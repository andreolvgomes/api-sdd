using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpGet("api/clientes")]
[AllowAnonymous()]
public class ListarClientesEndpoint : EndpointBase
{
    private readonly IClientesService _clientesService;

    public ListarClientesEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(CancellationToken ct)
    {
        return await Ok(await _clientesService.ListarAsync(), ct);
    }
}
