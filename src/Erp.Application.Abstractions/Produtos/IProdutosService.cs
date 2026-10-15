using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Produtos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Produtos;

public interface IProdutosService
{
    Task<AppResult<ProdutoResponse>> IncluirAsync(IncluirProdutoRequest request);

    Task<AppResult<ProdutoResponse>> AlterarAsync(AlterarProdutoRequest request);

    Task<AppResult<bool>> ExcluirAsync(ExcluirProdutoRequest request);

    Task<AppResult<bool>> DesativarAsync(DesativarProdutoRequest request);

    Task<AppResult<bool>> AtivarAsync(AtivarProdutoRequest request);

    Task<AppResult<Produto>> ObterAsync(ObterProdutoRequest request);

    Task<AppResult<List<Produto>>> ListarAsync(ListarProdutosRequest request);
}
