using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Vendedores.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Vendedores;

public interface IVendedoresService
{
    Task<AppResult<VendedorResponse>> IncluirAsync(IncluirVendedorRequest request);

    Task<AppResult<VendedorResponse>> AlterarAsync(AlterarVendedorRequest request);

    Task<AppResult<Vendedor>> ObterAsync(ObterVendedorRequest request);

    Task<AppResult<List<Vendedor>>> ListarAsync(ListarVendedoresRequest request);

    Task<AppResult<bool>> ExcluirAsync(ExcluirVendedorRequest request);
}
