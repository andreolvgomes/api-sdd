using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IProdutoRepository
{
    Task<bool> ExisteCodigoAsync(string codigo);

    Task InserirAsync(Produto produto);

    Task<Produto?> ObterPorCodigoAsync(string codigo);

    Task<List<Produto>> ListarAsync(string? status);

    Task<bool> AtualizarAsync(Produto produto);

    Task<bool> ExcluirAsync(string codigo);
}
