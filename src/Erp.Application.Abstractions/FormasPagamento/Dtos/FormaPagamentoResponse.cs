namespace Erp.Application.Abstractions.FormasPagamento.Dtos;

public class FormaPagamentoResponse
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public int Tipo { get; set; }
    public string Status { get; set; } = "";
}
