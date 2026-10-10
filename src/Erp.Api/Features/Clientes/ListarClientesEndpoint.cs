using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Core.Entities;

namespace Erp.Api.Features.Clientes;

[HttpGet("api/clientes")]
public class ListarClientesEndpoint : EndpointBase
{
    private readonly IClientesService _clientesService;

    public ListarClientesEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecutarAsync(CancellationToken ct)
    {
        List<Cliente> clientes = await _clientesService.ListarAsync();

        return await Ok(clientes, ct);
    }
}
