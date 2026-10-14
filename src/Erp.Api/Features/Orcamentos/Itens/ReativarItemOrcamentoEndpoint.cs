using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos.Itens;

[HttpPost("api/orcamentos/{Orca_id}/itens/{Orci_numero_item}/reativar")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class ReativarItemOrcamentoEndpoint : EndpointBase<ReativarItemOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public ReativarItemOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(ReativarItemOrcamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _orcamentosService.ReativarItemAsync(req), ct);
    }
}
