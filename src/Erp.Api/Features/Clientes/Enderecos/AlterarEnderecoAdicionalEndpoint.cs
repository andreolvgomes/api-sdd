using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes.Enderecos;

[HttpPut("api/clientes/{Clie_id}/enderecos/{Ende_id}")]
[AllowAnonymous()]
[Tags("Clientes")]
public class AlterarEnderecoAdicionalEndpoint : EndpointBase<AlterarEnderecoAdicionalRequest>
{
    private readonly IClientesService _clientesService;

    public AlterarEnderecoAdicionalEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(AlterarEnderecoAdicionalRequest req, CancellationToken ct)
    {
        return await Response(await _clientesService.AlterarEnderecoAdicionalAsync(req), ct);
    }
}
