namespace Erp.Application.Abstractions.FormasPagamento.Dtos;

public class IncluirFormaPagamentoRequest
{
    public string? Form_descricao { get; set; }
    public int? Form_tipo { get; set; }
    public string? Form_status { get; set; }
}
