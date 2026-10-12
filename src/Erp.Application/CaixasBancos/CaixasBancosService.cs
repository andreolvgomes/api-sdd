using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Application.Abstractions.Common;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Application.CaixasBancos;

public class CaixasBancosService : ICaixasBancosService
{
    private readonly ICaixaBancoRepository _repositorio;

    public CaixasBancosService(ICaixaBancoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<CaixaBancoResponse> IncluirAsync(IncluirCaixaBancoRequest request)
    {
        string? erro = CaixaBancoValidacao.ValidarDescricao(request.Descricao);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string status = request.Status ?? CaixaBancoValidacao.StatusAtivo;

        erro = CaixaBancoValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string descricao = request.Descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            throw new ErroDeNegocio("Já existe um caixa ou banco com esta descrição.", TipoErroDeNegocio.Conflito);

        CaixaBanco caixaBanco = new CaixaBanco
        {
            Descricao = descricao,
            Status = CaixaBancoValidacao.NormalizarStatus(status)
        };

        caixaBanco.Id = await _repositorio.InserirAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<CaixaBancoResponse> AlterarAsync(AlterarCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Id);

        if (caixaBanco == null)
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (request.Descricao != null)
        {
            string? erro = CaixaBancoValidacao.ValidarDescricao(request.Descricao);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            string descricao = request.Descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, caixaBanco.Id))
                throw new ErroDeNegocio("Já existe um caixa ou banco com esta descrição.", TipoErroDeNegocio.Conflito);

            caixaBanco.Descricao = descricao;
        }

        if (request.Status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            caixaBanco.Status = CaixaBancoValidacao.NormalizarStatus(request.Status);
        }

        await _repositorio.AtualizarAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<CaixaBanco> ObterAsync(ObterCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Id);

        if (caixaBanco == null)
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return caixaBanco;
    }

    public async Task<List<CaixaBanco>> ListarAsync(ListarCaixasBancosRequest request)
    {
        string? status = null;

        if (request.Status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            status = CaixaBancoValidacao.NormalizarStatus(request.Status);
        }

        return await _repositorio.ListarAsync(status);
    }

    public async Task ExcluirAsync(ExcluirCaixaBancoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Id))
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);
    }
}
