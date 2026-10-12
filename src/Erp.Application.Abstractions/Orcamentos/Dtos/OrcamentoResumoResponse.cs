namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class OrcamentoResumoResponse
{
    public int Id { get; set; }
    public string Data { get; set; } = "";
    public OrcamentoClienteResponse Cliente { get; set; } = new();
    public OrcamentoVendedorResponse Vendedor { get; set; } = new();
}
