using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpGet("api/formas-pagamento/{Form_id}")]
[AllowAnonymous()]
[Tags("Formas de pagamento")]
public class ObterFormaPagamentoEndpoint : EndpointBase<ObterFormaPagamentoRequest>
{
    private readonly IFormasPagamentoService _formasPagamentoService;

    public ObterFormaPagamentoEndpoint(IFormasPagamentoService formasPagamentoService)
    {
        _formasPagamentoService = formasPagamentoService;
    }

    public override async Task<object?> ExecuteAsync(ObterFormaPagamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _formasPagamentoService.ObterAsync(req), ct);
    }
}
