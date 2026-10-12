namespace Erp.Application.Abstractions.Vendedores.Dtos;

public class AlterarVendedorRequest
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Status { get; set; }
}
