namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class IncluirItemOrcamentoRequest
{
    public int Id { get; set; }
    public string? CodigoProduto { get; set; }
    public decimal? Quantidade { get; set; }
}
