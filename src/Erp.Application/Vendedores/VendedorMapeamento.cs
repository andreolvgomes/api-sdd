using Erp.Application.Abstractions.Vendedores.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Vendedores;

public static class VendedorMapeamento
{
    public static VendedorResponse ParaResposta(Vendedor vendedor)
    {
        return new VendedorResponse
        {
            Id = vendedor.Id,
            Nome = vendedor.Nome,
            Status = vendedor.Status
        };
    }
}
