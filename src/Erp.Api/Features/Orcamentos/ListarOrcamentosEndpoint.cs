using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpGet("api/orcamentos")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class ListarOrcamentosEndpoint : EndpointBase<ListarOrcamentosRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public ListarOrcamentosEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(ListarOrcamentosRequest req, CancellationToken ct)
    {
        return await Response(await _orcamentosService.ListarAsync(req), ct);
    }
}
