namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class ItemOrcamentoResponse
{
    public string NumeroItem { get; set; } = "";
    public string CodigoProduto { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal Total { get; set; }
    public bool Cancelado { get; set; }
}
