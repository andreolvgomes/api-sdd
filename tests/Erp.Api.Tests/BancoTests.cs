using Dapper;
using Erp.Infra.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Erp.Api.Tests;

public class BancoTests
{
    [Fact]
    public async Task CriarTabelas_BancoAntigoSemStatus_AdicionaColunaEClientesExistentesFicamAtivos()
    {
        string arquivo = Path.Combine(Path.GetTempPath(), $"erp-migracao-{Guid.NewGuid()}.db");
        string connectionString = $"Data Source={arquivo}";

        try
        {
            using (var conexao = new SqliteConnection(connectionString))
            {
                conexao.Execute(@"
                    create table cliente (
                        id integer primary key autoincrement,
                        nome text not null,
                        endereco text not null,
                        telefone text not null
                    )");
                conexao.Execute("insert into cliente (nome, endereco, telefone) values ('Maria Silva', 'Rua A, 1', '11 90000-0001')");
            }

            await Banco.CriarTabelasAsync(connectionString);

            using var conferencia = new SqliteConnection(connectionString);
            string status = conferencia.QuerySingle<string>("select status from cliente where id = 1");
            Assert.Equal("ativo", status);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(arquivo);
        }
    }

    [Fact]
    public async Task CriarTabelas_BancoComColunaSituacao_RenomeiaParaStatusEMantemOsValores()
    {
        string arquivo = Path.Combine(Path.GetTempPath(), $"erp-migracao-{Guid.NewGuid()}.db");
        string connectionString = $"Data Source={arquivo}";

        try
        {
            using (var conexao = new SqliteConnection(connectionString))
            {
                conexao.Execute(@"
                    create table cliente (
                        id integer primary key autoincrement,
                        nome text not null,
                        endereco text not null,
                        telefone text not null,
                        situacao text not null default 'ativo'
                    )");
                conexao.Execute("insert into cliente (nome, endereco, telefone, situacao) values ('João Souza', 'Rua B, 2', '21 90000-0002', 'inativo')");
            }

            await Banco.CriarTabelasAsync(connectionString);

            using var conferencia = new SqliteConnection(connectionString);
            string status = conferencia.QuerySingle<string>("select status from cliente where id = 1");
            Assert.Equal("inativo", status);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(arquivo);
        }
    }

    [Fact]
    public async Task CriarTabelas_ExecutadaDuasVezes_NaoFalha()
    {
        string arquivo = Path.Combine(Path.GetTempPath(), $"erp-migracao-{Guid.NewGuid()}.db");
        string connectionString = $"Data Source={arquivo}";

        try
        {
            await Banco.CriarTabelasAsync(connectionString);
            await Banco.CriarTabelasAsync(connectionString);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(arquivo);
        }
    }
}
