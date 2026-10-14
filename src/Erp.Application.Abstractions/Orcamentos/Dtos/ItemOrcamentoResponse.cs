namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class ItemOrcamentoResponse
{
    public string Orci_numero_item { get; set; } = "";
    public string Prod_codigo { get; set; } = "";
    public decimal Orci_quantidade { get; set; }
    public decimal Orci_preco { get; set; }
    public decimal Orci_total { get; set; }
    public bool Orci_cancelado { get; set; }
}
