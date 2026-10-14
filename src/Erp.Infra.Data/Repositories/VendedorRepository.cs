using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class VendedorRepository : IVendedorRepository
{
    private const string Colunas = "vend_id, vend_nome, vend_status";

    private readonly IDbConnection _conexao;

    public VendedorRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteNomeAsync(string nome, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from vendedor where vend_nome = @nome collate nocase and vend_id <> @idIgnorado",
            new { nome, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(Vendedor vendedor)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into vendedor (vend_nome, vend_status) values (@Vend_nome, @Vend_status) returning vend_id",
            vendedor);
    }

    public async Task<Vendedor?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Vendedor>(
            $"select {Colunas} from vendedor where vend_id = @id", new { id });
    }

    public async Task<List<Vendedor>> ListarAsync(string? status)
    {
        string sql = $"select {Colunas} from vendedor";

        if (status != null)
            sql = sql + " where vend_status = @status";

        sql = sql + " order by vend_id";

        IEnumerable<Vendedor> vendedores = await _conexao.QueryAsync<Vendedor>(sql, new { status });

        return vendedores.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from vendedor where vend_id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(Vendedor vendedor)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update vendedor set vend_nome = @Vend_nome, vend_status = @Vend_status where vend_id = @Vend_id",
            vendedor);

        return linhas > 0;
    }
}