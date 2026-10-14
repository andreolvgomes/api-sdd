namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class OrcamentoResponse
{
    public int Orca_id { get; set; }
    public string Orca_data { get; set; } = "";
    public bool Orca_cancelado { get; set; }
    public OrcamentoClienteResponse Cliente { get; set; } = new();
    public OrcamentoVendedorResponse Vendedor { get; set; } = new();
    public List<ItemOrcamentoResponse> Itens { get; set; } = new();
    public decimal Orca_total { get; set; }
    public decimal Orca_desconto { get; set; }
    public decimal Orca_total_com_desconto { get; set; }
    public List<PagamentoResponse> Pagamentos { get; set; } = new();
    public decimal Saldo { get; set; }
}
