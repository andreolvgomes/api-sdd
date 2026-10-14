namespace Erp.Application.Abstractions.Clientes.Dtos;

public class IncluirClienteRequest
{
    public string? Clie_nome { get; set; }
    public string? Clie_endereco { get; set; }
    public string? Clie_telefone { get; set; }
    public string? Clie_status { get; set; }
}
