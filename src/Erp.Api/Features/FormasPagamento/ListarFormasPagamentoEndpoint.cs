using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpGet("api/formas-pagamento")]
[AllowAnonymous()]
[Tags("Formas de pagamento")]
public class ListarFormasPagamentoEndpoint : EndpointBase<ListarFormasPagamentoRequest>
{
    private readonly IFormasPagamentoService _formasPagamentoService;

    public ListarFormasPagamentoEndpoint(IFormasPagamentoService formasPagamentoService)
    {
        _formasPagamentoService = formasPagamentoService;
    }

    public override async Task<object?> ExecuteAsync(ListarFormasPagamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _formasPagamentoService.ListarAsync(req), ct);
    }
}
