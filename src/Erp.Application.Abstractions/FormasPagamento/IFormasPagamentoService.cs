using Erp.Application.Abstractions.FormasPagamento.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.FormasPagamento;

public interface IFormasPagamentoService
{
    Task<FormaPagamentoResponse> IncluirAsync(IncluirFormaPagamentoRequest request);

    Task<FormaPagamentoResponse> AlterarAsync(AlterarFormaPagamentoRequest request);

    Task<FormaPagamento> ObterAsync(ObterFormaPagamentoRequest request);

    Task<List<FormaPagamento>> ListarAsync(ListarFormasPagamentoRequest request);

    Task ExcluirAsync(ExcluirFormaPagamentoRequest request);
}
