using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.FormasPagamento;

public interface IFormasPagamentoService
{
    Task<AppResult<FormaPagamentoResponse>> IncluirAsync(IncluirFormaPagamentoRequest request);

    Task<AppResult<FormaPagamentoResponse>> AlterarAsync(AlterarFormaPagamentoRequest request);

    Task<AppResult<FormaPagamento>> ObterAsync(ObterFormaPagamentoRequest request);

    Task<AppResult<List<FormaPagamento>>> ListarAsync(ListarFormasPagamentoRequest request);

    Task<AppResult<bool>> ExcluirAsync(ExcluirFormaPagamentoRequest request);
}
