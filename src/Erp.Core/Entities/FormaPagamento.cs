namespace Erp.Core.Entities;

public class FormaPagamento
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public int Tipo { get; set; }
    public string Status { get; set; } = "";
}
