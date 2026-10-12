namespace Erp.Application.Abstractions.Clientes.Dtos;

public class AlterarEnderecoAdicionalRequest
{
    public int ClienteId { get; set; }
    public int Id { get; set; }
    public string? Rotulo { get; set; }
    public string? Endereco { get; set; }
}
