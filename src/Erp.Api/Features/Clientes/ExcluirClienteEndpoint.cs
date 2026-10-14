using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Clientes;

[HttpDelete("api/clientes/{Clie_id}")]
[Tags("Clientes")]
[AllowAnonymous()]
public class ExcluirClienteEndpoint : EndpointBase<ExcluirClienteRequest>
{
    private readonly IClientesService _clientesService;

    public ExcluirClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirClienteRequest req, CancellationToken ct)
    {
        await _clientesService.ExcluirAsync(req);

        return await NoContent(ct: ct);
    }
}
