using Erp.Application.Abstractions.Vendedores.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Vendedores;

public static class VendedorMapeamento
{
    public static VendedorResponse ParaResposta(Vendedor vendedor)
    {
        return new VendedorResponse
        {
            Vend_id = vendedor.Vend_id,
            Vend_nome = vendedor.Vend_nome,
            Vend_status = vendedor.Vend_status
        };
    }
}
