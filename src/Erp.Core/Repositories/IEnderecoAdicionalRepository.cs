using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IEnderecoAdicionalRepository
{
    Task<int> InserirAsync(EnderecoAdicional endereco);

    Task<List<EnderecoAdicional>> ListarPorClienteAsync(int clienteId);

    Task<EnderecoAdicional?> ObterAsync(int clienteId, int id);

    Task<bool> AtualizarAsync(EnderecoAdicional endereco);

    Task<bool> ExcluirAsync(int clienteId, int id);
}
