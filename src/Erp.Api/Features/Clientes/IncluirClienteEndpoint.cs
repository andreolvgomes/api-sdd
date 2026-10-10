using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;

namespace Erp.Api.Features.Clientes;

[HttpPost("api/clientes")]
public class IncluirClienteEndpoint : EndpointBase<IncluirClienteRequest, ClienteResponse>
{
    private readonly IClientesService _clientesService;

    public IncluirClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecutarAsync(IncluirClienteRequest req, CancellationToken ct)
    {
        ClienteResponse cliente = await _clientesService.IncluirAsync(req);

        return await Created(cliente, ct);
    }
}
