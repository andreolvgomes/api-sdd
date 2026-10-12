namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class CancelarPagamentoRequest
{
    public int Id { get; set; }
    public string NumeroPagamento { get; set; } = "";
}
