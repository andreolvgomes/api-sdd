using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private const string Colunas = "codigo, descricao, precovenda, status, estoque";

    private readonly IDbConnection _conexao;

    public ProdutoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteCodigoAsync(string codigo)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from produto where codigo = @codigo", new { codigo });

        return quantidade > 0;
    }

    public async Task InserirAsync(Produto produto)
    {
        await _conexao.ExecuteAsync(
            "insert into produto (codigo, descricao, precovenda, status, estoque) values (@Codigo, @Descricao, @PrecoVenda, @Status, @Estoque)",
            produto);
    }

    public async Task<Produto?> ObterPorCodigoAsync(string codigo)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Produto>(
            $"select {Colunas} from produto where codigo = @codigo", new { codigo });
    }

    public async Task<List<Produto>> ListarAsync(string? status)
    {
        string sql = $"select {Colunas} from produto";

        if (status != null)
            sql = sql + " where status = @status";

        sql = sql + " order by codigo";

        IEnumerable<Produto> produtos = await _conexao.QueryAsync<Produto>(sql, new { status });

        return produtos.ToList();
    }

    public async Task<bool> AtualizarAsync(Produto produto)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update produto set descricao = @Descricao, precovenda = @PrecoVenda, status = @Status where codigo = @Codigo",
            produto);

        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(string codigo)
    {
        int linhas = await _conexao.ExecuteAsync("delete from produto where codigo = @codigo", new { codigo });

        return linhas > 0;
    }
}
