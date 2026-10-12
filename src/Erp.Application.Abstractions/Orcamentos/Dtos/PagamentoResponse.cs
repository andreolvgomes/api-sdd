namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class PagamentoResponse
{
    public string NumeroPagamento { get; set; } = "";
    public int FormaPagamentoId { get; set; }
    public int CaixaBancoId { get; set; }
    public decimal Valor { get; set; }
    public bool Cancelado { get; set; }
}
