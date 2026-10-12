namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class OrcamentoResponse
{
    public int Id { get; set; }
    public string Data { get; set; } = "";
    public bool Cancelado { get; set; }
    public OrcamentoClienteResponse Cliente { get; set; } = new();
    public OrcamentoVendedorResponse Vendedor { get; set; } = new();
    public List<ItemOrcamentoResponse> Itens { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public List<PagamentoResponse> Pagamentos { get; set; } = new();
    public decimal Saldo { get; set; }
}
