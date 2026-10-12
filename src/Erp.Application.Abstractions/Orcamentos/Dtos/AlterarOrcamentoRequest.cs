namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class AlterarOrcamentoRequest
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public int? VendedorId { get; set; }
}
