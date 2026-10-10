using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Application.Produtos;

public class ProdutosService : IProdutosService
{
    private readonly IProdutoRepository _repositorio;

    public ProdutosService(IProdutoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public ProdutoResponse Incluir(IncluirProdutoRequest request)
    {
        string status = request.Status ?? ProdutoValidacao.StatusAtivo;

        string? erro = ProdutoValidacao.Validar(request.Codigo, request.Descricao, request.PrecoVenda, status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        if (_repositorio.ExisteCodigo(request.Codigo!))
            throw new ErroDeNegocio("Código já cadastrado", TipoErroDeNegocio.Conflito);

        Produto produto = new Produto
        {
            Codigo = request.Codigo!,
            Descricao = request.Descricao!,
            PrecoVenda = request.PrecoVenda!.Value,
            Status = status
        };

        _repositorio.Inserir(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public ProdutoResponse Alterar(AlterarProdutoRequest request)
    {
        if (!_repositorio.ExisteCodigo(request.Codigo))
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? erro = ProdutoValidacao.ValidarAlteracao(request.Descricao, request.PrecoVenda, request.Status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Produto produto = new Produto
        {
            Codigo = request.Codigo,
            Descricao = request.Descricao!,
            PrecoVenda = request.PrecoVenda!.Value,
            Status = request.Status!
        };

        _repositorio.Atualizar(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public void Excluir(ExcluirProdutoRequest request)
    {
        if (!_repositorio.Excluir(request.Codigo))
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    public Produto Obter(ObterProdutoRequest request)
    {
        Produto? produto = _repositorio.ObterPorCodigo(request.Codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        return produto;
    }

    public List<Produto> Listar(ListarProdutosRequest request)
    {
        if (request.Status != null)
        {
            string? erro = ProdutoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        return _repositorio.Listar(request.Status);
    }
}
