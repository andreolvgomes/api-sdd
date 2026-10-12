namespace Erp.Application.Abstractions.Clientes;

public class IncluirEnderecoAdicionalRequest
{
    public int ClienteId { get; set; }
    public string? Rotulo { get; set; }
    public string? Endereco { get; set; }
}
