using Dapper;
using Microsoft.Data.Sqlite;

namespace Erp.Infra.Data;

public static class Banco
{
    public static void CriarTabelas(string connectionString)
    {
        using var conexao = new SqliteConnection(connectionString);
        conexao.Execute(@"
            create table if not exists produto (
                codigo text primary key,
                descricao text not null,
                precovenda text not null,
                status text not null
            )");
    }
}
