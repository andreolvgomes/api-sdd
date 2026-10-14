namespace Erp.Core.Entities;

public class OrcamentoItem
{
    public int Orci_id { get; set; }
    public int Orca_id { get; set; }
    public string Orci_numero_item { get; set; } = "";
    public string Prod_codigo { get; set; } = "";
    public decimal Orci_quantidade { get; set; }
    public decimal Orci_preco { get; set; }
    public decimal Orci_total { get; set; }
    public bool Orci_cancelado { get; set; }
}
