namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class AlterarItemOrcamentoRequest
{
    public int Id { get; set; }
    public string NumeroItem { get; set; } = "";
    public string? CodigoProduto { get; set; }
    public decimal? Quantidade { get; set; }
}
