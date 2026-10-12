namespace Erp.Application.Abstractions.Clientes.Dtos;

public class ClienteDetalheResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Status { get; set; } = "";
    public List<EnderecoAdicionalResponse> EnderecosAdicionais { get; set; } = new();
}
