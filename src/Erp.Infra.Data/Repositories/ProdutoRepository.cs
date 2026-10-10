using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;
using Microsoft.Data.Sqlite;

namespace Erp.Infra.Data.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private const string Colunas = "codigo, descricao, precovenda, status";

    private readonly string _connectionString;

    public ProdutoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool ExisteCodigo(string codigo)
    {
        using var conexao = new SqliteConnection(_connectionString);
        int quantidade = conexao.ExecuteScalar<int>(
            "select count(1) from produto where codigo = @codigo", new { codigo });

        return quantidade > 0;
    }

    public void Inserir(Produto produto)
    {
        using var conexao = new SqliteConnection(_connectionString);
        conexao.Execute(
            "insert into produto (codigo, descricao, precovenda, status) values (@Codigo, @Descricao, @PrecoVenda, @Status)",
            produto);
    }

    public Produto? ObterPorCodigo(string codigo)
    {
        using var conexao = new SqliteConnection(_connectionString);
        return conexao.QueryFirstOrDefault<Produto>(
            $"select {Colunas} from produto where codigo = @codigo", new { codigo });
    }

    public List<Produto> Listar(string? status)
    {
        using var conexao = new SqliteConnection(_connectionString);
        string sql = $"select {Colunas} from produto";

        if (status != null)
            sql = sql + " where status = @status";

        sql = sql + " order by codigo";

        return conexao.Query<Produto>(sql, new { status }).AsList();
    }

    public bool Atualizar(Produto produto)
    {
        using var conexao = new SqliteConnection(_connectionString);
        int linhas = conexao.Execute(
            "update produto set descricao = @Descricao, precovenda = @PrecoVenda, status = @Status where codigo = @Codigo",
            produto);

        return linhas > 0;
    }

    public bool Excluir(string codigo)
    {
        using var conexao = new SqliteConnection(_connectionString);
        int linhas = conexao.Execute("delete from produto where codigo = @codigo", new { codigo });

        return linhas > 0;
    }
}
