using Erp.Application.Abstractions.Vendedores.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Vendedores;

public interface IVendedoresService
{
    Task<VendedorResponse> IncluirAsync(IncluirVendedorRequest request);

    Task<VendedorResponse> AlterarAsync(AlterarVendedorRequest request);

    Task<Vendedor> ObterAsync(ObterVendedorRequest request);

    Task<List<Vendedor>> ListarAsync(ListarVendedoresRequest request);

    Task ExcluirAsync(ExcluirVendedorRequest request);
}
