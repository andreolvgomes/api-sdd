namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class AlterarItemOrcamentoRequest
{
    public int Orca_id { get; set; }
    public string Orci_numero_item { get; set; } = "";
    public string? Prod_codigo { get; set; }
    public decimal? Orci_quantidade { get; set; }
}
