namespace Erp.Application.Abstractions.CaixasBancos.Dtos;

public class AlterarCaixaBancoRequest
{
    public int Caix_id { get; set; }
    public string? Caix_descricao { get; set; }
    public string? Caix_status { get; set; }
}
