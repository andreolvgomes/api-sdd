namespace Erp.Application.Abstractions.Produtos.Dtos;

public class AlterarProdutoRequest
{
    public string Codigo { get; set; } = "";
    public string? Descricao { get; set; }
    public decimal? PrecoVenda { get; set; }
    public string? Status { get; set; }
    public decimal? Estoque { get; set; }
}
