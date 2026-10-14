namespace Erp.Core.Entities;

public class OrcamentoPagamento
{
    public int Orcp_id { get; set; }
    public int Orca_id { get; set; }
    public string Orcp_numero_pagamento { get; set; } = "";
    public int Form_id { get; set; }
    public int Caix_id { get; set; }
    public decimal Orcp_valor { get; set; }
    public bool Orcp_cancelado { get; set; }
}
