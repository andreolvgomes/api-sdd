using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.CaixasBancos;

public static class CaixaBancoMapeamento
{
    public static CaixaBancoResponse ParaResposta(CaixaBanco caixaBanco)
    {
        return new CaixaBancoResponse
        {
            Id = caixaBanco.Id,
            Descricao = caixaBanco.Descricao,
            Status = caixaBanco.Status
        };
    }
}
