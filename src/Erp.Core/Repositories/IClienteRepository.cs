using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IClienteRepository
{
    Task<int> InserirAsync(Cliente cliente);

    Task<Cliente?> ObterPorIdAsync(int id);

    Task<List<Cliente>> ListarAsync();

    Task<bool> AtualizarAsync(Cliente cliente);
}
