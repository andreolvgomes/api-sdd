using System.Diagnostics;
using Erp.Infra.CrossCutting.IoC;
using Erp.Infra.Data;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Erp") ?? "Data Source=erp.db";
await Banco.CriarTabelasAsync(connectionString);

bool abrirSwagger = builder.Configuration.GetValue("AbrirSwaggerNoNavegador", true);

builder.Services.AddErp(connectionString);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();
app.UseFastEndpoints();
app.UseSwaggerGen();

if (abrirSwagger)
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        string enderecoBase = app.Urls.FirstOrDefault(url => url.StartsWith("http://")) ?? app.Urls.First();
        Process.Start(new ProcessStartInfo($"{enderecoBase}/swagger") { UseShellExecute = true });
    });

app.Run();

public partial class Program
{
}
