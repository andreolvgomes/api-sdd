namespace Erp.Core.Entities;

public class EnderecoAdicional
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Rotulo { get; set; } = "";
    public string Endereco { get; set; } = "";
}
