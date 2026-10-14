using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class CaixaBancoRepository : ICaixaBancoRepository
{
    private const string Colunas = "caix_id, caix_descricao, caix_status";

    private readonly IDbConnection _conexao;

    public CaixaBancoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from caixa_banco where caix_descricao = @descricao collate nocase and caix_id <> @idIgnorado",
            new { descricao, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(CaixaBanco caixaBanco)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into caixa_banco (caix_descricao, caix_status) values (@Caix_descricao, @Caix_status) returning caix_id",
            caixaBanco);
    }

    public async Task<CaixaBanco?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<CaixaBanco>(
            $"select {Colunas} from caixa_banco where caix_id = @id", new { id });
    }

    public async Task<List<CaixaBanco>> ListarAsync(string? status)
    {
        string sql = $"select {Colunas} from caixa_banco";

        if (status != null)
            sql = sql + " where caix_status = @status";

        sql = sql + " order by caix_id";

        IEnumerable<CaixaBanco> caixasBancos = await _conexao.QueryAsync<CaixaBanco>(sql, new { status });

        return caixasBancos.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from caixa_banco where caix_id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(CaixaBanco caixaBanco)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update caixa_banco set caix_descricao = @Caix_descricao, caix_status = @Caix_status where caix_id = @Caix_id",
            caixaBanco);

        return linhas > 0;
    }
}