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
                prod_codigo text primary key,
                prod_descricao text not null,
                prod_precovenda text not null,
                prod_status text not null,
                prod_estoque text not null default '0'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists cliente (
                clie_id integer primary key autoincrement,
                clie_nome text not null,
                clie_endereco text not null,
                clie_telefone text not null,
                clie_status text not null default 'ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists endereco_adicional (
                clie_id integer not null,
                ende_id integer not null,
                ende_rotulo text not null,
                ende_endereco text not null,
                primary key (clie_id, ende_id),
                foreign key (clie_id) references cliente (clie_id)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists vendedor (
                vend_id integer primary key autoincrement,
                vend_nome text not null,
                vend_status text not null default 'Ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists forma_pagamento (
                form_id integer primary key autoincrement,
                form_descricao text not null,
                form_tipo integer not null,
                form_status text not null default 'Ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists caixa_banco (
                caix_id integer primary key autoincrement,
                caix_descricao text not null,
                caix_status text not null default 'Ativo'
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento (
                orca_id integer primary key autoincrement,
                orca_data text not null,
                clie_id integer not null,
                vend_id integer not null,
                orca_cancelado integer not null default 0,
                orca_desconto text not null default '0',
                orca_total decimal(12,2) not null default 0,
                orca_total_com_desconto decimal(12,2) not null default 0,
                foreign key (clie_id) references cliente (clie_id),
                foreign key (vend_id) references vendedor (vend_id)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento_item (
                orci_id integer primary key autoincrement,
                orca_id integer not null,
                orci_numero_item text not null,
                prod_codigo text not null,
                orci_quantidade text not null,
                orci_preco text not null,
                orci_total text not null,
                orci_cancelado integer not null default 0,
                unique (orca_id, orci_numero_item),
                foreign key (orca_id) references orcamento (orca_id),
                foreign key (prod_codigo) references produto (prod_codigo)
            )");

        await conexao.ExecuteAsync(@"
            create table if not exists orcamento_pagamento (
                orcp_id integer primary key autoincrement,
                orca_id integer not null,
                orcp_numero_pagamento text not null,
                form_id integer not null,
                caix_id integer not null,
                orcp_valor text not null,
                orcp_cancelado integer not null default 0,
                unique (orca_id, orcp_numero_pagamento),
                foreign key (orca_id) references orcamento (orca_id),
                foreign key (form_id) references forma_pagamento (form_id),
                foreign key (caix_id) references caixa_banco (caix_id)
            )");

        IEnumerable<string> colunasTotal = await conexao.QueryAsync<string>(
            "select name from pragma_table_info('orcamento') where name = 'orca_total'");

        if (!colunasTotal.Any())
        {
            await conexao.ExecuteAsync("alter table orcamento add column orca_total decimal(12,2) not null default 0");
            await conexao.ExecuteAsync("alter table orcamento add column orca_total_com_desconto decimal(12,2) not null default 0");
            await RecalcularTotaisAsync(conexao);
        }
    }

    private static async Task RecalcularTotaisAsync(SqliteConnection conexao)
    {
        List<OrcamentoDesconto> orcamentos = (await conexao.QueryAsync<OrcamentoDesconto>(
            "select orca_id, orca_desconto from orcamento")).ToList();

        List<ItemTotal> itens = (await conexao.QueryAsync<ItemTotal>(
            "select orca_id, orci_total, orci_cancelado from orcamento_item")).ToList();

        foreach (OrcamentoDesconto orcamento in orcamentos)
        {
            decimal total = itens
                .Where(item => item.Orca_id == orcamento.Orca_id && item.Orci_cancelado == 0)
                .Sum(item => item.Orci_total);

            decimal totalComDesconto = OrcamentoArredondar(total - orcamento.Orca_desconto);

            await conexao.ExecuteAsync(
                "update orcamento set orca_total = @total, orca_total_com_desconto = @totalComDesconto where orca_id = @Orca_id",
                new { orcamento.Orca_id, total, totalComDesconto });
        }
    }

    private static decimal OrcamentoArredondar(decimal valor)
    {
        return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    private class OrcamentoDesconto
    {
        public int Orca_id { get; set; }
        public decimal Orca_desconto { get; set; }
    }

    private class ItemTotal
    {
        public int Orca_id { get; set; }
        public decimal Orci_total { get; set; }
        public int Orci_cancelado { get; set; }
    }
}