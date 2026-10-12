using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPost("api/orcamentos/{Id}/itens")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class IncluirItemOrcamentoEndpoint : EndpointBase<IncluirItemOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public IncluirItemOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(IncluirItemOrcamentoRequest req, CancellationToken ct)
    {
        return await Created(await _orcamentosService.IncluirItemAsync(req), ct);
    }
}
