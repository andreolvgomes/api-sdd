using Erp.Core.Entities;

namespace Erp.Core.Repositories;

public interface IProdutoRepository
{
    bool ExisteCodigo(string codigo);

    void Inserir(Produto produto);

    Produto? ObterPorCodigo(string codigo);

    List<Produto> Listar(string? status);

    bool Atualizar(Produto produto);

    bool Excluir(string codigo);
}
