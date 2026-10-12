namespace Erp.Application.Abstractions.Clientes.Dtos;

public class IncluirClienteRequest
{
    public string? Nome { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Status { get; set; }
}
