using Erp.Core.Entities;
using Erp.Application.Abstractions.Produtos.Dtos;

namespace Erp.Application.Abstractions.Produtos;

public interface IProdutosService
{
    Task<ProdutoResponse> IncluirAsync(IncluirProdutoRequest request);

    Task<ProdutoResponse> AlterarAsync(AlterarProdutoRequest request);

    Task ExcluirAsync(ExcluirProdutoRequest request);

    Task DesativarAsync(DesativarProdutoRequest request);

    Task AtivarAsync(AtivarProdutoRequest request);

    Task<Produto> ObterAsync(ObterProdutoRequest request);

    Task<List<Produto>> ListarAsync(ListarProdutosRequest request);
}
