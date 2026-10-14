namespace Erp.Application.Abstractions.Vendedores.Dtos;

public class AlterarVendedorRequest
{
    public int Vend_id { get; set; }
    public string? Vend_nome { get; set; }
    public string? Vend_status { get; set; }
}
