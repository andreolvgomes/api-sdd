using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos.Itens;

[HttpPut("api/orcamentos/{Orca_id}/itens/{Orci_numero_item}")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class AlterarItemOrcamentoEndpoint : EndpointBase<AlterarItemOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public AlterarItemOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(AlterarItemOrcamentoRequest req, CancellationToken ct)
    {
        return await Response(await _orcamentosService.AlterarItemAsync(req), ct);
    }
}
