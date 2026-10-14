using Erp.Api.Common;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.CaixasBancos;

[HttpGet("api/caixas-bancos/{Caix_id}")]
[AllowAnonymous()]
[Tags("Caixas/Bancos")]
public class ObterCaixaBancoEndpoint : EndpointBase<ObterCaixaBancoRequest>
{
    private readonly ICaixasBancosService _caixasBancosService;

    public ObterCaixaBancoEndpoint(ICaixasBancosService caixasBancosService)
    {
        _caixasBancosService = caixasBancosService;
    }

    public override async Task<object?> ExecuteAsync(ObterCaixaBancoRequest req, CancellationToken ct)
    {
        return await Ok(await _caixasBancosService.ObterAsync(req), ct);
    }
}
