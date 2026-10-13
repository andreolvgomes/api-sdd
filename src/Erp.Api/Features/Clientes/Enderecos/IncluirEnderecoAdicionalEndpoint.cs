using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes.Enderecos;

[HttpPost("api/clientes/{ClienteId}/enderecos")]
[AllowAnonymous()]
[Tags("Clientes")]
public class IncluirEnderecoAdicionalEndpoint : EndpointBase<IncluirEnderecoAdicionalRequest>
{
    private readonly IClientesService _clientesService;

    public IncluirEnderecoAdicionalEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(IncluirEnderecoAdicionalRequest req, CancellationToken ct)
    {
        return await Created(await _clientesService.IncluirEnderecoAdicionalAsync(req), ct);
    }
}
