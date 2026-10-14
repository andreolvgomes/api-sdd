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
}
