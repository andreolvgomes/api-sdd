using Erp.Application.Abstractions.Produtos;
using Erp.Application.Produtos;
using Erp.Core.Repositories;
using Erp.Infra.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Infra.CrossCutting.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddErp(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IProdutoRepository>(new ProdutoRepository(connectionString));

        services.AddTransient<IProdutosService, ProdutosService>();

        return services;
    }
}
