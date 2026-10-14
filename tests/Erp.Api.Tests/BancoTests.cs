using Dapper;
using Erp.Infra.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Erp.Api.Tests;

public class BancoTests
{
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

    [Fact]
    public async Task CriarTabelas_BancoAntigoSemTotais_CalculaTotaisDosOrcamentosExistentes()
    {
        string arquivo = Path.Combine(Path.GetTempPath(), $"erp-migracao-{Guid.NewGuid()}.db");
        string connectionString = $"Data Source={arquivo}";

        try
        {
            using (var conexao = new SqliteConnection(connectionString))
            {
                conexao.Execute(@"
                    create table orcamento (
                        orca_id integer primary key autoincrement,
                        orca_data text not null,
                        clie_id integer not null,
                        vend_id integer not null,
                        orca_cancelado integer not null default 0,
                        orca_desconto text not null default '0'
                    )");

                conexao.Execute(@"
                    create table orcamento_item (
                        orci_id integer primary key autoincrement,
                        orca_id integer not null,
                        orci_numero_item text not null,
                        prod_codigo text not null,
                        orci_quantidade text not null,
                        orci_preco text not null,
                        orci_total text not null,
                        orci_cancelado integer not null default 0
                    )");

                conexao.Execute("insert into orcamento (orca_data, clie_id, vend_id, orca_desconto) values ('2026-10-13', 1, 1, '5.00')");
                conexao.Execute("insert into orcamento_item (orca_id, orci_numero_item, prod_codigo, orci_quantidade, orci_preco, orci_total, orci_cancelado) values (1, '001', 'P001', '1', '10.00', '10.00', 0)");
                conexao.Execute("insert into orcamento_item (orca_id, orci_numero_item, prod_codigo, orci_quantidade, orci_preco, orci_total, orci_cancelado) values (1, '002', 'P002', '1', '3.00', '3.00', 1)");
                conexao.Execute("insert into orcamento_item (orca_id, orci_numero_item, prod_codigo, orci_quantidade, orci_preco, orci_total, orci_cancelado) values (1, '003', 'P003', '1', '2.50', '2.50', 0)");
            }

            await Banco.CriarTabelasAsync(connectionString);

            using var conferencia = new SqliteConnection(connectionString);
            var totais = await conferencia.QuerySingleAsync<TotaisGravados>(
                "select orca_total, orca_total_com_desconto from orcamento where orca_id = 1");

            Assert.Equal(12.50, totais.Orca_total);
            Assert.Equal(7.50, totais.Orca_total_com_desconto);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(arquivo);
        }
    }

    // O SQLite devolve colunas NUMERIC como double.
    private record TotaisGravados(double Orca_total, double Orca_total_com_desconto);
}
