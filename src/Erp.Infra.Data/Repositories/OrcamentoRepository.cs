using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private const string ColunasOrcamento = "orca_id, orca_data, clie_id, vend_id, orca_cancelado, orca_desconto";

    private const string ColunasItem = "orci_id, orca_id, orci_numero_item, prod_codigo, orci_quantidade, orci_preco, orci_total, orci_cancelado";

    private const string ColunasPagamento = "orcp_id, orca_id, orcp_numero_pagamento, form_id, caix_id, orcp_valor, orcp_cancelado";

    private readonly IDbConnection _conexao;

    public OrcamentoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(Orcamento orcamento)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into orcamento (orca_data, clie_id, vend_id) values (@Orca_data, @Clie_id, @Vend_id) returning orca_id",
            orcamento);
    }

    public async Task<Orcamento?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Orcamento>(
            $"select {ColunasOrcamento} from orcamento where orca_id = @id",
            new { id });
    }

    public async Task<List<Orcamento>> ListarAsync(int? clienteId, int? vendedorId)
    {
        string sql = $"select {ColunasOrcamento} from orcamento where 1 = 1";

        if (clienteId != null)
            sql = sql + " and clie_id = @clienteId";

        if (vendedorId != null)
            sql = sql + " and vend_id = @vendedorId";

        sql = sql + " order by orca_id";

        IEnumerable<Orcamento> orcamentos = await _conexao.QueryAsync<Orcamento>(sql, new { clienteId, vendedorId });

        return orcamentos.ToList();
    }

    public async Task AtualizarAsync(Orcamento orcamento)
    {
        await _conexao.ExecuteAsync(
            "update orcamento set clie_id = @Clie_id, vend_id = @Vend_id where orca_id = @Orca_id",
            orcamento);
    }

    public async Task DefinirDescontoAsync(int id, decimal desconto)
    {
        await _conexao.ExecuteAsync("update orcamento set orca_desconto = @desconto where orca_id = @id", new { id, desconto });
    }

    public async Task CancelarAsync(int id)
    {
        await _conexao.ExecuteAsync("update orcamento set orca_cancelado = 1 where orca_id = @id", new { id });
    }

    public async Task<string> InserirItemAsync(OrcamentoItem item)
    {
        return await _conexao.ExecuteScalarAsync<string>(
            @"insert into orcamento_item (orca_id, orci_numero_item, prod_codigo, orci_quantidade, orci_preco, orci_total)
              values (
                @Orca_id,
                (select printf('%03d', coalesce(max(cast(orci_numero_item as integer)), 0) + 1) from orcamento_item where orca_id = @Orca_id),
                @Prod_codigo, @Orci_quantidade, @Orci_preco, @Orci_total)
              returning orci_numero_item",
            item);
    }

    public async Task<OrcamentoItem?> ObterItemAsync(int orcamentoId, string numeroItem)
    {
        return await _conexao.QueryFirstOrDefaultAsync<OrcamentoItem>(
            $"select {ColunasItem} from orcamento_item where orca_id = @orcamentoId and orci_numero_item = @numeroItem",
            new { orcamentoId, numeroItem });
    }

    public async Task AtualizarItemAsync(OrcamentoItem item)
    {
        await _conexao.ExecuteAsync(
            "update orcamento_item set prod_codigo = @Prod_codigo, orci_quantidade = @Orci_quantidade, orci_preco = @Orci_preco, orci_total = @Orci_total where orca_id = @Orca_id and orci_numero_item = @Orci_numero_item",
            item);
    }

    public async Task DefinirCanceladoItemAsync(int orcamentoId, string numeroItem, bool cancelado)
    {
        await _conexao.ExecuteAsync(
            "update orcamento_item set orci_cancelado = @cancelado where orca_id = @orcamentoId and orci_numero_item = @numeroItem",
            new { orcamentoId, numeroItem, cancelado });
    }

    public async Task<List<OrcamentoItem>> ListarItensAsync(int orcamentoId)
    {
        IEnumerable<OrcamentoItem> itens = await _conexao.QueryAsync<OrcamentoItem>(
            $"select {ColunasItem} from orcamento_item where orca_id = @orcamentoId order by orci_numero_item",
            new { orcamentoId });

        return itens.ToList();
    }

    public async Task<int> ContarPagamentosNaoCanceladosAsync(int orcamentoId)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from orcamento_pagamento where orca_id = @orcamentoId and orcp_cancelado = 0",
            new { orcamentoId });
    }

    public async Task<List<OrcamentoPagamento>> ListarPagamentosAsync(int orcamentoId)
    {
        IEnumerable<OrcamentoPagamento> pagamentos = await _conexao.QueryAsync<OrcamentoPagamento>(
            $"select {ColunasPagamento} from orcamento_pagamento where orca_id = @orcamentoId order by orcp_numero_pagamento",
            new { orcamentoId });

        return pagamentos.ToList();
    }

    public async Task<OrcamentoPagamento?> ObterPagamentoAsync(int orcamentoId, string numeroPagamento)
    {
        return await _conexao.QueryFirstOrDefaultAsync<OrcamentoPagamento>(
            $"select {ColunasPagamento} from orcamento_pagamento where orca_id = @orcamentoId and orcp_numero_pagamento = @numeroPagamento",
            new { orcamentoId, numeroPagamento });
    }

    public async Task<bool> InserirPagamentoSeCouberAsync(OrcamentoPagamento pagamento, decimal total)
    {
        await _conexao.ExecuteAsync("begin immediate");

        try
        {
            IEnumerable<decimal> valores = await _conexao.QueryAsync<decimal>(
                "select orcp_valor from orcamento_pagamento where orca_id = @orcamentoId and orcp_cancelado = 0",
                new { orcamentoId = pagamento.Orca_id });

            decimal pago = valores.Sum();

            if (pago + pagamento.Orcp_valor > total)
            {
                await _conexao.ExecuteAsync("rollback");
                return false;
            }

            await _conexao.ExecuteAsync(
                @"insert into orcamento_pagamento (orca_id, orcp_numero_pagamento, form_id, caix_id, orcp_valor, orcp_cancelado)
                  values (
                    @Orca_id,
                    (select printf('%03d', coalesce(max(cast(orcp_numero_pagamento as integer)), 0) + 1) from orcamento_pagamento where orca_id = @Orca_id),
                    @Form_id, @Caix_id, @Orcp_valor, 0)",
                pagamento);

            await _conexao.ExecuteAsync("commit");
            return true;
        }
        catch
        {
            await _conexao.ExecuteAsync("rollback");
            throw;
        }
    }

    public async Task CancelarPagamentoAsync(int orcamentoId, string numeroPagamento)
    {
        await _conexao.ExecuteAsync(
            "update orcamento_pagamento set orcp_cancelado = 1 where orca_id = @orcamentoId and orcp_numero_pagamento = @numeroPagamento",
            new { orcamentoId, numeroPagamento });
    }
}