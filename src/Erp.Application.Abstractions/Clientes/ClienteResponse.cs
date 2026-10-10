namespace Erp.Application.Abstractions.Clientes;

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Status { get; set; } = "";
}
