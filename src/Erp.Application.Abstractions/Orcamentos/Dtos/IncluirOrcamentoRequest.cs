namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class IncluirOrcamentoRequest
{
    public int? ClienteId { get; set; }
    public int? VendedorId { get; set; }
    public string? CodigoProduto { get; set; }
    public decimal? Quantidade { get; set; }
}
