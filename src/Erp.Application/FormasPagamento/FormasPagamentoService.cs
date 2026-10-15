using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
using Erp.Application.Abstractions.FormasPagamento;
using Erp.Application.Abstractions.FormasPagamento.Dtos;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Application.FormasPagamento;

public class FormasPagamentoService : IFormasPagamentoService
{
    private readonly IFormaPagamentoRepository _repositorio;

    public FormasPagamentoService(IFormaPagamentoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<AppResult<FormaPagamentoResponse>> IncluirAsync(IncluirFormaPagamentoRequest request)
    {
        string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Form_descricao);

        if (erro != null)
            return new AppValidation(erro);

        erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

        if (erro != null)
            return new AppValidation(erro);

        string status = request.Form_status ?? FormaPagamentoValidacao.StatusAtivo;

        erro = FormaPagamentoValidacao.ValidarStatus(status);

        if (erro != null)
            return new AppValidation(erro);

        string descricao = request.Form_descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            return new AppConflito("Já existe uma forma de pagamento com esta descrição.");

        FormaPagamento formaPagamento = new FormaPagamento
        {
            Form_descricao = descricao,
            Form_tipo = request.Form_tipo!.Value,
            Form_status = FormaPagamentoValidacao.NormalizarStatus(status)
        };

        formaPagamento.Form_id = await _repositorio.InserirAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<AppResult<FormaPagamentoResponse>> AlterarAsync(AlterarFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Form_id);

        if (formaPagamento == null)
            return new AppNotFound("Forma de pagamento não encontrada.");

        if (request.Form_descricao != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Form_descricao);

            if (erro != null)
                return new AppValidation(erro);

            string descricao = request.Form_descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, formaPagamento.Form_id))
                return new AppConflito("Já existe uma forma de pagamento com esta descrição.");

            formaPagamento.Form_descricao = descricao;
        }

        if (request.Form_tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

            if (erro != null)
                return new AppValidation(erro);

            formaPagamento.Form_tipo = request.Form_tipo.Value;
        }

        if (request.Form_status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Form_status);

            if (erro != null)
                return new AppValidation(erro);

            formaPagamento.Form_status = FormaPagamentoValidacao.NormalizarStatus(request.Form_status);
        }

        await _repositorio.AtualizarAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<AppResult<FormaPagamento>> ObterAsync(ObterFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Form_id);

        if (formaPagamento == null)
            return new AppNotFound("Forma de pagamento não encontrada.");

        return formaPagamento;
    }

    public async Task<AppResult<List<FormaPagamento>>> ListarAsync(ListarFormasPagamentoRequest request)
    {
        string? status = null;

        if (request.Form_status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Form_status);

            if (erro != null)
                return new AppValidation(erro);

            status = FormaPagamentoValidacao.NormalizarStatus(request.Form_status);
        }

        if (request.Form_tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

            if (erro != null)
                return new AppValidation(erro);
        }

        return await _repositorio.ListarAsync(status, request.Form_tipo);
    }

    public async Task<AppResult<bool>> ExcluirAsync(ExcluirFormaPagamentoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Form_id))
            return new AppNotFound("Forma de pagamento não encontrada.");

        return true;
    }
}
