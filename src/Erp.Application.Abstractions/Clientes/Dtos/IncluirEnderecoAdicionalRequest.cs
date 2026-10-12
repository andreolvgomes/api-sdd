namespace Erp.Application.Abstractions.Clientes.Dtos;

public class IncluirEnderecoAdicionalRequest
{
    public int ClienteId { get; set; }
    public string? Rotulo { get; set; }
    public string? Endereco { get; set; }
}
