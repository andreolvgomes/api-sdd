namespace Erp.Application.Abstractions.Clientes.Dtos;

public class AlterarBloqueioClienteRequest
{
    public int Clie_id { get; set; }
    public string Clie_bloqueio { get; set; } = "";
}
