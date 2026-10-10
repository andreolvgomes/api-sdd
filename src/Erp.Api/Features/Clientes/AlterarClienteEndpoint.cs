using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;

namespace Erp.Api.Features.Clientes;

[HttpPut("api/clientes/{Id}")]
public class AlterarClienteEndpoint : EndpointBase<AlterarClienteRequest, ClienteResponse>
{
    private readonly IClientesService _clientesService;

    public AlterarClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecutarAsync(AlterarClienteRequest req, CancellationToken ct)
    {
        ClienteResponse cliente = await _clientesService.AlterarAsync(req);

        return await Ok(cliente, ct);
    }
}
