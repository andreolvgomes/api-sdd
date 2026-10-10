using Erp.Application.Abstractions.Produtos;
using Erp.Core.Entities;

namespace Erp.Application.Produtos;

public static class ProdutoMapeamento
{
    public static ProdutoResponse ParaResposta(Produto produto)
    {
        return new ProdutoResponse
        {
            Codigo = produto.Codigo,
            Descricao = produto.Descricao,
            PrecoVenda = produto.PrecoVenda,
            Status = produto.Status
        };
    }
}
