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
        string? erro = CaixaBancoValidacao.ValidarDescricao(request.Caix_descricao);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string status = request.Caix_status ?? CaixaBancoValidacao.StatusAtivo;

        erro = CaixaBancoValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string descricao = request.Caix_descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            throw new ErroDeNegocio("Já existe um caixa ou banco com esta descrição.", TipoErroDeNegocio.Conflito);

        CaixaBanco caixaBanco = new CaixaBanco
        {
            Caix_descricao = descricao,
            Caix_status = CaixaBancoValidacao.NormalizarStatus(status)
        };

        caixaBanco.Caix_id = await _repositorio.InserirAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<CaixaBancoResponse> AlterarAsync(AlterarCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Caix_id);

        if (caixaBanco == null)
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (request.Caix_descricao != null)
        {
            string? erro = CaixaBancoValidacao.ValidarDescricao(request.Caix_descricao);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            string descricao = request.Caix_descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, caixaBanco.Caix_id))
                throw new ErroDeNegocio("Já existe um caixa ou banco com esta descrição.", TipoErroDeNegocio.Conflito);

            caixaBanco.Caix_descricao = descricao;
        }

        if (request.Caix_status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Caix_status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            caixaBanco.Caix_status = CaixaBancoValidacao.NormalizarStatus(request.Caix_status);
        }

        await _repositorio.AtualizarAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<CaixaBanco> ObterAsync(ObterCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Caix_id);

        if (caixaBanco == null)
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return caixaBanco;
    }

    public async Task<List<CaixaBanco>> ListarAsync(ListarCaixasBancosRequest request)
    {
        string? status = null;

        if (request.Caix_status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Caix_status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            status = CaixaBancoValidacao.NormalizarStatus(request.Caix_status);
        }

        return await _repositorio.ListarAsync(status);
    }

    public async Task ExcluirAsync(ExcluirCaixaBancoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Caix_id))
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);
    }
}
