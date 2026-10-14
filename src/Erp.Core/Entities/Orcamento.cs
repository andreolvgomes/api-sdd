namespace Erp.Core.Entities;

public class Orcamento
{
    public int Orca_id { get; set; }
    public string Orca_data { get; set; } = "";
    public int Clie_id { get; set; }
    public int Vend_id { get; set; }
    public bool Orca_cancelado { get; set; }
    public decimal Orca_desconto { get; set; }
}
