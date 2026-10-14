namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class IncluirOrcamentoRequest
{
    public int? Clie_id { get; set; }
    public int? Vend_id { get; set; }
    public string? Prod_codigo { get; set; }
    public decimal? Orci_quantidade { get; set; }
}
