using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos.Itens;

[HttpPost("api/orcamentos/{Orca_id}/itens/{Orci_numero_item}/cancelar")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class CancelarItemOrcamentoEndpoint : EndpointBase<CancelarItemOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public CancelarItemOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(CancelarItemOrcamentoRequest req, CancellationToken ct)
    {
        return await Response(await _orcamentosService.CancelarItemAsync(req), ct);
    }
}
