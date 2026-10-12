using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPost("api/clientes")]
[AllowAnonymous()]
[Tags("Clientes")]
public class IncluirClienteEndpoint : EndpointBase<IncluirClienteRequest>
{
    private readonly IClientesService _clientesService;

    public IncluirClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(IncluirClienteRequest req, CancellationToken ct)
    {
        return await Created(await _clientesService.IncluirAsync(req), ct);
    }
}
