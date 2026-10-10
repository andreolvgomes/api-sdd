namespace Erp.Application.Abstractions.Produtos;

public class ProdutoResponse
{
    public string Codigo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public decimal PrecoVenda { get; set; }
    public string Status { get; set; } = "";
}
