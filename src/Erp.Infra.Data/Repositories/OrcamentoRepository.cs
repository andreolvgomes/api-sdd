using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private const string ColunasOrcamento = "id, data, cliente_id as ClienteId, vendedor_id as VendedorId, cancelado as Cancelado, desconto as Desconto";

    private const string ColunasItem = "id, orcamento_id as OrcamentoId, numero_item as NumeroItem, codigo_produto as CodigoProduto, quantidade as Quantidade, preco as Preco, total as Total, cancelado as Cancelado";

    private readonly IDbConnection _conexao;

    public OrcamentoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(Orcamento orcamento)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into orcamento (data, cliente_id, vendedor_id) values (@Data, @ClienteId, @VendedorId) returning id",
            orcamento);
    }

    public async Task<Orcamento?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Orcamento>(
            $"select {ColunasOrcamento} from orcamento where id = @id",
            new { id });
    }

    public async Task<List<Orcamento>> ListarAsync(int? clienteId, int? vendedorId)
    {
        string sql = $"select {ColunasOrcamento} from orcamento where 1 = 1";

        if (clienteId != null)
            sql = sql + " and cliente_id = @clienteId";

        if (vendedorId != null)
            sql = sql + " and vendedor_id = @vendedorId";

        sql = sql + " order by id";

        IEnumerable<Orcamento> orcamentos = await _conexao.QueryAsync<Orcamento>(sql, new { clienteId, vendedorId });

        return orcamentos.ToList();
    }

    public async Task AtualizarAsync(Orcamento orcamento)
    {
        await _conexao.ExecuteAsync(
            "update orcamento set cliente_id = @ClienteId, vendedor_id = @VendedorId where id = @Id",
            orcamento);
    }

    public async Task DefinirDescontoAsync(int id, decimal desconto)
    {
        await _conexao.ExecuteAsync("update orcamento set desconto = @desconto where id = @id", new { id, desconto });
    }

    public async Task CancelarAsync(int id)
    {
        await _conexao.ExecuteAsync("update orcamento set cancelado = 1 where id = @id", new { id });
    }

    public async Task<string> InserirItemAsync(OrcamentoItem item)
    {
        return await _conexao.ExecuteScalarAsync<string>(
            @"insert into orcamento_item (orcamento_id, numero_item, codigo_produto, quantidade, preco, total)
              values (
                @OrcamentoId,
                (select printf('%03d', coalesce(max(cast(numero_item as integer)), 0) + 1) from orcamento_item where orcamento_id = @OrcamentoId),
                @CodigoProduto, @Quantidade, @Preco, @Total)
              returning numero_item",
            item);
    }

    public async Task<OrcamentoItem?> ObterItemAsync(int orcamentoId, string numeroItem)
    {
        return await _conexao.QueryFirstOrDefaultAsync<OrcamentoItem>(
            $"select {ColunasItem} from orcamento_item where orcamento_id = @orcamentoId and numero_item = @numeroItem",
            new { orcamentoId, numeroItem });
    }

    public async Task AtualizarItemAsync(OrcamentoItem item)
    {
        await _conexao.ExecuteAsync(
            "update orcamento_item set codigo_produto = @CodigoProduto, quantidade = @Quantidade, preco = @Preco, total = @Total where orcamento_id = @OrcamentoId and numero_item = @NumeroItem",
            item);
    }

    public async Task DefinirCanceladoItemAsync(int orcamentoId, string numeroItem, bool cancelado)
    {
        await _conexao.ExecuteAsync(
            "update orcamento_item set cancelado = @cancelado where orcamento_id = @orcamentoId and numero_item = @numeroItem",
            new { orcamentoId, numeroItem, cancelado });
    }

    public async Task<List<OrcamentoItem>> ListarItensAsync(int orcamentoId)
    {
        IEnumerable<OrcamentoItem> itens = await _conexao.QueryAsync<OrcamentoItem>(
            $"select {ColunasItem} from orcamento_item where orcamento_id = @orcamentoId order by numero_item",
            new { orcamentoId });

        return itens.ToList();
    }

    public async Task<int> ContarPagamentosNaoCanceladosAsync(int orcamentoId)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from orcamento_pagamento where orcamento_id = @orcamentoId and cancelado = 0",
            new { orcamentoId });
    }

    public async Task<List<OrcamentoPagamento>> ListarPagamentosAsync(int orcamentoId)
    {
        IEnumerable<OrcamentoPagamento> pagamentos = await _conexao.QueryAsync<OrcamentoPagamento>(
            "select id, orcamento_id as OrcamentoId, numero_pagamento as NumeroPagamento, forma_pagamento_id as FormaPagamentoId, caixa_banco_id as CaixaBancoId, valor as Valor, cancelado as Cancelado from orcamento_pagamento where orcamento_id = @orcamentoId order by numero_pagamento",
            new { orcamentoId });

        return pagamentos.ToList();
    }

    public async Task<OrcamentoPagamento?> ObterPagamentoAsync(int orcamentoId, string numeroPagamento)
    {
        return await _conexao.QueryFirstOrDefaultAsync<OrcamentoPagamento>(
            "select id, orcamento_id as OrcamentoId, numero_pagamento as NumeroPagamento, forma_pagamento_id as FormaPagamentoId, caixa_banco_id as CaixaBancoId, valor as Valor, cancelado as Cancelado from orcamento_pagamento where orcamento_id = @orcamentoId and numero_pagamento = @numeroPagamento",
            new { orcamentoId, numeroPagamento });
    }

    public async Task<bool> InserirPagamentoSeCouberAsync(OrcamentoPagamento pagamento, decimal total)
    {
        await _conexao.ExecuteAsync("begin immediate");

        try
        {
            IEnumerable<decimal> valores = await _conexao.QueryAsync<decimal>(
                "select valor from orcamento_pagamento where orcamento_id = @orcamentoId and cancelado = 0",
                new { orcamentoId = pagamento.OrcamentoId });

            decimal pago = valores.Sum();

            if (pago + pagamento.Valor > total)
            {
                await _conexao.ExecuteAsync("rollback");
                return false;
            }

            await _conexao.ExecuteAsync(
                @"insert into orcamento_pagamento (orcamento_id, numero_pagamento, forma_pagamento_id, caixa_banco_id, valor, cancelado)
                  values (
                    @OrcamentoId,
                    (select printf('%03d', coalesce(max(cast(numero_pagamento as integer)), 0) + 1) from orcamento_pagamento where orcamento_id = @OrcamentoId),
                    @FormaPagamentoId, @CaixaBancoId, @Valor, 0)",
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
            "update orcamento_pagamento set cancelado = 1 where orcamento_id = @orcamentoId and numero_pagamento = @numeroPagamento",
            new { orcamentoId, numeroPagamento });
    }
}
