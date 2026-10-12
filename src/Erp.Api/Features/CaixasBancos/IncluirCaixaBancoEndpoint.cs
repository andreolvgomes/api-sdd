using Erp.Api.Common;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.CaixasBancos;

[HttpPost("api/caixas-bancos")]
[AllowAnonymous()]
[Tags("Caixas/Bancos")]
public class IncluirCaixaBancoEndpoint : EndpointBase<IncluirCaixaBancoRequest>
{
    private readonly ICaixasBancosService _caixasBancosService;

    public IncluirCaixaBancoEndpoint(ICaixasBancosService caixasBancosService)
    {
        _caixasBancosService = caixasBancosService;
    }

    public override async Task<object?> ExecuteAsync(IncluirCaixaBancoRequest req, CancellationToken ct)
    {
        return await Created(await _caixasBancosService.IncluirAsync(req), ct);
    }
}
