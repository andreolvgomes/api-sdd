using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IOrcamentoRepository
{
    Task<int> InserirAsync(Orcamento orcamento);

    Task<Orcamento?> ObterPorIdAsync(int id);

    Task<List<Orcamento>> ListarAsync(int? clienteId, int? vendedorId);

    Task AtualizarAsync(Orcamento orcamento);

    Task CancelarAsync(int id);

    Task DefinirDescontoAsync(int id, decimal desconto);

    Task AtualizarTotaisAsync(int id, decimal total, decimal totalComDesconto);

    Task<string> InserirItemAsync(OrcamentoItem item);

    Task<OrcamentoItem?> ObterItemAsync(int orcamentoId, string numeroItem);

    Task AtualizarItemAsync(OrcamentoItem item);

    Task DefinirCanceladoItemAsync(int orcamentoId, string numeroItem, bool cancelado);

    Task<List<OrcamentoItem>> ListarItensAsync(int orcamentoId);

    Task<int> ContarPagamentosNaoCanceladosAsync(int orcamentoId);

    Task<List<OrcamentoPagamento>> ListarPagamentosAsync(int orcamentoId);

    Task<OrcamentoPagamento?> ObterPagamentoAsync(int orcamentoId, string numeroPagamento);

    Task<bool> InserirPagamentoSeCouberAsync(OrcamentoPagamento pagamento, decimal total);

    Task CancelarPagamentoAsync(int orcamentoId, string numeroPagamento);
}
