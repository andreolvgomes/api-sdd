namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class OrcamentoResumoResponse
{
    public int Orca_id { get; set; }
    public string Orca_data { get; set; } = "";
    public OrcamentoClienteResponse Cliente { get; set; } = new();
    public OrcamentoVendedorResponse Vendedor { get; set; } = new();
}
