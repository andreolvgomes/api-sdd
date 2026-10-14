using Dapper;
using Microsoft.Data.Sqlite;

namespace Erp.Infra.Data;

public static class Seed
{
    public static async Task PopularAsync(string connectionString)
    {
        await using var conexao = new SqliteConnection(connectionString);

        if (await ContarAsync(conexao, "cliente") == 0)
            await InserirClientesAsync(conexao);

        if (await ContarAsync(conexao, "vendedor") == 0)
            await InserirVendedoresAsync(conexao);

        if (await ContarAsync(conexao, "forma_pagamento") == 0)
            await InserirFormasPagamentoAsync(conexao);

        if (await ContarAsync(conexao, "caixa_banco") == 0)
            await InserirCaixasBancosAsync(conexao);
    }

    private static async Task<int> ContarAsync(SqliteConnection conexao, string tabela)
    {
        return await conexao.ExecuteScalarAsync<int>($"select count(1) from {tabela}");
    }

    private static async Task InserirClientesAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into cliente (clie_nome, clie_endereco, clie_telefone, clie_status) values (@Clie_nome, @Clie_endereco, @Clie_telefone, @Clie_status)",
            new[]
            {
                new { Clie_nome = "Maria Silva", Clie_endereco = "Rua das Flores, 120 - Centro", Clie_telefone = "11 90000-0001", Clie_status = "ativo" },
                new { Clie_nome = "João Pereira", Clie_endereco = "Av. Brasil, 500 - Jardim Norte", Clie_telefone = "11 90000-0002", Clie_status = "ativo" },
                new { Clie_nome = "Ana Lima", Clie_endereco = "Rua do Comércio, 45 - Vila Nova", Clie_telefone = "11 90000-0003", Clie_status = "inativo" },
                new { Clie_nome = "Carlos Souza", Clie_endereco = "Rua Verde, 8 - Bela Vista", Clie_telefone = "11 90000-0004", Clie_status = "ativo" }
            });
    }

    private static async Task InserirVendedoresAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into vendedor (vend_nome, vend_status) values (@Vend_nome, @Vend_status)",
            new[]
            {
                new { Vend_nome = "Roberto Alves", Vend_status = "Ativo" },
                new { Vend_nome = "Fernanda Costa", Vend_status = "Ativo" },
                new { Vend_nome = "Paulo Souza", Vend_status = "Inativo" }
            });
    }

    private static async Task InserirFormasPagamentoAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into forma_pagamento (form_descricao, form_tipo, form_status) values (@Form_descricao, @Form_tipo, @Form_status)",
            new[]
            {
                new { Form_descricao = "Dinheiro", Form_tipo = 1, Form_status = "Ativo" },
                new { Form_descricao = "Cartão de crédito", Form_tipo = 2, Form_status = "Ativo" },
                new { Form_descricao = "Cartão de débito", Form_tipo = 3, Form_status = "Ativo" },
                new { Form_descricao = "Pix", Form_tipo = 4, Form_status = "Ativo" },
                new { Form_descricao = "Boleto", Form_tipo = 5, Form_status = "Ativo" },
                new { Form_descricao = "Crediário", Form_tipo = 6, Form_status = "Inativo" }
            });
    }

    private static async Task InserirCaixasBancosAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into caixa_banco (caix_descricao, caix_status) values (@Caix_descricao, @Caix_status)",
            new[]
            {
                new { Caix_descricao = "Caixa geral", Caix_status = "Ativo" },
                new { Caix_descricao = "Banco - conta corrente", Caix_status = "Ativo" },
                new { Caix_descricao = "Banco - conta poupança", Caix_status = "Inativo" }
            });
    }
}