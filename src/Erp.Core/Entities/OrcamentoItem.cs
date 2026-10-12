namespace Erp.Core.Entities;

public class OrcamentoItem
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    public string NumeroItem { get; set; } = "";
    public string CodigoProduto { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal Total { get; set; }
    public bool Cancelado { get; set; }
}
