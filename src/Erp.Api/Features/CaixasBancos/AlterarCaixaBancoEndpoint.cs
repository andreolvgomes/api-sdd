using Erp.Api.Common;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.CaixasBancos;

[HttpPut("api/caixas-bancos/{Caix_id}")]
[AllowAnonymous()]
[Tags("Caixas/Bancos")]
public class AlterarCaixaBancoEndpoint : EndpointBase<AlterarCaixaBancoRequest>
{
    private readonly ICaixasBancosService _caixasBancosService;

    public AlterarCaixaBancoEndpoint(ICaixasBancosService caixasBancosService)
    {
        _caixasBancosService = caixasBancosService;
    }

    public override async Task<object?> ExecuteAsync(AlterarCaixaBancoRequest req, CancellationToken ct)
    {
        return await Ok(await _caixasBancosService.AlterarAsync(req), ct);
    }
}
