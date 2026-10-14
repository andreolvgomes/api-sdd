using Erp.Application.Abstractions.Common;
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

    public async Task<FormaPagamentoResponse> IncluirAsync(IncluirFormaPagamentoRequest request)
    {
        string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Form_descricao);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string status = request.Form_status ?? FormaPagamentoValidacao.StatusAtivo;

        erro = FormaPagamentoValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string descricao = request.Form_descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            throw new ErroDeNegocio("Já existe uma forma de pagamento com esta descrição.", TipoErroDeNegocio.Conflito);

        FormaPagamento formaPagamento = new FormaPagamento
        {
            Form_descricao = descricao,
            Form_tipo = request.Form_tipo!.Value,
            Form_status = FormaPagamentoValidacao.NormalizarStatus(status)
        };

        formaPagamento.Form_id = await _repositorio.InserirAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<FormaPagamentoResponse> AlterarAsync(AlterarFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Form_id);

        if (formaPagamento == null)
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);

        if (request.Form_descricao != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Form_descricao);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            string descricao = request.Form_descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, formaPagamento.Form_id))
                throw new ErroDeNegocio("Já existe uma forma de pagamento com esta descrição.", TipoErroDeNegocio.Conflito);

            formaPagamento.Form_descricao = descricao;
        }

        if (request.Form_tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            formaPagamento.Form_tipo = request.Form_tipo.Value;
        }

        if (request.Form_status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Form_status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            formaPagamento.Form_status = FormaPagamentoValidacao.NormalizarStatus(request.Form_status);
        }

        await _repositorio.AtualizarAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<FormaPagamento> ObterAsync(ObterFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Form_id);

        if (formaPagamento == null)
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);

        return formaPagamento;
    }

    public async Task<List<FormaPagamento>> ListarAsync(ListarFormasPagamentoRequest request)
    {
        string? status = null;

        if (request.Form_status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Form_status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            status = FormaPagamentoValidacao.NormalizarStatus(request.Form_status);
        }

        if (request.Form_tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Form_tipo);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        return await _repositorio.ListarAsync(status, request.Form_tipo);
    }

    public async Task ExcluirAsync(ExcluirFormaPagamentoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Form_id))
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);
    }
}
