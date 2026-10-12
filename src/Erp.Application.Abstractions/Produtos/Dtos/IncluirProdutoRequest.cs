namespace Erp.Application.Abstractions.Produtos.Dtos;

public class IncluirProdutoRequest
{
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
    public decimal? PrecoVenda { get; set; }
    public string? Status { get; set; }
}
