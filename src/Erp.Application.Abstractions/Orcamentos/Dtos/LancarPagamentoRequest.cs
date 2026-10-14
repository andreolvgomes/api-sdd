namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class LancarPagamentoRequest
{
    public int Orca_id { get; set; }
    public int? Form_id { get; set; }
    public int? Caix_id { get; set; }
    public decimal? Orcp_valor { get; set; }
}
