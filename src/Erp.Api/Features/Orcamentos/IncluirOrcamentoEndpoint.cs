using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPost("api/orcamentos")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class IncluirOrcamentoEndpoint : EndpointBase<IncluirOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public IncluirOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(IncluirOrcamentoRequest req, CancellationToken ct)
    {
        return await Created(await _orcamentosService.IncluirAsync(req), ct);
    }
}
