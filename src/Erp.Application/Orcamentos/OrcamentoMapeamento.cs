using Erp.Application.Abstractions.Orcamentos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Orcamentos;

public static class OrcamentoMapeamento
{
    public static OrcamentoResponse ParaResposta(Orcamento orcamento, Cliente? cliente, Vendedor? vendedor, List<OrcamentoItem> itens, List<OrcamentoPagamento> pagamentos)
    {
        decimal pago = pagamentos.Where(pagamento => !pagamento.Orcp_cancelado).Sum(pagamento => pagamento.Orcp_valor);

        return new OrcamentoResponse
        {
            Orca_id = orcamento.Orca_id,
            Orca_data = orcamento.Orca_data,
            Orca_cancelado = orcamento.Orca_cancelado,
            Cliente = ParaCliente(cliente),
            Vendedor = ParaVendedor(vendedor),
            Itens = itens.Select(ParaItem).ToList(),
            Orca_total = orcamento.Orca_total,
            Orca_desconto = orcamento.Orca_desconto,
            Orca_total_com_desconto = orcamento.Orca_total_com_desconto,
            Pagamentos = pagamentos.Select(ParaPagamento).ToList(),
            Saldo = OrcamentoValidacao.Arredondar(orcamento.Orca_total_com_desconto - pago)
        };
    }

    private static PagamentoResponse ParaPagamento(OrcamentoPagamento pagamento)
    {
        return new PagamentoResponse
        {
            Orcp_numero_pagamento = pagamento.Orcp_numero_pagamento,
            Form_id = pagamento.Form_id,
            Caix_id = pagamento.Caix_id,
            Orcp_valor = pagamento.Orcp_valor,
            Orcp_cancelado = pagamento.Orcp_cancelado
        };
    }

    public static OrcamentoResumoResponse ParaResumo(Orcamento orcamento, Cliente? cliente, Vendedor? vendedor)
    {
        return new OrcamentoResumoResponse
        {
            Orca_id = orcamento.Orca_id,
            Orca_data = orcamento.Orca_data,
            Cliente = ParaCliente(cliente),
            Vendedor = ParaVendedor(vendedor)
        };
    }

    private static ItemOrcamentoResponse ParaItem(OrcamentoItem item)
    {
        return new ItemOrcamentoResponse
        {
            Orci_numero_item = item.Orci_numero_item,
            Prod_codigo = item.Prod_codigo,
            Orci_quantidade = item.Orci_quantidade,
            Orci_preco = item.Orci_preco,
            Orci_total = item.Orci_total,
            Orci_cancelado = item.Orci_cancelado
        };
    }

    private static OrcamentoClienteResponse ParaCliente(Cliente? cliente)
    {
        return new OrcamentoClienteResponse
        {
            Clie_id = cliente?.Clie_id ?? 0,
            Clie_nome = cliente?.Clie_nome ?? ""
        };
    }

    private static OrcamentoVendedorResponse ParaVendedor(Vendedor? vendedor)
    {
        return new OrcamentoVendedorResponse
        {
            Vend_id = vendedor?.Vend_id ?? 0,
            Vend_nome = vendedor?.Vend_nome ?? ""
        };
    }
}
