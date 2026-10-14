using Erp.Api.Common;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.FormasPagamento;

[HttpPut("api/formas-pagamento/{Form_id}")]
[AllowAnonymous()]
[Tags("Formas de pagamento")]
public class AlterarFormaPagamentoEndpoint : EndpointBase<AlterarFormaPagamentoRequest>
{
    private readonly IFormasPagamentoService _formasPagamentoService;

    public AlterarFormaPagamentoEndpoint(IFormasPagamentoService formasPagamentoService)
    {
        _formasPagamentoService = formasPagamentoService;
    }

    public override async Task<object?> ExecuteAsync(AlterarFormaPagamentoRequest req, CancellationToken ct)
    {
        return await Ok(await _formasPagamentoService.AlterarAsync(req), ct);
    }
}
