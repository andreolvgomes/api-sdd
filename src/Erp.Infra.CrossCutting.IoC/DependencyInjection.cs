using System.Data;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Clientes;
using Erp.Application.Produtos;
using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.CaixasBancos;
using Erp.Application.FormasPagamento;
using Erp.Application.Vendedores;
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
        services.AddScoped<IEnderecoAdicionalRepository, EnderecoAdicionalRepository>();
        services.AddScoped<IVendedorRepository, VendedorRepository>();
        services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
        services.AddScoped<ICaixaBancoRepository, CaixaBancoRepository>();

        services.AddTransient<IProdutosService, ProdutosService>();
        services.AddTransient<IClientesService, ClientesService>();
        services.AddTransient<IVendedoresService, VendedoresService>();
        services.AddTransient<IFormasPagamentoService, FormasPagamentoService>();
        services.AddTransient<ICaixasBancosService, CaixasBancosService>();

        return services;
    }
}
