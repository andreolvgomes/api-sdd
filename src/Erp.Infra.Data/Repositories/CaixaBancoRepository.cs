using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class CaixaBancoRepository : ICaixaBancoRepository
{
    private readonly IDbConnection _conexao;

    public CaixaBancoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from caixa_banco where descricao = @descricao collate nocase and id <> @idIgnorado",
            new { descricao, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(CaixaBanco caixaBanco)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into caixa_banco (descricao, status) values (@Descricao, @Status) returning id",
            caixaBanco);
    }

    public async Task<CaixaBanco?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<CaixaBanco>(
            "select id, descricao, status from caixa_banco where id = @id", new { id });
    }

    public async Task<List<CaixaBanco>> ListarAsync(string? status)
    {
        string sql = "select id, descricao, status from caixa_banco";

        if (status != null)
            sql = sql + " where status = @status";

        sql = sql + " order by id";

        IEnumerable<CaixaBanco> caixasBancos = await _conexao.QueryAsync<CaixaBanco>(sql, new { status });

        return caixasBancos.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from caixa_banco where id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(CaixaBanco caixaBanco)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update caixa_banco set descricao = @Descricao, status = @Status where id = @Id",
            caixaBanco);

        return linhas > 0;
    }
}
