using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Vendedores;
using Erp.Application.Abstractions.Vendedores.Dtos;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Application.Vendedores;

public class VendedoresService : IVendedoresService
{
    private readonly IVendedorRepository _repositorio;

    public VendedoresService(IVendedorRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<VendedorResponse> IncluirAsync(IncluirVendedorRequest request)
    {
        string? erro = VendedorValidacao.ValidarNome(request.Nome);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string status = request.Status ?? VendedorValidacao.StatusAtivo;

        erro = VendedorValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string nome = request.Nome!.Trim();

        if (await _repositorio.ExisteNomeAsync(nome, 0))
            throw new ErroDeNegocio("Já existe um vendedor com este nome.", TipoErroDeNegocio.Conflito);

        Vendedor vendedor = new Vendedor
        {
            Nome = nome,
            Status = VendedorValidacao.NormalizarStatus(status)
        };

        vendedor.Id = await _repositorio.InserirAsync(vendedor);

        return VendedorMapeamento.ParaResposta(vendedor);
    }

    public async Task<VendedorResponse> AlterarAsync(AlterarVendedorRequest request)
    {
        Vendedor? vendedor = await _repositorio.ObterPorIdAsync(request.Id);

        if (vendedor == null)
            throw new ErroDeNegocio("Vendedor não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (request.Nome != null)
        {
            string? erro = VendedorValidacao.ValidarNome(request.Nome);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            string nome = request.Nome.Trim();

            if (await _repositorio.ExisteNomeAsync(nome, vendedor.Id))
                throw new ErroDeNegocio("Já existe um vendedor com este nome.", TipoErroDeNegocio.Conflito);

            vendedor.Nome = nome;
        }

        if (request.Status != null)
        {
            string? erro = VendedorValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            vendedor.Status = VendedorValidacao.NormalizarStatus(request.Status);
        }

        await _repositorio.AtualizarAsync(vendedor);

        return VendedorMapeamento.ParaResposta(vendedor);
    }

    public async Task<Vendedor> ObterAsync(ObterVendedorRequest request)
    {
        Vendedor? vendedor = await _repositorio.ObterPorIdAsync(request.Id);

        if (vendedor == null)
            throw new ErroDeNegocio("Vendedor não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return vendedor;
    }

    public async Task<List<Vendedor>> ListarAsync(ListarVendedoresRequest request)
    {
        string? status = null;

        if (request.Status != null)
        {
            string? erro = VendedorValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            status = VendedorValidacao.NormalizarStatus(request.Status);
        }

        return await _repositorio.ListarAsync(status);
    }

    public async Task ExcluirAsync(ExcluirVendedorRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Id))
            throw new ErroDeNegocio("Vendedor não encontrado.", TipoErroDeNegocio.NaoEncontrado);
    }
}
