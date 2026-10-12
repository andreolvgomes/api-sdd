using Erp.Application.Abstractions.Orcamentos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Orcamentos;

public static class OrcamentoMapeamento
{
    public static OrcamentoResponse ParaResposta(Orcamento orcamento, Cliente? cliente, Vendedor? vendedor, List<OrcamentoItem> itens)
    {
        decimal subtotal = itens.Where(item => !item.Cancelado).Sum(item => item.Total);

        return new OrcamentoResponse
        {
            Id = orcamento.Id,
            Data = orcamento.Data,
            Cancelado = orcamento.Cancelado,
            Cliente = ParaCliente(cliente),
            Vendedor = ParaVendedor(vendedor),
            Itens = itens.Select(ParaItem).ToList(),
            Subtotal = subtotal,
            Desconto = orcamento.Desconto,
            Total = OrcamentoValidacao.Arredondar(subtotal - orcamento.Desconto)
        };
    }

    public static OrcamentoResumoResponse ParaResumo(Orcamento orcamento, Cliente? cliente, Vendedor? vendedor)
    {
        return new OrcamentoResumoResponse
        {
            Id = orcamento.Id,
            Data = orcamento.Data,
            Cliente = ParaCliente(cliente),
            Vendedor = ParaVendedor(vendedor)
        };
    }

    private static ItemOrcamentoResponse ParaItem(OrcamentoItem item)
    {
        return new ItemOrcamentoResponse
        {
            NumeroItem = item.NumeroItem,
            CodigoProduto = item.CodigoProduto,
            Quantidade = item.Quantidade,
            Preco = item.Preco,
            Total = item.Total,
            Cancelado = item.Cancelado
        };
    }

    private static OrcamentoClienteResponse ParaCliente(Cliente? cliente)
    {
        return new OrcamentoClienteResponse
        {
            Id = cliente?.Id ?? 0,
            Nome = cliente?.Nome ?? ""
        };
    }

    private static OrcamentoVendedorResponse ParaVendedor(Vendedor? vendedor)
    {
        return new OrcamentoVendedorResponse
        {
            Id = vendedor?.Id ?? 0,
            Nome = vendedor?.Nome ?? ""
        };
    }
}
