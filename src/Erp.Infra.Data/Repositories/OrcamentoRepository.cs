using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
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
            "select id, data, cliente_id as ClienteId, vendedor_id as VendedorId from orcamento where id = @id",
            new { id });
    }

    public async Task<List<Orcamento>> ListarAsync(int? clienteId, int? vendedorId)
    {
        string sql = "select id, data, cliente_id as ClienteId, vendedor_id as VendedorId from orcamento where 1 = 1";

        if (clienteId != null)
            sql = sql + " and cliente_id = @clienteId";

        if (vendedorId != null)
            sql = sql + " and vendedor_id = @vendedorId";

        sql = sql + " order by id";

        IEnumerable<Orcamento> orcamentos = await _conexao.QueryAsync<Orcamento>(sql, new { clienteId, vendedorId });

        return orcamentos.ToList();
    }

    public async Task InserirItemAsync(OrcamentoItem item)
    {
        await _conexao.ExecuteAsync(
            "insert into orcamento_item (orcamento_id, codigo_produto, quantidade, preco, total) values (@OrcamentoId, @CodigoProduto, @Quantidade, @Preco, @Total)",
            item);
    }

    public async Task<List<OrcamentoItem>> ListarItensAsync(int orcamentoId)
    {
        IEnumerable<OrcamentoItem> itens = await _conexao.QueryAsync<OrcamentoItem>(
            "select id, orcamento_id as OrcamentoId, codigo_produto as CodigoProduto, quantidade as Quantidade, preco as Preco, total as Total from orcamento_item where orcamento_id = @orcamentoId order by id",
            new { orcamentoId });

        return itens.ToList();
    }
}
