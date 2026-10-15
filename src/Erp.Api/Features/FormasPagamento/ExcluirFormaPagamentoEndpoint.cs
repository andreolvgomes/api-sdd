using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpDelete("api/formas-pagamento/{Form_id}")]
[AllowAnonymous()]
[Tags("Formas de pagamento")]
public class ExcluirFormaPagamentoEndpoint : EndpointBase<ExcluirFormaPagamentoRequest>
{
    private readonly IFormasPagamentoService _formasPagamentoService;

    public ExcluirFormaPagamentoEndpoint(IFormasPagamentoService formasPagamentoService)
    {
        _formasPagamentoService = formasPagamentoService;
    }

    public override async Task<object?> ExecuteAsync(ExcluirFormaPagamentoRequest req, CancellationToken ct)
    {
        return await NoContent(await _formasPagamentoService.ExcluirAsync(req), ct);
    }
}
