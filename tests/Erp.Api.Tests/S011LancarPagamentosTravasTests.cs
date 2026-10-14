using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S011LancarPagamentosTravasTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;
    private int? _formaPadraoId;
    private int? _caixaPadraoId;

    public S011LancarPagamentosTravasTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Lançamento a qualquer momento (R1) e campos do pagamento (R2) -----

    [Fact]
    public async Task S011_R1_LancarPagamento_DepoisDoOrcamentoExistir_AceitoESaldoAbate()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(8.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    [Fact]
    public async Task S011_R2_PagamentoTemFormaCaixaEValor()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Pix", 4, "Ativo");
        int caixaId = await CriarCaixaAsync("Banco - conta corrente", "Ativo");

        using var doc = await LerAsync(await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 5.00m }));

        JsonElement pagamento = Pagamento(doc, "001");
        Assert.Equal(formaId, pagamento.GetProperty("form_id").GetInt32());
        Assert.Equal(caixaId, pagamento.GetProperty("caix_id").GetInt32());
        Assert.Equal(5.00m, pagamento.GetProperty("orcp_valor").GetDecimal());
    }

    // ----- Forma de pagamento (R3) -----

    [Fact]
    public async Task S011_R3_FormaInexistente_RecusadaENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = 99, Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    [Fact]
    public async Task S011_R3_FormaInativa_RecusadaENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Crediário", 6, "Inativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Forma de pagamento inativa não pode ser usada.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    // ----- Caixa ou banco (R4) -----

    [Fact]
    public async Task S011_R4_CaixaInexistente_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = 99, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    [Fact]
    public async Task S011_R4_CaixaInativo_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Banco - conta poupança", "Inativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Caixa ou banco inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    // ----- Valor do pagamento (R5) -----

    [Fact]
    public async Task S011_R5_ValorZero_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ser maior que zero.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    [Fact]
    public async Task S011_R5_ValorNegativo_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = -2m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ser maior que zero.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R5_ValorComTresCasas_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 1.234m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    // ----- Valor acima do saldo (R6) e soma exata (R7) -----

    [Fact]
    public async Task S011_R6_ValorMaiorQueSaldo_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await LancarPagamentoPadraoAsync(Orca_id, 9.00m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor maior que o saldo do orçamento.", await TextoErroAsync(resposta));
        Assert.Equal(8.50m, await SaldoAsync(Orca_id));
    }

    [Fact]
    public async Task S011_R7_SomaPodeChegarExatamenteAoTotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await LancarPagamentoPadraoAsync(Orca_id, 8.50m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(0m, await SaldoAsync(Orca_id));
    }

    // ----- Pagamentos simultâneos (R8) -----

    [Fact]
    public async Task S011_R8_DoisPagamentosSimultaneos_SegundoQueNaoCabeETRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var tarefas = Enumerable.Range(0, 2).Select(_ => LancarPagamentoPadraoAsync(Orca_id, 8.50m));
        var respostas = await Task.WhenAll(tarefas);

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal(0m, await SaldoAsync(Orca_id));
    }

    // ----- Saldo (R9) e consulta (R17) -----

    [Fact]
    public async Task S011_R9_R17_ConsultaMostraPagamentosESaldoSemOsCancelados()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        await CancelarPagamentoAsync(Orca_id, "001");
        await LancarPagamentoPadraoAsync(Orca_id, 3.00m);

        using var doc = await LerAsync(await ObterAsync(Orca_id));

        Assert.Equal(2, doc.RootElement.GetProperty("pagamentos").GetArrayLength());
        Assert.True(Pagamento(doc, "001").GetProperty("orcp_cancelado").GetBoolean());
        Assert.False(Pagamento(doc, "002").GetProperty("orcp_cancelado").GetBoolean());
        Assert.Equal(10.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    // ----- Travas enquanto houver pagamento não cancelado (R10) -----

    [Fact]
    public async Task S011_R10_ComPagamento_IncluirItemRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await IncluirItemAsync(Orca_id, "P002", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarItemRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_CancelarItemRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await CancelarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_ReativarItemRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await ReativarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarClienteRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        int clienteNovo = await CriarClienteAsync("Ana Lima");

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Clie_id = clienteNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal("Maria Silva", doc.RootElement.GetProperty("cliente").GetProperty("clie_nome").GetString());
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarVendedorRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Vend_id = vendedorNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_FecharComDescontoRecusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    // ----- Pagamentos todos cancelados liberam as travas (R11) -----

    [Fact]
    public async Task S011_R11_TodosPagamentosCancelados_DestravaAsEdicoes()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        await CancelarPagamentoAsync(Orca_id, "001");

        var resposta = await IncluirItemAsync(Orca_id, "P002", 1);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Cancelar pagamento (R12) e já cancelado (R13) -----

    [Fact]
    public async Task S011_R12_CancelarPagamento_GravaCanceladoSemExcluir()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await CancelarPagamentoAsync(Orca_id, "001");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(Pagamento(doc, "001").GetProperty("orcp_cancelado").GetBoolean());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    [Fact]
    public async Task S011_R13_CancelarPagamentoJaCancelado_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        await CancelarPagamentoAsync(Orca_id, "001");

        var resposta = await CancelarPagamentoAsync(Orca_id, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Pagamento já está cancelado.", await TextoErroAsync(resposta));
    }

    // ----- Orçamento cancelado congela pagamentos (R14) -----

    [Fact]
    public async Task S011_R14_OrcamentoCancelado_NaoAceitaLancamentoDePagamento()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await LancarPagamentoPadraoAsync(Orca_id, 1.00m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R14_OrcamentoCancelado_NaoAceitaCancelamentoDePagamento()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await CancelarPagamentoAsync(Orca_id, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.False(Pagamento(doc, "001").GetProperty("orcp_cancelado").GetBoolean());
    }

    // ----- Cancelar orçamento com pagamento não cancelado (R15) -----

    [Fact]
    public async Task S011_R15_CancelarOrcamentoComPagamentoNaoCancelado_Permitido()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        var resposta = await CancelarOrcamentoAsync(Orca_id);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(doc.RootElement.GetProperty("orca_cancelado").GetBoolean());
        Assert.False(Pagamento(doc, "001").GetProperty("orcp_cancelado").GetBoolean());
        Assert.Equal(8.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    // ----- Caixa ou banco é só referência (R16) -----

    [Fact]
    public async Task S011_R16_LancarPagamento_NaoMovimentaCaixaOuBanco()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        string antes = await TextoAsync($"/api/caixas-bancos/{caixaId}");

        await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(antes, await TextoAsync($"/api/caixas-bancos/{caixaId}"));
    }

    // ----- Número do pagamento (R20) -----

    [Fact]
    public async Task S011_R20_NumeroDoPagamentoEhSequencial()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(Orca_id, 5.00m);

        using var doc = await LerAsync(await LancarPagamentoPadraoAsync(Orca_id, 3.00m));

        Assert.Equal("002", doc.RootElement.GetProperty("pagamentos")[1].GetProperty("orcp_numero_pagamento").GetString());
    }

    // ----- Valor obrigatório (R18) -----

    [Fact]
    public async Task S011_R18_ValorAusente_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Caix_id = caixaId });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento é obrigatório.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    // ----- Forma e caixa obrigatórios (R19) -----

    [Fact]
    public async Task S011_R19_FormaAusente_RecusadaENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Caix_id = caixaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Forma de pagamento é obrigatória.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    [Fact]
    public async Task S011_R19_CaixaAusente_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");

        var resposta = await LancarPagamentoAsync(Orca_id, new { Form_id = formaId, Orcp_valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Caixa ou banco é obrigatório.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(Orca_id));
    }

    // ----- Pagamento inexistente (R21) -----

    [Fact]
    public async Task S011_R21_CancelarPagamentoInexistente_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await CancelarPagamentoAsync(Orca_id, "099");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Pagamento não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    // Orçamento 1 com total 13.50: 001 = P001 x2 (7.00), 002 = P002 x3 (3.00), 003 = P001 x1 (3.50).
    private async Task<int> CriarOrcamentoComTresItensAsync()
    {
        int Clie_id = await CriarClienteAsync("Maria Silva");
        int Vend_id = await CriarVendedorAsync("João Lima");
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo = "P001", Orci_quantidade = 2 }));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await IncluirItemAsync(Orca_id, "P002", 3);
        await IncluirItemAsync(Orca_id, "P001", 1);
        return Orca_id;
    }

    // Forma e caixa padrão são criadas uma vez por teste, porque o cadastro não aceita descrição repetida.
    private async Task<HttpResponseMessage> LancarPagamentoPadraoAsync(int Orca_id, decimal Orcp_valor)
    {
        _formaPadraoId ??= await CriarFormaAsync("Dinheiro", 1, "Ativo");
        _caixaPadraoId ??= await CriarCaixaAsync("Caixa geral", "Ativo");

        return await LancarPagamentoAsync(Orca_id, new { Form_id = _formaPadraoId, Caix_id = _caixaPadraoId, Orcp_valor });
    }

    private async Task<decimal> SaldoAsync(int Orca_id)
    {
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        return doc.RootElement.GetProperty("saldo").GetDecimal();
    }

    private async Task<int> CriarClienteAsync(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { Clie_nome = nome, Clie_endereco = "Rua A, 1", Clie_telefone = "11 90000-0001", Clie_status = "ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("clie_id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { Vend_nome = nome, Vend_status = "Ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("vend_id").GetInt32();
    }

    private async Task<int> CriarFormaAsync(string descricao, int tipo, string status)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/formas-pagamento", new { Form_descricao = descricao, Form_tipo = tipo, Form_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("form_id").GetInt32();
    }

    private async Task<int> CriarCaixaAsync(string descricao, string status)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/caixas-bancos", new { Caix_descricao = descricao, Caix_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("caix_id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda)
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { Prod_codigo = codigo, Prod_descricao = "Produto " + codigo, Prod_precovenda = precoVenda, Prod_status = "ativo" });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int Orca_id, string Prod_codigo, decimal Orci_quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/itens", new { Prod_codigo, Orci_quantidade });
    }

    private Task<HttpResponseMessage> AlterarItemAsync(int Orca_id, string Orci_numero_item, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{Orca_id}/itens/{Orci_numero_item}", corpo);
    }

    private Task<HttpResponseMessage> CancelarItemAsync(int Orca_id, string Orci_numero_item)
    {
        return _cliente.PostAsync($"/api/orcamentos/{Orca_id}/itens/{Orci_numero_item}/cancelar", null);
    }

    private Task<HttpResponseMessage> ReativarItemAsync(int Orca_id, string Orci_numero_item)
    {
        return _cliente.PostAsync($"/api/orcamentos/{Orca_id}/itens/{Orci_numero_item}/reativar", null);
    }

    private Task<HttpResponseMessage> AlterarOrcamentoAsync(int Orca_id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{Orca_id}", corpo);
    }

    private Task<HttpResponseMessage> FecharAsync(int Orca_id, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/fechar", corpo);
    }

    private Task<HttpResponseMessage> CancelarOrcamentoAsync(int Orca_id)
    {
        return _cliente.PostAsync($"/api/orcamentos/{Orca_id}/cancelar", null);
    }

    private Task<HttpResponseMessage> LancarPagamentoAsync(int Orca_id, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/pagamentos", corpo);
    }

    private Task<HttpResponseMessage> CancelarPagamentoAsync(int Orca_id, string Orcp_numero_pagamento)
    {
        return _cliente.PostAsync($"/api/orcamentos/{Orca_id}/pagamentos/{Orcp_numero_pagamento}/cancelar", null);
    }

    private Task<HttpResponseMessage> ObterAsync(int Orca_id)
    {
        return _cliente.GetAsync($"/api/orcamentos/{Orca_id}");
    }

    private async Task<string> TextoAsync(string url)
    {
        return await (await _cliente.GetAsync(url)).Content.ReadAsStringAsync();
    }

    private static JsonElement Pagamento(JsonDocument doc, string Orcp_numero_pagamento)
    {
        foreach (JsonElement pagamento in doc.RootElement.GetProperty("pagamentos").EnumerateArray())
            if (pagamento.GetProperty("orcp_numero_pagamento").GetString() == Orcp_numero_pagamento)
                return pagamento.Clone();

        throw new InvalidOperationException($"Pagamento {Orcp_numero_pagamento} não encontrado na resposta.");
    }

    private static async Task<JsonDocument> LerAsync(HttpResponseMessage resposta)
    {
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private static string ReunirTextos(JsonElement elemento)
    {
        if (elemento.ValueKind == JsonValueKind.String)
            return elemento.GetString() ?? "";

        string texto = "";

        if (elemento.ValueKind == JsonValueKind.Object)
            foreach (JsonProperty propriedade in elemento.EnumerateObject())
                texto += " " + ReunirTextos(propriedade.Value);

        if (elemento.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in elemento.EnumerateArray())
                texto += " " + ReunirTextos(item);

        return texto;
    }
}
