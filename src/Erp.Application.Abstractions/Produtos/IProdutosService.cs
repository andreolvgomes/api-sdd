using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Produtos;

public interface IProdutosService
{
    Task<ProdutoResponse> IncluirAsync(IncluirProdutoRequest request);

    Task<ProdutoResponse> AlterarAsync(AlterarProdutoRequest request);

    Task ExcluirAsync(ExcluirProdutoRequest request);

    Task<Produto> ObterAsync(ObterProdutoRequest request);

    Task<List<Produto>> ListarAsync(ListarProdutosRequest request);
}
