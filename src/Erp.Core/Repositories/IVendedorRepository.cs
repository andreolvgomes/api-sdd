using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IVendedorRepository
{
    Task<bool> ExisteNomeAsync(string nome, int idIgnorado);

    Task<int> InserirAsync(Vendedor vendedor);

    Task<Vendedor?> ObterPorIdAsync(int id);

    Task<bool> AtualizarAsync(Vendedor vendedor);

    Task<List<Vendedor>> ListarAsync(string? status);

    Task<bool> ExcluirAsync(int id);
}
