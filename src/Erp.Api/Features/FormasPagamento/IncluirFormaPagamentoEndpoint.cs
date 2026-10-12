using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpPost("api/formas-pagamento")]
[AllowAnonymous()]
[Tags("Formas de pagamento")]
public class IncluirFormaPagamentoEndpoint : EndpointBase<IncluirFormaPagamentoRequest>
{
    private readonly IFormasPagamentoService _formasPagamentoService;

    public IncluirFormaPagamentoEndpoint(IFormasPagamentoService formasPagamentoService)
    {
        _formasPagamentoService = formasPagamentoService;
    }

    public override async Task<object?> ExecuteAsync(IncluirFormaPagamentoRequest req, CancellationToken ct)
    {
        return await Created(await _formasPagamentoService.IncluirAsync(req), ct);
    }
}
