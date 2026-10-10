using Erp.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Erp.Api.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly string _arquivoBanco = Path.Combine(Path.GetTempPath(), $"erp-teste-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Erp", $"Data Source={_arquivoBanco}");
        builder.UseSetting("AbrirSwaggerNoNavegador", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        SqliteConnection.ClearAllPools();
        File.Delete(_arquivoBanco);
    }
}
