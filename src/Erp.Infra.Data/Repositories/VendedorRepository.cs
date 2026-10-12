using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class VendedorRepository : IVendedorRepository
{
    private readonly IDbConnection _conexao;

    public VendedorRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteNomeAsync(string nome, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from vendedor where nome = @nome collate nocase and id <> @idIgnorado",
            new { nome, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(Vendedor vendedor)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into vendedor (nome, status) values (@Nome, @Status) returning id",
            vendedor);
    }

    public async Task<Vendedor?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Vendedor>(
            "select id, nome, status from vendedor where id = @id", new { id });
    }

    public async Task<List<Vendedor>> ListarAsync(string? status)
    {
        string sql = "select id, nome, status from vendedor";

        if (status != null)
            sql = sql + " where status = @status";

        sql = sql + " order by id";

        IEnumerable<Vendedor> vendedores = await _conexao.QueryAsync<Vendedor>(sql, new { status });

        return vendedores.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from vendedor where id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(Vendedor vendedor)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update vendedor set nome = @Nome, status = @Status where id = @Id",
            vendedor);

        return linhas > 0;
    }
}
