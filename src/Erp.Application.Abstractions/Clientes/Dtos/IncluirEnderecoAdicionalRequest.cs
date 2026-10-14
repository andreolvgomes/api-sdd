namespace Erp.Application.Abstractions.Clientes.Dtos;

public class IncluirEnderecoAdicionalRequest
{
    public int Clie_id { get; set; }
    public string? Ende_rotulo { get; set; }
    public string? Ende_endereco { get; set; }
}
