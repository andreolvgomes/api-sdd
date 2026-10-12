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

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento (
                id integer primary key autoincrement,
                data text not null,
                cliente_id integer not null,
                vendedor_id integer not null,
                cancelado integer not null default 0,
                foreign key (cliente_id) references cliente (id),
                foreign key (vendedor_id) references vendedor (id)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento_item (
                id integer primary key autoincrement,
                orcamento_id integer not null,
                numero_item text not null,
                codigo_produto text not null,
                quantidade text not null,
                preco text not null,
                total text not null,
                cancelado integer not null default 0,
                unique (orcamento_id, numero_item),
                foreign key (orcamento_id) references orcamento (id),
                foreign key (codigo_produto) references produto (codigo)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento_pagamento (
                id integer primary key autoincrement,
                orcamento_id integer not null,
                numero_pagamento text not null,
                forma_pagamento_id integer not null,
                caixa_banco_id integer not null,
                valor text not null,
                cancelado integer not null default 0,
                unique (orcamento_id, numero_pagamento),
                foreign key (orcamento_id) references orcamento (id),
                foreign key (forma_pagamento_id) references forma_pagamento (id),
                foreign key (caixa_banco_id) references caixa_banco (id)
            )");

        IEnumerable<string> colunasDesconto = await conexao.QueryAsync<string>(
            "select name from pragma_table_info('orcamento') where name = 'desconto'");

        if (!colunasDesconto.Any())
            await conexao.ExecuteAsync("alter table orcamento add column desconto text not null default '0'");

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
