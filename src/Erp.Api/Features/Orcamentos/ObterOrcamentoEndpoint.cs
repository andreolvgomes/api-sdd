using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpGet("api/orcamentos/{Orca_id}")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class ObterOrcamentoEndpoint : EndpointBase<ObterOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public ObterOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(ObterOrcamentoRequest req, CancellationToken ct)
    {
        return await Response(await _orcamentosService.ObterAsync(req), ct);
    }
}
