namespace Erp.Core.Entities;

public class OrcamentoPagamento
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    public string NumeroPagamento { get; set; } = "";
    public int FormaPagamentoId { get; set; }
    public int CaixaBancoId { get; set; }
    public decimal Valor { get; set; }
    public bool Cancelado { get; set; }
}
