using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes.Enderecos;

[HttpDelete("api/clientes/{ClienteId}/enderecos/{Id}")]
[AllowAnonymous()]
[Tags("Clientes")]
public class ExcluirEnderecoAdicionalEndpoint : EndpointBase<ExcluirEnderecoAdicionalRequest>
{
    private readonly IClientesService _clientesService;

    public ExcluirEnderecoAdicionalEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirEnderecoAdicionalRequest req, CancellationToken ct)
    {
        await _clientesService.ExcluirEnderecoAdicionalAsync(req);

        return await NoContent(ct: ct);
    }
}
