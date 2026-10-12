using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPost("api/orcamentos/{Id}/pagamentos")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class LancarPagamentoEndpoint : EndpointBase<LancarPagamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public LancarPagamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(LancarPagamentoRequest req, CancellationToken ct)
    {
        return await Created(await _orcamentosService.LancarPagamentoAsync(req), ct);
    }
}
