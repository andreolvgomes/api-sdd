using System.Data;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Clientes;
using Erp.Application.Produtos;
using Erp.Core.Repositories;
using Erp.Infra.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Infra.CrossCutting.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddErp(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));

        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();

        services.AddTransient<IProdutosService, ProdutosService>();
        services.AddTransient<IClientesService, ClientesService>();

        return services;
    }
}
