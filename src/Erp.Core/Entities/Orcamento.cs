namespace Erp.Core.Entities;

public class Orcamento
{
    public int Id { get; set; }
    public string Data { get; set; } = "";
    public int ClienteId { get; set; }
    public int VendedorId { get; set; }
}
