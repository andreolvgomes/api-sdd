using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPut("api/clientes/{Clie_id}")]
[AllowAnonymous()]
[Tags("Clientes")]
public class AlterarClienteEndpoint : EndpointBase<AlterarClienteRequest>
{
    private readonly IClientesService _clientesService;

    public AlterarClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(AlterarClienteRequest req, CancellationToken ct)
    {
        return await Response(await _clientesService.AlterarAsync(req), ct);
    }
}
