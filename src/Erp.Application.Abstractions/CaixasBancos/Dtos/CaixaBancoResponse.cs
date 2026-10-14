namespace Erp.Application.Abstractions.CaixasBancos.Dtos;

public class CaixaBancoResponse
{
    public int Caix_id { get; set; }
    public string Caix_descricao { get; set; } = "";
    public string Caix_status { get; set; } = "";
}
