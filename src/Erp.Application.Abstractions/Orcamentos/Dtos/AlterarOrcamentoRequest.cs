namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class AlterarOrcamentoRequest
{
    public int Orca_id { get; set; }
    public int? Clie_id { get; set; }
    public int? Vend_id { get; set; }
}
