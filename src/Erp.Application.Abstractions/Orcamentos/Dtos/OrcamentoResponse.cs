namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class OrcamentoResponse
{
    public int Id { get; set; }
    public string Data { get; set; } = "";
    public OrcamentoClienteResponse Cliente { get; set; } = new();
    public OrcamentoVendedorResponse Vendedor { get; set; } = new();
    public List<ItemOrcamentoResponse> Itens { get; set; } = new();
    public decimal Subtotal { get; set; }
}
