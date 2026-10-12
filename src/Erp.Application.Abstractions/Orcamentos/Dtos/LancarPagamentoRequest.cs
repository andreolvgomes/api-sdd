namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class LancarPagamentoRequest
{
    public int Id { get; set; }
    public int? FormaPagamentoId { get; set; }
    public int? CaixaBancoId { get; set; }
    public decimal? Valor { get; set; }
}
