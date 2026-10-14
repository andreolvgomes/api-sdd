using Erp.Api.Common;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.CaixasBancos;

[HttpDelete("api/caixas-bancos/{Caix_id}")]
[AllowAnonymous()]
[Tags("Caixas/Bancos")]
public class ExcluirCaixaBancoEndpoint : EndpointBase<ExcluirCaixaBancoRequest>
{
    private readonly ICaixasBancosService _caixasBancosService;

    public ExcluirCaixaBancoEndpoint(ICaixasBancosService caixasBancosService)
    {
        _caixasBancosService = caixasBancosService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirCaixaBancoRequest req, CancellationToken ct)
    {
        await _caixasBancosService.ExcluirAsync(req);

        return await NoContent(ct: ct);
    }
}
