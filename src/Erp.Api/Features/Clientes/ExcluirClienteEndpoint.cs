using Erp.Api.Common;
using Erp.Application.Abstractions.Clientes;

namespace Erp.Api.Features.Clientes;

[HttpDelete("api/clientes/{Id}")]
public class ExcluirClienteEndpoint : EndpointBase<ExcluirClienteRequest, FastEndpoints.EmptyResponse>
{
    private readonly IClientesService _clientesService;

    public ExcluirClienteEndpoint(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public override async Task<object?> ExecutarAsync(ExcluirClienteRequest req, CancellationToken ct)
    {
        await _clientesService.ExcluirAsync(req);

        return await NoContent(ct);
    }
}
