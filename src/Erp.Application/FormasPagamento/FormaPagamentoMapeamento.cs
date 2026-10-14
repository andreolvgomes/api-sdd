using Erp.Application.Abstractions.FormasPagamento.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.FormasPagamento;

public static class FormaPagamentoMapeamento
{
    public static FormaPagamentoResponse ParaResposta(FormaPagamento formaPagamento)
    {
        return new FormaPagamentoResponse
        {
            Form_id = formaPagamento.Form_id,
            Form_descricao = formaPagamento.Form_descricao,
            Form_tipo = formaPagamento.Form_tipo,
            Form_status = formaPagamento.Form_status
        };
    }
}
