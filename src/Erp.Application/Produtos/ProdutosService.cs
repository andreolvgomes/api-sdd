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

    public async Task<ProdutoResponse> IncluirAsync(IncluirProdutoRequest request)
    {
        string status = request.Status ?? ProdutoValidacao.StatusAtivo;

        string? erro = ProdutoValidacao.Validar(request.Codigo, request.Descricao, request.PrecoVenda, status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        if (await _repositorio.ExisteCodigoAsync(request.Codigo!))
            throw new ErroDeNegocio("Código já cadastrado", TipoErroDeNegocio.Conflito);

        Produto produto = new Produto
        {
            Codigo = request.Codigo!,
            Descricao = request.Descricao!,
            PrecoVenda = request.PrecoVenda!.Value,
            Status = status
        };

        await _repositorio.InserirAsync(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public async Task<ProdutoResponse> AlterarAsync(AlterarProdutoRequest request)
    {
        if (!await _repositorio.ExisteCodigoAsync(request.Codigo))
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

        await _repositorio.AtualizarAsync(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public async Task ExcluirAsync(ExcluirProdutoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Codigo))
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    public async Task<Produto> ObterAsync(ObterProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        return produto;
    }

    public async Task<List<Produto>> ListarAsync(ListarProdutosRequest request)
    {
        if (request.Status != null)
        {
            string? erro = ProdutoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        return await _repositorio.ListarAsync(request.Status);
    }
}
