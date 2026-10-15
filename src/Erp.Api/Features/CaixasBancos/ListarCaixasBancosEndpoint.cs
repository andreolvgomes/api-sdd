using Erp.Api.Common;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.CaixasBancos;

[HttpGet("api/caixas-bancos")]
[AllowAnonymous()]
[Tags("Caixas/Bancos")]
public class ListarCaixasBancosEndpoint : EndpointBase<ListarCaixasBancosRequest>
{
    private readonly ICaixasBancosService _caixasBancosService;

    public ListarCaixasBancosEndpoint(ICaixasBancosService caixasBancosService)
    {
        _caixasBancosService = caixasBancosService;
    }

    public override async Task<object?> ExecuteAsync(ListarCaixasBancosRequest req, CancellationToken ct)
    {
        return await Response(await _caixasBancosService.ListarAsync(req), ct);
    }
}
