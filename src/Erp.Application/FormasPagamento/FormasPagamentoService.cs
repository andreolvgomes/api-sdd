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
        string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Descricao);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        erro = FormaPagamentoValidacao.ValidarTipo(request.Tipo);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string status = request.Status ?? FormaPagamentoValidacao.StatusAtivo;

        erro = FormaPagamentoValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        string descricao = request.Descricao!.Trim();

        if (await _repositorio.ExisteDescricaoAsync(descricao, 0))
            throw new ErroDeNegocio("Já existe uma forma de pagamento com esta descrição.", TipoErroDeNegocio.Conflito);

        FormaPagamento formaPagamento = new FormaPagamento
        {
            Descricao = descricao,
            Tipo = request.Tipo!.Value,
            Status = FormaPagamentoValidacao.NormalizarStatus(status)
        };

        formaPagamento.Id = await _repositorio.InserirAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<FormaPagamentoResponse> AlterarAsync(AlterarFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Id);

        if (formaPagamento == null)
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);

        if (request.Descricao != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarDescricao(request.Descricao);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            string descricao = request.Descricao.Trim();

            if (await _repositorio.ExisteDescricaoAsync(descricao, formaPagamento.Id))
                throw new ErroDeNegocio("Já existe uma forma de pagamento com esta descrição.", TipoErroDeNegocio.Conflito);

            formaPagamento.Descricao = descricao;
        }

        if (request.Tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Tipo);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            formaPagamento.Tipo = request.Tipo.Value;
        }

        if (request.Status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            formaPagamento.Status = FormaPagamentoValidacao.NormalizarStatus(request.Status);
        }

        await _repositorio.AtualizarAsync(formaPagamento);

        return FormaPagamentoMapeamento.ParaResposta(formaPagamento);
    }

    public async Task<FormaPagamento> ObterAsync(ObterFormaPagamentoRequest request)
    {
        FormaPagamento? formaPagamento = await _repositorio.ObterPorIdAsync(request.Id);

        if (formaPagamento == null)
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);

        return formaPagamento;
    }

    public async Task<List<FormaPagamento>> ListarAsync(ListarFormasPagamentoRequest request)
    {
        string? status = null;

        if (request.Status != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarStatus(request.Status);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

            status = FormaPagamentoValidacao.NormalizarStatus(request.Status);
        }

        if (request.Tipo != null)
        {
            string? erro = FormaPagamentoValidacao.ValidarTipo(request.Tipo);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        return await _repositorio.ListarAsync(status, request.Tipo);
    }

    public async Task ExcluirAsync(ExcluirFormaPagamentoRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Id))
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);
    }
}
