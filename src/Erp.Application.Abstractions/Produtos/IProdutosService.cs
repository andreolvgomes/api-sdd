using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Produtos;

public interface IProdutosService
{
    ProdutoResponse Incluir(IncluirProdutoRequest request);

    ProdutoResponse Alterar(AlterarProdutoRequest request);

    void Excluir(ExcluirProdutoRequest request);

    Produto Obter(ObterProdutoRequest request);

    List<Produto> Listar(ListarProdutosRequest request);
}
