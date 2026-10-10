using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;
using Erp.Core.Entities;

namespace Erp.Api.Features.Clientes;

[HttpGet("api/clientes/{Id}")]
public class ObterClienteEndpoint : EndpointBase<ObterClienteRequest, Cliente>
{
    private readonly IClientesService _clientesService;

    public ObterClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecutarAsync(ObterClienteRequest req, CancellationToken ct)
    {
        Cliente cliente = await _clientesService.ObterAsync(req);

        return await Ok(cliente, ct);
    }
}
