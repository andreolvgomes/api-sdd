using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface ICaixaBancoRepository
{
    Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado);

    Task<int> InserirAsync(CaixaBanco caixaBanco);

    Task<CaixaBanco?> ObterPorIdAsync(int id);

    Task<bool> AtualizarAsync(CaixaBanco caixaBanco);

    Task<List<CaixaBanco>> ListarAsync(string? status);

    Task<bool> ExcluirAsync(int id);
}
