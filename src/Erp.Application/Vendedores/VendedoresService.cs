using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
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

    public async Task<AppResult<VendedorResponse>> IncluirAsync(IncluirVendedorRequest request)
    {
        string? erro = VendedorValidacao.ValidarNome(request.Vend_nome);

        if (erro != null)
            return new AppValidation(erro);

        string status = request.Vend_status ?? VendedorValidacao.StatusAtivo;

        erro = VendedorValidacao.ValidarStatus(status);

        if (erro != null)
            return new AppValidation(erro);

        string nome = request.Vend_nome!.Trim();

        if (await _repositorio.ExisteNomeAsync(nome, 0))
            return new AppConflito("Já existe um vendedor com este nome.");

        Vendedor vendedor = new Vendedor
        {
            Vend_nome = nome,
            Vend_status = VendedorValidacao.NormalizarStatus(status)
        };

        vendedor.Vend_id = await _repositorio.InserirAsync(vendedor);

        return VendedorMapeamento.ParaResposta(vendedor);
    }

    public async Task<AppResult<VendedorResponse>> AlterarAsync(AlterarVendedorRequest request)
    {
        Vendedor? vendedor = await _repositorio.ObterPorIdAsync(request.Vend_id);

        if (vendedor == null)
            return new ApplicationNotfound("Vendedor não encontrado.");

        if (request.Vend_nome != null)
        {
            string? erro = VendedorValidacao.ValidarNome(request.Vend_nome);

            if (erro != null)
                return new AppValidation(erro);

            string nome = request.Vend_nome.Trim();

            if (await _repositorio.ExisteNomeAsync(nome, vendedor.Vend_id))
                return new AppConflito("Já existe um vendedor com este nome.");

            vendedor.Vend_nome = nome;
        }

        if (request.Vend_status != null)
        {
            string? erro = VendedorValidacao.ValidarStatus(request.Vend_status);

            if (erro != null)
                return new AppValidation(erro);

            vendedor.Vend_status = VendedorValidacao.NormalizarStatus(request.Vend_status);
        }

        await _repositorio.AtualizarAsync(vendedor);

        return VendedorMapeamento.ParaResposta(vendedor);
    }

    public async Task<AppResult<Vendedor>> ObterAsync(ObterVendedorRequest request)
    {
        Vendedor? vendedor = await _repositorio.ObterPorIdAsync(request.Vend_id);

        if (vendedor == null)
            return new ApplicationNotfound("Vendedor não encontrado.");

        return vendedor;
    }

    public async Task<AppResult<List<Vendedor>>> ListarAsync(ListarVendedoresRequest request)
    {
        string? status = null;

        if (request.Vend_status != null)
        {
            string? erro = VendedorValidacao.ValidarStatus(request.Vend_status);

            if (erro != null)
                return new AppValidation(erro);

            status = VendedorValidacao.NormalizarStatus(request.Vend_status);
        }

        return await _repositorio.ListarAsync(status);
    }

    public async Task<AppResult<bool>> ExcluirAsync(ExcluirVendedorRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Vend_id))
            return new ApplicationNotfound("Vendedor não encontrado.");

        return true;
    }
}
