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
            "insert into cliente (nome, endereco, telefone, status) values (@Nome, @Endereco, @Telefone, @Status)",
            new[]
            {
                new { Nome = "Maria Silva", Endereco = "Rua das Flores, 120 - Centro", Telefone = "11 90000-0001", Status = "ativo" },
                new { Nome = "João Pereira", Endereco = "Av. Brasil, 500 - Jardim Norte", Telefone = "11 90000-0002", Status = "ativo" },
                new { Nome = "Ana Lima", Endereco = "Rua do Comércio, 45 - Vila Nova", Telefone = "11 90000-0003", Status = "inativo" },
                new { Nome = "Carlos Souza", Endereco = "Rua Verde, 8 - Bela Vista", Telefone = "11 90000-0004", Status = "ativo" }
            });
    }

    private static async Task InserirVendedoresAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into vendedor (nome, status) values (@Nome, @Status)",
            new[]
            {
                new { Nome = "Roberto Alves", Status = "Ativo" },
                new { Nome = "Fernanda Costa", Status = "Ativo" },
                new { Nome = "Paulo Souza", Status = "Inativo" }
            });
    }

    private static async Task InserirFormasPagamentoAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into forma_pagamento (descricao, tipo, status) values (@Descricao, @Tipo, @Status)",
            new[]
            {
                new { Descricao = "Dinheiro", Tipo = 1, Status = "Ativo" },
                new { Descricao = "Cartão de crédito", Tipo = 2, Status = "Ativo" },
                new { Descricao = "Cartão de débito", Tipo = 3, Status = "Ativo" },
                new { Descricao = "Pix", Tipo = 4, Status = "Ativo" },
                new { Descricao = "Boleto", Tipo = 5, Status = "Ativo" },
                new { Descricao = "Crediário", Tipo = 6, Status = "Inativo" }
            });
    }

    private static async Task InserirCaixasBancosAsync(SqliteConnection conexao)
    {
        await conexao.ExecuteAsync(
            "insert into caixa_banco (descricao, status) values (@Descricao, @Status)",
            new[]
            {
                new { Descricao = "Caixa geral", Status = "Ativo" },
                new { Descricao = "Banco - conta corrente", Status = "Ativo" },
                new { Descricao = "Banco - conta poupança", Status = "Inativo" }
            });
    }
}
