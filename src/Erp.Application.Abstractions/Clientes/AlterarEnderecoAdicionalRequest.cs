namespace Erp.Application.Abstractions.Clientes;

public class AlterarEnderecoAdicionalRequest
{
    public int ClienteId { get; set; }
    public int Id { get; set; }
    public string? Rotulo { get; set; }
    public string? Endereco { get; set; }
}
