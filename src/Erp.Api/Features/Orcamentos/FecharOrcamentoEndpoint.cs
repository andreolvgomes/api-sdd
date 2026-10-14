using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPost("api/orcamentos/{Orca_id}/fechar")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class FecharOrcamentoEndpoint : EndpointBase<FecharOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public FecharOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(FecharOrcamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _orcamentosService.FecharAsync(req), ct);
    }
}
