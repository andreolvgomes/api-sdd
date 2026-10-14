namespace Erp.Application.Abstractions.Orcamentos.Dtos;

public class CancelarPagamentoRequest
{
    public int Orca_id { get; set; }
    public string Orcp_numero_pagamento { get; set; } = "";
}
