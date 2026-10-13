using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes.Enderecos;

[HttpPut("api/clientes/{ClienteId}/enderecos/{Id}")]
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
        return await Ok(await _clientesService.AlterarEnderecoAdicionalAsync(req), ct);
    }
}
