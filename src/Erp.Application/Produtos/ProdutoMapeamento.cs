using Erp.Application.Abstractions.Produtos;
using Erp.Application.Abstractions.Produtos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Produtos;

public static class ProdutoMapeamento
{
    public static ProdutoResponse ParaResposta(Produto produto)
    {
        return new ProdutoResponse
        {
            Prod_codigo = produto.Prod_codigo,
            Prod_descricao = produto.Prod_descricao,
            Prod_precovenda = produto.Prod_precovenda,
            Prod_status = produto.Prod_status,
            Prod_estoque = produto.Prod_estoque
        };
    }
}
