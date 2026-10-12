namespace Erp.Application.Abstractions.CaixasBancos.Dtos;

public class AlterarCaixaBancoRequest
{
    public int Id { get; set; }
    public string? Descricao { get; set; }
    public string? Status { get; set; }
}
