using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos;

[HttpPut("api/orcamentos/{Id}")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class AlterarOrcamentoEndpoint : EndpointBase<AlterarOrcamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public AlterarOrcamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(AlterarOrcamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _orcamentosService.AlterarAsync(req), ct);
    }
}
