namespace Erp.Application.Abstractions.FormasPagamento.Dtos;

public class IncluirFormaPagamentoRequest
{
    public string? Descricao { get; set; }
    public int? Tipo { get; set; }
    public string? Status { get; set; }
}
