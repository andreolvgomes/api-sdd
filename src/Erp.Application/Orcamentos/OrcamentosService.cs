using System.Globalization;
using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using Erp.Core.Entities;
using Erp.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Erp.Application.Orcamentos;

public class OrcamentosService : IOrcamentosService
{
    private readonly IOrcamentoRepository _repositorio;
    private readonly IClienteRepository _clientes;
    private readonly IVendedorRepository _vendedores;
    private readonly IProdutoRepository _produtos;
    private readonly IFormaPagamentoRepository _formasPagamento;
    private readonly ICaixaBancoRepository _caixas;
    private readonly ILogger<OrcamentosService> _log;

    public OrcamentosService(
        IOrcamentoRepository repositorio,
        IClienteRepository clientes,
        IVendedorRepository vendedores,
        IProdutoRepository produtos,
        IFormaPagamentoRepository formasPagamento,
        ICaixaBancoRepository caixas,
        ILogger<OrcamentosService> log)
    {
        _repositorio = repositorio;
        _clientes = clientes;
        _vendedores = vendedores;
        _produtos = produtos;
        _formasPagamento = formasPagamento;
        _caixas = caixas;
        _log = log;
    }

    public async Task<OrcamentoResponse> IncluirAsync(IncluirOrcamentoRequest request)
    {
        if (request.Clie_id == null)
            throw new ErroDeNegocio("Cliente é obrigatório.", TipoErroDeNegocio.Validacao);

        if (request.Vend_id == null)
            throw new ErroDeNegocio("Vendedor é obrigatório.", TipoErroDeNegocio.Validacao);

        string? erro = OrcamentoValidacao.ValidarItem(request.Prod_codigo, request.Orci_quantidade);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = await ValidarClienteAsync(request.Clie_id.Value);
        Vendedor vendedor = await ValidarVendedorAsync(request.Vend_id.Value);
        Produto produto = await ValidarProdutoAsync(request.Prod_codigo!, "Produto inativo não pode ser incluído.");

        Orcamento orcamento = new Orcamento
        {
            Orca_data = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Clie_id = cliente.Clie_id,
            Vend_id = vendedor.Vend_id
        };

        orcamento.Orca_id = await _repositorio.InserirAsync(orcamento);

        await GravarItemAsync(orcamento.Orca_id, produto, request.Orci_quantidade!.Value);
        await RecalcularTotaisAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> IncluirItemAsync(IncluirItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);
        await ValidarSemPagamentosAsync(orcamento.Orca_id);

        string? erro = OrcamentoValidacao.ValidarItem(request.Prod_codigo, request.Orci_quantidade);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Produto produto = await ValidarProdutoAsync(request.Prod_codigo!, "Produto inativo não pode ser incluído.");

        await GravarItemAsync(orcamento.Orca_id, produto, request.Orci_quantidade!.Value);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> AlterarAsync(AlterarOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);
        await ValidarSemPagamentosAsync(orcamento.Orca_id);

        if (request.Clie_id == null && request.Vend_id == null)
            throw new ErroDeNegocio("Informe o cliente ou o vendedor para alterar.", TipoErroDeNegocio.Validacao);

        if (request.Clie_id != null)
        {
            Cliente cliente = await ValidarClienteAsync(request.Clie_id.Value);
            orcamento.Clie_id = cliente.Clie_id;
        }

        if (request.Vend_id != null)
        {
            Vendedor vendedor = await ValidarVendedorAsync(request.Vend_id.Value);
            orcamento.Vend_id = vendedor.Vend_id;
        }

        await _repositorio.AtualizarAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> AlterarItemAsync(AlterarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);
        await ValidarSemPagamentosAsync(orcamento.Orca_id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (item.Orci_cancelado)
            throw new ErroDeNegocio("Item cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        if (request.Prod_codigo == null && request.Orci_quantidade == null)
            throw new ErroDeNegocio("Informe a quantidade ou o produto para alterar.", TipoErroDeNegocio.Validacao);

        if (request.Prod_codigo != null && string.IsNullOrWhiteSpace(request.Prod_codigo))
            throw new ErroDeNegocio("Produto é obrigatório.", TipoErroDeNegocio.Validacao);

        if (request.Orci_quantidade != null)
        {
            string? erro = OrcamentoValidacao.ValidarQuantidade(request.Orci_quantidade);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        decimal quantidade = request.Orci_quantidade ?? item.Orci_quantidade;
        string codigoProduto = item.Prod_codigo;
        decimal preco = item.Orci_preco;

        if (request.Prod_codigo != null)
        {
            Produto produto = await ValidarProdutoAsync(request.Prod_codigo, "Produto inativo não pode ser usado.");

            if (produto.Prod_codigo != item.Prod_codigo)
                preco = produto.Prod_precovenda;

            codigoProduto = produto.Prod_codigo;
        }

        item.Prod_codigo = codigoProduto;
        item.Orci_quantidade = quantidade;
        item.Orci_preco = preco;
        item.Orci_total = OrcamentoValidacao.Arredondar(quantidade * preco);

        await _repositorio.AtualizarItemAsync(item);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> CancelarItemAsync(CancelarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);
        await ValidarSemPagamentosAsync(orcamento.Orca_id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (item.Orci_cancelado)
            throw new ErroDeNegocio("Item já está cancelado.", TipoErroDeNegocio.Validacao);

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Orca_id, item.Orci_numero_item, true);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> ReativarItemAsync(ReativarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);
        await ValidarSemPagamentosAsync(orcamento.Orca_id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (!item.Orci_cancelado)
            throw new ErroDeNegocio("Item não está cancelado.", TipoErroDeNegocio.Validacao);

        await ValidarProdutoAsync(item.Prod_codigo, "Produto inativo não pode ser usado.");

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Orca_id, item.Orci_numero_item, false);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> CancelarAsync(CancelarOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (orcamento.Orca_cancelado)
            throw new ErroDeNegocio("Orçamento já está cancelado.", TipoErroDeNegocio.Validacao);

        await _repositorio.CancelarAsync(orcamento.Orca_id);
        orcamento.Orca_cancelado = true;

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> FecharAsync(FecharOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (orcamento.Orca_cancelado)
            throw new ErroDeNegocio("Orçamento cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        await ValidarSemPagamentosAsync(orcamento.Orca_id);

        if (request.Orca_desconto == null)
            throw new ErroDeNegocio("Desconto é obrigatório.", TipoErroDeNegocio.Validacao);

        string? erro = OrcamentoValidacao.ValidarDesconto(request.Orca_desconto.Value);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        List<OrcamentoItem> itens = await _repositorio.ListarItensAsync(orcamento.Orca_id);
        List<OrcamentoItem> ativos = itens.Where(item => !item.Orci_cancelado).ToList();

        if (ativos.Count == 0)
            throw new ErroDeNegocio("Orçamento sem itens ativos não pode ser fechado.", TipoErroDeNegocio.Validacao);

        decimal subtotal = ativos.Sum(item => item.Orci_total);

        if (request.Orca_desconto.Value > subtotal)
            throw new ErroDeNegocio("Desconto não pode ser maior que o subtotal.", TipoErroDeNegocio.Validacao);

        await _repositorio.DefinirDescontoAsync(orcamento.Orca_id, request.Orca_desconto.Value);
        orcamento.Orca_desconto = request.Orca_desconto.Value;
        await RecalcularTotaisAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> LancarPagamentoAsync(LancarPagamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Orca_id);

        if (request.Form_id == null)
            throw new ErroDeNegocio("Forma de pagamento é obrigatória.", TipoErroDeNegocio.Validacao);

        if (request.Caix_id == null)
            throw new ErroDeNegocio("Caixa ou banco é obrigatório.", TipoErroDeNegocio.Validacao);

        if (request.Orcp_valor == null)
            throw new ErroDeNegocio("Valor do pagamento é obrigatório.", TipoErroDeNegocio.Validacao);

        string? erro = OrcamentoValidacao.ValidarValorPagamento(request.Orcp_valor.Value);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        FormaPagamento forma = await ValidarFormaPagamentoAsync(request.Form_id.Value);
        await ValidarCaixaBancoAsync(request.Caix_id.Value);

        Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.Clie_id);

        if (cliente != null && cliente.Clie_bloqueio == "bloqueado" && !OrcamentoValidacao.FormaPermitidaParaClienteBloqueado(forma.Form_tipo))
            throw new ErroDeNegocio("Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)", TipoErroDeNegocio.Validacao);

        OrcamentoPagamento pagamento = new OrcamentoPagamento
        {
            Orca_id = orcamento.Orca_id,
            Form_id = request.Form_id.Value,
            Caix_id = request.Caix_id.Value,
            Orcp_valor = request.Orcp_valor.Value
        };

        decimal total = orcamento.Orca_total_com_desconto;

        bool gravou = await _repositorio.InserirPagamentoSeCouberAsync(pagamento, total);

        if (!gravou)
            throw new ErroDeNegocio("Valor maior que o saldo do orçamento.", TipoErroDeNegocio.Validacao);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> CancelarPagamentoAsync(CancelarPagamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (orcamento.Orca_cancelado)
            throw new ErroDeNegocio("Orçamento cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        OrcamentoPagamento? pagamento = await _repositorio.ObterPagamentoAsync(orcamento.Orca_id, request.Orcp_numero_pagamento);

        if (pagamento == null)
            throw new ErroDeNegocio("Pagamento não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (pagamento.Orcp_cancelado)
            throw new ErroDeNegocio("Pagamento já está cancelado.", TipoErroDeNegocio.Validacao);

        await _repositorio.CancelarPagamentoAsync(orcamento.Orca_id, pagamento.Orcp_numero_pagamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> ObterAsync(ObterOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(request.Orca_id);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<List<OrcamentoResumoResponse>> ListarAsync(ListarOrcamentosRequest request)
    {
        List<Orcamento> orcamentos = await _repositorio.ListarAsync(request.Clie_id, request.Vend_id);

        List<OrcamentoResumoResponse> resposta = new List<OrcamentoResumoResponse>();

        foreach (Orcamento orcamento in orcamentos)
        {
            Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.Clie_id);
            Vendedor? vendedor = await _vendedores.ObterPorIdAsync(orcamento.Vend_id);

            resposta.Add(OrcamentoMapeamento.ParaResumo(orcamento, cliente, vendedor));
        }

        return resposta;
    }

    private async Task RecalcularTotaisAsync(Orcamento orcamento)
    {
        List<OrcamentoItem> itens = await _repositorio.ListarItensAsync(orcamento.Orca_id);
        decimal total = itens.Where(item => !item.Orci_cancelado).Sum(item => item.Orci_total);
        decimal totalComDesconto = OrcamentoValidacao.Arredondar(total - orcamento.Orca_desconto);

        await _repositorio.AtualizarTotaisAsync(orcamento.Orca_id, total, totalComDesconto);

        orcamento.Orca_total = total;
        orcamento.Orca_total_com_desconto = totalComDesconto;
    }

    private async Task ZerarDescontoAsync(Orcamento orcamento)
    {
        await _repositorio.DefinirDescontoAsync(orcamento.Orca_id, 0);
        orcamento.Orca_desconto = 0;
        await RecalcularTotaisAsync(orcamento);
    }

    private async Task<Orcamento> ObterOrcamentoExistenteAsync(int id)
    {
        Orcamento? orcamento = await _repositorio.ObterPorIdAsync(id);

        if (orcamento == null)
            throw new ErroDeNegocio("Orçamento não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return orcamento;
    }

    private async Task<Orcamento> ObterOrcamentoAbertoAsync(int id)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(id);

        if (orcamento.Orca_cancelado)
            throw new ErroDeNegocio("Orçamento cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        return orcamento;
    }

    private async Task ValidarSemPagamentosAsync(int orcamentoId)
    {
        int quantidade = await _repositorio.ContarPagamentosNaoCanceladosAsync(orcamentoId);

        if (quantidade > 0)
            throw new ErroDeNegocio("Orçamento possui pagamentos não cancelados.", TipoErroDeNegocio.Validacao);
    }

    private async Task<OrcamentoItem> ObterItemExistenteAsync(int orcamentoId, string numeroItem)
    {
        OrcamentoItem? item = await _repositorio.ObterItemAsync(orcamentoId, numeroItem);

        if (item == null)
            throw new ErroDeNegocio("Item não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return item;
    }

    private async Task<Cliente> ValidarClienteAsync(int id)
    {
        Cliente? cliente = await _clientes.ObterPorIdAsync(id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(cliente.Clie_status))
            throw new ErroDeNegocio("Cliente inativo não pode ser usado.", TipoErroDeNegocio.Validacao);

        return cliente;
    }

    private async Task<Vendedor> ValidarVendedorAsync(int id)
    {
        Vendedor? vendedor = await _vendedores.ObterPorIdAsync(id);

        if (vendedor == null)
            throw new ErroDeNegocio("Vendedor não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(vendedor.Vend_status))
            throw new ErroDeNegocio("Vendedor inativo não pode ser usado.", TipoErroDeNegocio.Validacao);

        return vendedor;
    }

    private async Task<Produto> ValidarProdutoAsync(string codigo, string mensagemInativo)
    {
        Produto? produto = await _produtos.ObterPorCodigoAsync(codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(produto.Prod_status))
            throw new ErroDeNegocio(mensagemInativo, TipoErroDeNegocio.Validacao);

        return produto;
    }

    private async Task<FormaPagamento> ValidarFormaPagamentoAsync(int id)
    {
        FormaPagamento? forma = await _formasPagamento.ObterPorIdAsync(id);

        if (forma == null)
            throw new ErroDeNegocio("Forma de pagamento não encontrada.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(forma.Form_status))
            throw new ErroDeNegocio("Forma de pagamento inativa não pode ser usada.", TipoErroDeNegocio.Validacao);

        return forma;
    }

    private async Task<CaixaBanco> ValidarCaixaBancoAsync(int id)
    {
        CaixaBanco? caixa = await _caixas.ObterPorIdAsync(id);

        if (caixa == null)
            throw new ErroDeNegocio("Caixa ou banco não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(caixa.Caix_status))
            throw new ErroDeNegocio("Caixa ou banco inativo não pode ser usado.", TipoErroDeNegocio.Validacao);

        return caixa;
    }

    private async Task GravarItemAsync(int orcamentoId, Produto produto, decimal quantidade)
    {
        OrcamentoItem item = new OrcamentoItem
        {
            Orca_id = orcamentoId,
            Prod_codigo = produto.Prod_codigo,
            Orci_quantidade = quantidade,
            Orci_preco = produto.Prod_precovenda,
            Orci_total = OrcamentoValidacao.Arredondar(quantidade * produto.Prod_precovenda)
        };

        try
        {
            await _repositorio.InserirItemAsync(item);
        }
        catch (Exception erro)
        {
            _log.LogError(erro, "Falha ao gravar item do orçamento {OrcamentoId} (produto {CodigoProduto})", orcamentoId, produto.Prod_codigo);
            throw;
        }
    }

    private async Task<OrcamentoResponse> MontarRespostaAsync(Orcamento orcamento)
    {
        Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.Clie_id);
        Vendedor? vendedor = await _vendedores.ObterPorIdAsync(orcamento.Vend_id);
        List<OrcamentoItem> itens = await _repositorio.ListarItensAsync(orcamento.Orca_id);
        List<OrcamentoPagamento> pagamentos = await _repositorio.ListarPagamentosAsync(orcamento.Orca_id);

        return OrcamentoMapeamento.ParaResposta(orcamento, cliente, vendedor, itens, pagamentos);
    }
}
