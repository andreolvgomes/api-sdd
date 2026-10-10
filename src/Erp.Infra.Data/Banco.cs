using Dapper;
using Microsoft.Data.Sqlite;

namespace Erp.Infra.Data;

public static class Banco
{
    public static async Task CriarTabelasAsync(string connectionString)
    {
        await using var conexao = new SqliteConnection(connectionString);
        await conexao.ExecuteAsync(@"
            create table if not exists produto (
                codigo text primary key,
                descricao text not null,
                precovenda text not null,
                status text not null
            )");
    }
}
