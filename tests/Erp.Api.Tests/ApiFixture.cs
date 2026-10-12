using Erp.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Erp.Api.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly string _arquivoBanco = Path.Combine(Path.GetTempPath(), $"erp-teste-{Guid.NewGuid()}.db");

    public string ArquivoBanco => _arquivoBanco;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Erp", $"Data Source={_arquivoBanco};Pooling=False");
        builder.UseSetting("AbrirSwaggerNoNavegador", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ApagarArquivoBanco();
    }

    // O host pode ainda segurar o arquivo por um instante logo após o Dispose; tenta de novo antes de desistir.
    private void ApagarArquivoBanco()
    {
        for (int tentativa = 1; tentativa <= 10; tentativa++)
        {
            try
            {
                File.Delete(_arquivoBanco);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
        }
    }
}
