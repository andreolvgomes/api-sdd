namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class ItemOrcamentoResponse
{
    public string CodigoProduto { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal Total { get; set; }
}
