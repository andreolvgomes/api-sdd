using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPost("api/orcamentos/{Orca_id}/cancelar")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class CancelarOrcamentoEndpoint : EndpointBase<CancelarOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public CancelarOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(CancelarOrcamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _orcamentosService.CancelarAsync(req), ct);
    }
}
