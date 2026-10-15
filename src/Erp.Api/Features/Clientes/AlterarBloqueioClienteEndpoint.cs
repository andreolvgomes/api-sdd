using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpPut("api/clientes/{Clie_id}/bloqueio")]
[AllowAnonymous()]
[Tags("Clientes")]
public class AlterarBloqueioClienteEndpoint : EndpointBase<AlterarBloqueioClienteRequest>
{
    private readonly IClientesService _clientesService;

    public AlterarBloqueioClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(AlterarBloqueioClienteRequest req, CancellationToken ct)
    {
        return await Response(await _clientesService.AlterarBloqueioAsync(req), ct);
    }
}
