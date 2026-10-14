using Erp.Api.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Orcamentos.Pagamentos;

[HttpPost("api/orcamentos/{Orca_id}/pagamentos/{Orcp_numero_pagamento}/cancelar")]
[AllowAnonymous()]
[Tags("Orçamentos")]
public class CancelarPagamentoEndpoint : EndpointBase<CancelarPagamentoRequest>
{
    private readonly IOrcamentosService _orcamentosService;

    public CancelarPagamentoEndpoint(IOrcamentosService orcamentosService)
    {
        _orcamentosService = orcamentosService;
    }

    public override async Task<object?> ExecuteAsync(CancelarPagamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _orcamentosService.CancelarPagamentoAsync(req), ct);
    }
}
