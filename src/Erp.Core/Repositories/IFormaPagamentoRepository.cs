using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IFormaPagamentoRepository
{
    Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado);

    Task<int> InserirAsync(FormaPagamento formaPagamento);

    Task<FormaPagamento?> ObterPorIdAsync(int id);

    Task<bool> AtualizarAsync(FormaPagamento formaPagamento);

    Task<List<FormaPagamento>> ListarAsync(string? status, int? tipo);

    Task<bool> ExcluirAsync(int id);
}
