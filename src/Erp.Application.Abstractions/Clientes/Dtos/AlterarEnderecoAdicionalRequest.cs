namespace Erp.Application.Abstractions.Clientes.Dtos;

public class AlterarEnderecoAdicionalRequest
{
    public int Clie_id { get; set; }
    public int Ende_id { get; set; }
    public string? Ende_rotulo { get; set; }
    public string? Ende_endereco { get; set; }
}
