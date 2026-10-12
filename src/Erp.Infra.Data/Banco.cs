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
                status text not null,
                estoque text not null default '0'
            )");

        IEnumerable<string> colunasEstoque = await conexao.QueryAsync<string>(
            "select name from pragma_table_info('produto') where name = 'estoque'");

        if (!colunasEstoque.Any())
            await conexao.ExecuteAsync("alter table produto add column estoque text not null default '0'");

        await conexao.ExecuteAsync(@"
            create table if not exists cliente (
                id integer primary key autoincrement,
                nome text not null,
                endereco text not null,
                telefone text not null,
                status text not null default 'ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists endereco_adicional (
                cliente_id integer not null,
                id integer not null,
                rotulo text not null,
                endereco text not null,
                primary key (cliente_id, id),
                foreign key (cliente_id) references cliente (id)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists vendedor (
                id integer primary key autoincrement,
                nome text not null,
                status text not null default 'Ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists forma_pagamento (
                id integer primary key autoincrement,
                descricao text not null,
                tipo integer not null,
                status text not null default 'Ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists caixa_banco (
                id integer primary key autoincrement,
                descricao text not null,
                status text not null default 'Ativo'
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
