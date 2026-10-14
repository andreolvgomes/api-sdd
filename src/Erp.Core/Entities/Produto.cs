namespace Erp.Core.Entities;

public class Produto
{
    public string Prod_codigo { get; set; } = "";
    public string Prod_descricao { get; set; } = "";
    public decimal Prod_precovenda { get; set; }
    public string Prod_status { get; set; } = "";
    public decimal Prod_estoque { get; set; }
}
