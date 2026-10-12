using Erp.Application.Abstractions.FormasPagamento.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.FormasPagamento;

public static class FormaPagamentoMapeamento
{
    public static FormaPagamentoResponse ParaResposta(FormaPagamento formaPagamento)
    {
        return new FormaPagamentoResponse
        {
            Id = formaPagamento.Id,
            Descricao = formaPagamento.Descricao,
            Tipo = formaPagamento.Tipo,
            Status = formaPagamento.Status
        };
    }
}
