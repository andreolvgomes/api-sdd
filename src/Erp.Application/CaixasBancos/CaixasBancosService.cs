using Erp.Application.Abstractions.CaixasBancos;
using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
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

    public async Task<AppResult<CaixaBancoResponse>> IncluirAsync(IncluirCaixaBancoRequest request)
    {
        string? erro = CaixaBancoValidacao.ValidarDescricao(request.Caix_descricao);

        if (erro != null)
            return new AppValidation(erro);

        string status = request.Caix_status ?? CaixaBancoValidacao.StatusAtivo;

        erro = CaixaBancoValidacao.ValidarStatus(status);

        if (erro != null)
            return new AppValidation(erro);

        string descricao = request.Caix_descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            return new AppConflito("Já existe um caixa ou banco com esta descrição.");

        CaixaBanco caixaBanco = new CaixaBanco
        {
            Caix_descricao = descricao,
            Caix_status = CaixaBancoValidacao.NormalizarStatus(status)
        };

        caixaBanco.Caix_id = await _repositorio.InserirAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<AppResult<CaixaBancoResponse>> AlterarAsync(AlterarCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Caix_id);

        if (caixaBanco == null)
            return new ApplicationNotfound("Caixa ou banco não encontrado.");

        if (request.Caix_descricao != null)
        {
            string? erro = CaixaBancoValidacao.ValidarDescricao(request.Caix_descricao);

            if (erro != null)
                return new AppValidation(erro);

            string descricao = request.Caix_descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, caixaBanco.Caix_id))
                return new AppConflito("Já existe um caixa ou banco com esta descrição.");

            caixaBanco.Caix_descricao = descricao;
        }

        if (request.Caix_status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Caix_status);

            if (erro != null)
                return new AppValidation(erro);

            caixaBanco.Caix_status = CaixaBancoValidacao.NormalizarStatus(request.Caix_status);
        }

        await _repositorio.AtualizarAsync(caixaBanco);

        return CaixaBancoMapeamento.ParaResposta(caixaBanco);
    }

    public async Task<AppResult<CaixaBanco>> ObterAsync(ObterCaixaBancoRequest request)
    {
        CaixaBanco? caixaBanco = await _repositorio.ObterPorIdAsync(request.Caix_id);

        if (caixaBanco == null)
            return new ApplicationNotfound("Caixa ou banco não encontrado.");

        return caixaBanco;
    }

    public async Task<AppResult<List<CaixaBanco>>> ListarAsync(ListarCaixasBancosRequest request)
    {
        string? status = null;

        if (request.Caix_status != null)
        {
            string? erro = CaixaBancoValidacao.ValidarStatus(request.Caix_status);

            if (erro != null)
                return new AppValidation(erro);

            status = CaixaBancoValidacao.NormalizarStatus(request.Caix_status);
        }

        return await _repositorio.ListarAsync(status);
    }

    public async Task<AppResult<bool>> ExcluirAsync(ExcluirCaixaBancoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Caix_id))
            return new ApplicationNotfound("Caixa ou banco não encontrado.");

        return true;
    }
}
