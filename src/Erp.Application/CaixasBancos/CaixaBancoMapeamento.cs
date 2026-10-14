using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.CaixasBancos;

public static class CaixaBancoMapeamento
{
    public static CaixaBancoResponse ParaResposta(CaixaBanco caixaBanco)
    {
        return new CaixaBancoResponse
        {
            Caix_id = caixaBanco.Caix_id,
            Caix_descricao = caixaBanco.Caix_descricao,
            Caix_status = caixaBanco.Caix_status
        };
    }
}
