using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Abstractions.Produtos.Dtos;
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
        if (request.Prod_estoque != null)
            throw new ErroDeNegocio("Estoque não pode ser informado nesta operação", TipoErroDeNegocio.Validacao);

        string status = request.Prod_status ?? ProdutoValidacao.StatusAtivo;

        string? erro = ProdutoValidacao.Validar(request.Prod_codigo, request.Prod_descricao, request.Prod_precovenda, status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        if (await _repositorio.ExisteCodigoAsync(request.Prod_codigo!))
            throw new ErroDeNegocio("Código já cadastrado", TipoErroDeNegocio.Conflito);

        Produto produto = new Produto
        {
            Prod_codigo = request.Prod_codigo!,
            Prod_descricao = request.Prod_descricao!,
            Prod_precovenda = request.Prod_precovenda!.Value,
            Prod_status = status
        };

        await _repositorio.InserirAsync(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public async Task<ProdutoResponse> AlterarAsync(AlterarProdutoRequest request)
    {
        if (!await _repositorio.ExisteCodigoAsync(request.Prod_codigo))
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (request.Prod_estoque != null)
            throw new ErroDeNegocio("Estoque não pode ser informado nesta operação", TipoErroDeNegocio.Validacao);

        string? erro = ProdutoValidacao.ValidarAlteracao(request.Prod_descricao, request.Prod_precovenda, request.Prod_status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Produto produto = new Produto
        {
            Prod_codigo = request.Prod_codigo,
            Prod_descricao = request.Prod_descricao!,
            Prod_precovenda = request.Prod_precovenda!.Value,
            Prod_status = request.Prod_status!
        };

        await _repositorio.AtualizarAsync(produto);

        return ProdutoMapeamento.ParaResposta(produto);
    }

    public async Task ExcluirAsync(ExcluirProdutoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Prod_codigo))
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    public async Task DesativarAsync(DesativarProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (produto.Prod_status == ProdutoValidacao.StatusInativo)
            throw new ErroDeNegocio("Produto já está inativo", TipoErroDeNegocio.Validacao);

        produto.Prod_status = ProdutoValidacao.StatusInativo;

        await _repositorio.AtualizarAsync(produto);
    }

    public async Task AtivarAsync(AtivarProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (produto.Prod_status == ProdutoValidacao.StatusAtivo)
            throw new ErroDeNegocio("Produto já está ativo", TipoErroDeNegocio.Validacao);

        produto.Prod_status = ProdutoValidacao.StatusAtivo;

        await _repositorio.AtualizarAsync(produto);
    }

    public async Task<Produto> ObterAsync(ObterProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado", TipoErroDeNegocio.NaoEncontrado);

        return produto;
    }

    public async Task<List<Produto>> ListarAsync(ListarProdutosRequest request)
    {
        if (request.Prod_status != null)
        {
            string? erro = ProdutoValidacao.ValidarStatus(request.Prod_status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        return await _repositorio.ListarAsync(request.Prod_status);
    }
}
