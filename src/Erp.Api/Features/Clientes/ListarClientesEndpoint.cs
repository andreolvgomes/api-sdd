using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using Erp.Core.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpGet("api/clientes")]
[AllowAnonymous()]
[Tags("Clientes")]
public class ListarClientesEndpoint : EndpointBase<ListarClientesRequest>
{
    private readonly IClientesService _clientesService;

    public ListarClientesEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(ListarClientesRequest req, CancellationToken ct)
    {
        return await Response(await _clientesService.ListarAsync(req), ct);
    }
}
