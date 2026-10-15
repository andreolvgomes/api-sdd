using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
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

    public async Task<AppResult<ProdutoResponse>> IncluirAsync(IncluirProdutoRequest request)
    {
        if (request.Prod_estoque != null)
            return new AppValidation("Estoque não pode ser informado nesta operação");

        string status = request.Prod_status ?? ProdutoValidacao.StatusAtivo;

        string? erro = ProdutoValidacao.Validar(request.Prod_codigo, request.Prod_descricao, request.Prod_precovenda, status);

        if (erro != null)
            return new AppValidation(erro);

        if (await _repositorio.ExisteCodigoAsync(request.Prod_codigo!))
            return new AppConflito("Código já cadastrado");

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

    public async Task<AppResult<ProdutoResponse>> AlterarAsync(AlterarProdutoRequest request)
    {
        if (!await _repositorio.ExisteCodigoAsync(request.Prod_codigo))
            return new ApplicationNotfound("Produto não encontrado");

        if (request.Prod_estoque != null)
            return new AppValidation("Estoque não pode ser informado nesta operação");

        string? erro = ProdutoValidacao.ValidarAlteracao(request.Prod_descricao, request.Prod_precovenda, request.Prod_status);

        if (erro != null)
            return new AppValidation(erro);

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

    public async Task<AppResult<bool>> ExcluirAsync(ExcluirProdutoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Prod_codigo))
            return new ApplicationNotfound("Produto não encontrado");

        return true;
    }

    public async Task<AppResult<bool>> DesativarAsync(DesativarProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            return new ApplicationNotfound("Produto não encontrado");

        if (produto.Prod_status == ProdutoValidacao.StatusInativo)
            return new AppValidation("Produto já está inativo");

        produto.Prod_status = ProdutoValidacao.StatusInativo;

        await _repositorio.AtualizarAsync(produto);

        return true;
    }

    public async Task<AppResult<bool>> AtivarAsync(AtivarProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            return new ApplicationNotfound("Produto não encontrado");

        if (produto.Prod_status == ProdutoValidacao.StatusAtivo)
            return new AppValidation("Produto já está ativo");

        produto.Prod_status = ProdutoValidacao.StatusAtivo;

        await _repositorio.AtualizarAsync(produto);

        return true;
    }

    public async Task<AppResult<Produto>> ObterAsync(ObterProdutoRequest request)
    {
        Produto? produto = await _repositorio.ObterPorCodigoAsync(request.Prod_codigo);

        if (produto == null)
            return new ApplicationNotfound("Produto não encontrado");

        return produto;
    }

    public async Task<AppResult<List<Produto>>> ListarAsync(ListarProdutosRequest request)
    {
        if (request.Prod_status != null)
        {
            string? erro = ProdutoValidacao.ValidarStatus(request.Prod_status);

            if (erro != null)
                return new AppValidation(erro);
        }

        return await _repositorio.ListarAsync(request.Prod_status);
    }
}
