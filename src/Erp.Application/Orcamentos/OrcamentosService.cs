using System.Globalization;
using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
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

    public async Task<AppResult<OrcamentoResponse>> IncluirAsync(IncluirOrcamentoRequest request)
    {
        if (request.Clie_id == null)
            return new AppValidation("Cliente é obrigatório.");

        if (request.Vend_id == null)
            return new AppValidation("Vendedor é obrigatório.");

        string? erro = OrcamentoValidacao.ValidarItem(request.Prod_codigo, request.Orci_quantidade);

        if (erro != null)
            return new AppValidation(erro);

        AppResult<Cliente> clienteResultado = await ValidarClienteAsync(request.Clie_id.Value);

        if (!clienteResultado.Sucess)
            return clienteResultado.Propagar<OrcamentoResponse>();

        AppResult<Vendedor> vendedorResultado = await ValidarVendedorAsync(request.Vend_id.Value);

        if (!vendedorResultado.Sucess)
            return vendedorResultado.Propagar<OrcamentoResponse>();

        AppResult<Produto> produtoResultado = await ValidarProdutoAsync(request.Prod_codigo!, "Produto inativo não pode ser incluído.");

        if (!produtoResultado.Sucess)
            return produtoResultado.Propagar<OrcamentoResponse>();

        Cliente cliente = clienteResultado.Data!;
        Vendedor vendedor = vendedorResultado.Data!;
        Produto produto = produtoResultado.Data!;

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

    public async Task<AppResult<OrcamentoResponse>> IncluirItemAsync(IncluirItemOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        string? erro = OrcamentoValidacao.ValidarItem(request.Prod_codigo, request.Orci_quantidade);

        if (erro != null)
            return new AppValidation(erro);

        AppResult<Produto> produtoResultado = await ValidarProdutoAsync(request.Prod_codigo!, "Produto inativo não pode ser incluído.");

        if (!produtoResultado.Sucess)
            return produtoResultado.Propagar<OrcamentoResponse>();

        await GravarItemAsync(orcamento.Orca_id, produtoResultado.Data!, request.Orci_quantidade!.Value);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> AlterarAsync(AlterarOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        if (request.Clie_id == null && request.Vend_id == null)
            return new AppValidation("Informe o cliente ou o vendedor para alterar.");

        if (request.Clie_id != null)
        {
            AppResult<Cliente> clienteResultado = await ValidarClienteAsync(request.Clie_id.Value);

            if (!clienteResultado.Sucess)
                return clienteResultado.Propagar<OrcamentoResponse>();

            orcamento.Clie_id = clienteResultado.Data!.Clie_id;
        }

        if (request.Vend_id != null)
        {
            AppResult<Vendedor> vendedorResultado = await ValidarVendedorAsync(request.Vend_id.Value);

            if (!vendedorResultado.Sucess)
                return vendedorResultado.Propagar<OrcamentoResponse>();

            orcamento.Vend_id = vendedorResultado.Data!.Vend_id;
        }

        await _repositorio.AtualizarAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> AlterarItemAsync(AlterarItemOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        AppResult<OrcamentoItem> itemResultado = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (!itemResultado.Sucess)
            return itemResultado.Propagar<OrcamentoResponse>();

        OrcamentoItem item = itemResultado.Data!;

        if (item.Orci_cancelado)
            return new AppValidation("Item cancelado não pode ser alterado.");

        if (request.Prod_codigo == null && request.Orci_quantidade == null)
            return new AppValidation("Informe a quantidade ou o produto para alterar.");

        if (request.Prod_codigo != null && string.IsNullOrWhiteSpace(request.Prod_codigo))
            return new AppValidation("Produto é obrigatório.");

        if (request.Orci_quantidade != null)
        {
            string? erro = OrcamentoValidacao.ValidarQuantidade(request.Orci_quantidade);

            if (erro != null)
                return new AppValidation(erro);
        }

        decimal quantidade = request.Orci_quantidade ?? item.Orci_quantidade;
        string codigoProduto = item.Prod_codigo;
        decimal preco = item.Orci_preco;

        if (request.Prod_codigo != null)
        {
            AppResult<Produto> produtoResultado = await ValidarProdutoAsync(request.Prod_codigo, "Produto inativo não pode ser usado.");

            if (!produtoResultado.Sucess)
                return produtoResultado.Propagar<OrcamentoResponse>();

            Produto produto = produtoResultado.Data!;

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

    public async Task<AppResult<OrcamentoResponse>> CancelarItemAsync(CancelarItemOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        AppResult<OrcamentoItem> itemResultado = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (!itemResultado.Sucess)
            return itemResultado.Propagar<OrcamentoResponse>();

        OrcamentoItem item = itemResultado.Data!;

        if (item.Orci_cancelado)
            return new AppValidation("Item já está cancelado.");

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Orca_id, item.Orci_numero_item, true);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> ReativarItemAsync(ReativarItemOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        AppResult<OrcamentoItem> itemResultado = await ObterItemExistenteAsync(orcamento.Orca_id, request.Orci_numero_item);

        if (!itemResultado.Sucess)
            return itemResultado.Propagar<OrcamentoResponse>();

        OrcamentoItem item = itemResultado.Data!;

        if (!item.Orci_cancelado)
            return new AppValidation("Item não está cancelado.");

        AppResult<Produto> produtoResultado = await ValidarProdutoAsync(item.Prod_codigo, "Produto inativo não pode ser usado.");

        if (!produtoResultado.Sucess)
            return produtoResultado.Propagar<OrcamentoResponse>();

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Orca_id, item.Orci_numero_item, false);
        await ZerarDescontoAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> CancelarAsync(CancelarOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        if (orcamento.Orca_cancelado)
            return new AppValidation("Orçamento já está cancelado.");

        await _repositorio.CancelarAsync(orcamento.Orca_id);
        orcamento.Orca_cancelado = true;

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> FecharAsync(FecharOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ValidarOrcamentoEditavelAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        if (request.Orca_desconto == null)
            return new AppValidation("Desconto é obrigatório.");

        string? erro = OrcamentoValidacao.ValidarDesconto(request.Orca_desconto.Value);

        if (erro != null)
            return new AppValidation(erro);

        List<OrcamentoItem> itens = await _repositorio.ListarItensAsync(orcamento.Orca_id);
        List<OrcamentoItem> ativos = itens.Where(item => !item.Orci_cancelado).ToList();

        if (ativos.Count == 0)
            return new AppValidation("Orçamento sem itens ativos não pode ser fechado.");

        decimal subtotal = ativos.Sum(item => item.Orci_total);

        if (request.Orca_desconto.Value > subtotal)
            return new AppValidation("Desconto não pode ser maior que o subtotal.");

        await _repositorio.DefinirDescontoAsync(orcamento.Orca_id, request.Orca_desconto.Value);
        orcamento.Orca_desconto = request.Orca_desconto.Value;
        await RecalcularTotaisAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> LancarPagamentoAsync(LancarPagamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ObterOrcamentoAbertoAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        if (request.Form_id == null)
            return new AppValidation("Forma de pagamento é obrigatória.");

        if (request.Caix_id == null)
            return new AppValidation("Caixa ou banco é obrigatório.");

        if (request.Orcp_valor == null)
            return new AppValidation("Valor do pagamento é obrigatório.");

        string? erro = OrcamentoValidacao.ValidarValorPagamento(request.Orcp_valor.Value);

        if (erro != null)
            return new AppValidation(erro);

        AppResult<FormaPagamento> formaResultado = await ValidarFormaPagamentoAsync(request.Form_id.Value);

        if (!formaResultado.Sucess)
            return formaResultado.Propagar<OrcamentoResponse>();

        AppResult<CaixaBanco> caixaResultado = await ValidarCaixaBancoAsync(request.Caix_id.Value);

        if (!caixaResultado.Sucess)
            return caixaResultado.Propagar<OrcamentoResponse>();

        FormaPagamento forma = formaResultado.Data!;

        Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.Clie_id);

        if (cliente != null && cliente.Clie_bloqueio == "bloqueado" && !OrcamentoValidacao.FormaPermitidaParaClienteBloqueado(forma.Form_tipo))
            return new AppValidation("Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)");

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
            return new AppValidation("Valor maior que o saldo do orçamento.");

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> CancelarPagamentoAsync(CancelarPagamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        Orcamento orcamento = orcamentoResultado.Data!;

        if (orcamento.Orca_cancelado)
            return new AppValidation("Orçamento cancelado não pode ser alterado.");

        OrcamentoPagamento? pagamento = await _repositorio.ObterPagamentoAsync(orcamento.Orca_id, request.Orcp_numero_pagamento);

        if (pagamento == null)
            return new AppNotFound("Pagamento não encontrado.");

        if (pagamento.Orcp_cancelado)
            return new AppValidation("Pagamento já está cancelado.");

        await _repositorio.CancelarPagamentoAsync(orcamento.Orca_id, pagamento.Orcp_numero_pagamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<AppResult<OrcamentoResponse>> ObterAsync(ObterOrcamentoRequest request)
    {
        AppResult<Orcamento> orcamentoResultado = await ObterOrcamentoExistenteAsync(request.Orca_id);

        if (!orcamentoResultado.Sucess)
            return orcamentoResultado.Propagar<OrcamentoResponse>();

        return await MontarRespostaAsync(orcamentoResultado.Data!);
    }

    public async Task<AppResult<List<OrcamentoResumoResponse>>> ListarAsync(ListarOrcamentosRequest request)
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

    private async Task<AppResult<Orcamento>> ObterOrcamentoExistenteAsync(int id)
    {
        Orcamento? orcamento = await _repositorio.ObterPorIdAsync(id);

        if (orcamento == null)
            return new AppNotFound("Orçamento não encontrado.");

        return orcamento;
    }

    private async Task<AppResult<Orcamento>> ObterOrcamentoAbertoAsync(int id)
    {
        AppResult<Orcamento> resultado = await ObterOrcamentoExistenteAsync(id);

        if (!resultado.Sucess)
            return resultado;

        if (resultado.Data!.Orca_cancelado)
            return new AppValidation("Orçamento cancelado não pode ser alterado.");

        return resultado.Data;
    }

    private async Task<AppResult<Orcamento>> ValidarOrcamentoEditavelAsync(int id)
    {
        AppResult<Orcamento> resultado = await ObterOrcamentoAbertoAsync(id);

        if (!resultado.Sucess)
            return resultado;

        AppResult<bool> semPagamentos = await ValidarSemPagamentosAsync(resultado.Data!.Orca_id);

        if (!semPagamentos.Sucess)
            return semPagamentos.Propagar<Orcamento>();

        return resultado.Data;
    }

    private async Task<AppResult<bool>> ValidarSemPagamentosAsync(int orcamentoId)
    {
        int quantidade = await _repositorio.ContarPagamentosNaoCanceladosAsync(orcamentoId);

        if (quantidade > 0)
            return new AppValidation("Orçamento possui pagamentos não cancelados.");

        return true;
    }

    private async Task<AppResult<OrcamentoItem>> ObterItemExistenteAsync(int orcamentoId, string numeroItem)
    {
        OrcamentoItem? item = await _repositorio.ObterItemAsync(orcamentoId, numeroItem);

        if (item == null)
            return new AppNotFound("Item não encontrado.");

        return item;
    }

    private async Task<AppResult<Cliente>> ValidarClienteAsync(int id)
    {
        Cliente? cliente = await _clientes.ObterPorIdAsync(id);

        if (cliente == null)
            return new AppNotFound("Cliente não encontrado.");

        if (!OrcamentoValidacao.EhAtivo(cliente.Clie_status))
            return new AppValidation("Cliente inativo não pode ser usado.");

        return cliente;
    }

    private async Task<AppResult<Vendedor>> ValidarVendedorAsync(int id)
    {
        Vendedor? vendedor = await _vendedores.ObterPorIdAsync(id);

        if (vendedor == null)
            return new AppNotFound("Vendedor não encontrado.");

        if (!OrcamentoValidacao.EhAtivo(vendedor.Vend_status))
            return new AppValidation("Vendedor inativo não pode ser usado.");

        return vendedor;
    }

    private async Task<AppResult<Produto>> ValidarProdutoAsync(string codigo, string mensagemInativo)
    {
        Produto? produto = await _produtos.ObterPorCodigoAsync(codigo);

        if (produto == null)
            return new AppNotFound("Produto não encontrado.");

        if (!OrcamentoValidacao.EhAtivo(produto.Prod_status))
            return new AppValidation(mensagemInativo);

        return produto;
    }

    private async Task<AppResult<FormaPagamento>> ValidarFormaPagamentoAsync(int id)
    {
        FormaPagamento? forma = await _formasPagamento.ObterPorIdAsync(id);

        if (forma == null)
            return new AppNotFound("Forma de pagamento não encontrada.");

        if (!OrcamentoValidacao.EhAtivo(forma.Form_status))
            return new AppValidation("Forma de pagamento inativa não pode ser usada.");

        return forma;
    }

    private async Task<AppResult<CaixaBanco>> ValidarCaixaBancoAsync(int id)
    {
        CaixaBanco? caixa = await _caixas.ObterPorIdAsync(id);

        if (caixa == null)
            return new AppNotFound("Caixa ou banco não encontrado.");

        if (!OrcamentoValidacao.EhAtivo(caixa.Caix_status))
            return new AppValidation("Caixa ou banco inativo não pode ser usado.");

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
