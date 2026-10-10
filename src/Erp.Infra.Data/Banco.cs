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

        await conexao.ExecuteAsync(@"
            create table if not exists cliente (
                id integer primary key autoincrement,
                nome text not null,
                endereco text not null,
                telefone text not null,
                status text not null default 'ativo'
            )");

        IEnumerable<string> colunasStatus = await conexao.QueryAsync<string>(
            "select name from pragma_table_info('cliente') where name = 'status'");

        if (colunasStatus.Any())
            return;

        IEnumerable<string> colunasSituacao = await conexao.QueryAsync<string>(
            "select name from pragma_table_info('cliente') where name = 'situacao'");

        if (colunasSituacao.Any())
            await conexao.ExecuteAsync("alter table cliente rename column situacao to status");
        else
            await conexao.ExecuteAsync("alter table cliente add column status text not null default 'ativo'");
    }
}
