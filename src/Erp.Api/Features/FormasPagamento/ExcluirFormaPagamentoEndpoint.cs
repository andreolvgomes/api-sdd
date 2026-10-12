using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpDelete("api/formas-pagamento/{Id}")]
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
        await _formasPagamentoService.ExcluirAsync(req);

        return await NoContent(ct: ct);
    }
}
