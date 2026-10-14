namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class PagamentoResponse
{
    public string Orcp_numero_pagamento { get; set; } = "";
    public int Form_id { get; set; }
    public int Caix_id { get; set; }
    public decimal Orcp_valor { get; set; }
    public bool Orcp_cancelado { get; set; }
}
