using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IOrcamentoRepository
{
    Task<int> InserirAsync(Orcamento orcamento);

    Task<Orcamento?> ObterPorIdAsync(int id);

    Task<List<Orcamento>> ListarAsync(int? clienteId, int? vendedorId);

    Task InserirItemAsync(OrcamentoItem item);

    Task<List<OrcamentoItem>> ListarItensAsync(int orcamentoId);
}
