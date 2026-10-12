using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private const string ColunasOrcamento = "id, data, cliente_id as ClienteId, vendedor_id as VendedorId, cancelado as Cancelado";

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
}
