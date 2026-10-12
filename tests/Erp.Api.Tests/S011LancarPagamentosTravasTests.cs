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
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(8.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    [Fact]
    public async Task S011_R2_PagamentoTemFormaCaixaEValor()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Pix", 4, "Ativo");
        int caixaId = await CriarCaixaAsync("Banco - conta corrente", "Ativo");

        using var doc = await LerAsync(await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 5.00m }));

        JsonElement pagamento = Pagamento(doc, "001");
        Assert.Equal(formaId, pagamento.GetProperty("formaPagamentoId").GetInt32());
        Assert.Equal(caixaId, pagamento.GetProperty("caixaBancoId").GetInt32());
        Assert.Equal(5.00m, pagamento.GetProperty("valor").GetDecimal());
    }

    // ----- Forma de pagamento (R3) -----

    [Fact]
    public async Task S011_R3_FormaInexistente_RecusadaENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = 99, caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    [Fact]
    public async Task S011_R3_FormaInativa_RecusadaENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Crediário", 6, "Inativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Forma de pagamento inativa não pode ser usada.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    // ----- Caixa ou banco (R4) -----

    [Fact]
    public async Task S011_R4_CaixaInexistente_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = 99, valor = 5.00m });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    [Fact]
    public async Task S011_R4_CaixaInativo_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Banco - conta poupança", "Inativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Caixa ou banco inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    // ----- Valor do pagamento (R5) -----

    [Fact]
    public async Task S011_R5_ValorZero_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ser maior que zero.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    [Fact]
    public async Task S011_R5_ValorNegativo_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = -2m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ser maior que zero.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R5_ValorComTresCasas_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 1.234m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    // ----- Valor acima do saldo (R6) e soma exata (R7) -----

    [Fact]
    public async Task S011_R6_ValorMaiorQueSaldo_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await LancarPagamentoPadraoAsync(orcamentoId, 9.00m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor maior que o saldo do orçamento.", await TextoErroAsync(resposta));
        Assert.Equal(8.50m, await SaldoAsync(orcamentoId));
    }

    [Fact]
    public async Task S011_R7_SomaPodeChegarExatamenteAoTotal()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await LancarPagamentoPadraoAsync(orcamentoId, 8.50m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(0m, await SaldoAsync(orcamentoId));
    }

    // ----- Pagamentos simultâneos (R8) -----

    [Fact]
    public async Task S011_R8_DoisPagamentosSimultaneos_SegundoQueNaoCabeETRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var tarefas = Enumerable.Range(0, 2).Select(_ => LancarPagamentoPadraoAsync(orcamentoId, 8.50m));
        var respostas = await Task.WhenAll(tarefas);

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal(0m, await SaldoAsync(orcamentoId));
    }

    // ----- Saldo (R9) e consulta (R17) -----

    [Fact]
    public async Task S011_R9_R17_ConsultaMostraPagamentosESaldoSemOsCancelados()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        await CancelarPagamentoAsync(orcamentoId, "001");
        await LancarPagamentoPadraoAsync(orcamentoId, 3.00m);

        using var doc = await LerAsync(await ObterAsync(orcamentoId));

        Assert.Equal(2, doc.RootElement.GetProperty("pagamentos").GetArrayLength());
        Assert.True(Pagamento(doc, "001").GetProperty("cancelado").GetBoolean());
        Assert.False(Pagamento(doc, "002").GetProperty("cancelado").GetBoolean());
        Assert.Equal(10.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    // ----- Travas enquanto houver pagamento não cancelado (R10) -----

    [Fact]
    public async Task S011_R10_ComPagamento_IncluirItemRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await IncluirItemAsync(orcamentoId, "P002", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarItemRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { quantidade = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_CancelarItemRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await CancelarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_ReativarItemRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await ReativarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarClienteRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        int clienteNovo = await CriarClienteAsync("Ana Lima");

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { clienteId = clienteNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal("Maria Silva", doc.RootElement.GetProperty("cliente").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task S011_R10_ComPagamento_AlterarVendedorRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { vendedorId = vendedorNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R10_ComPagamento_FecharComDescontoRecusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await FecharAsync(orcamentoId, new { desconto = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento possui pagamentos não cancelados.", await TextoErroAsync(resposta));
    }

    // ----- Pagamentos todos cancelados liberam as travas (R11) -----

    [Fact]
    public async Task S011_R11_TodosPagamentosCancelados_DestravaAsEdicoes()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        await CancelarPagamentoAsync(orcamentoId, "001");

        var resposta = await IncluirItemAsync(orcamentoId, "P002", 1);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Cancelar pagamento (R12) e já cancelado (R13) -----

    [Fact]
    public async Task S011_R12_CancelarPagamento_GravaCanceladoSemExcluir()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await CancelarPagamentoAsync(orcamentoId, "001");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(Pagamento(doc, "001").GetProperty("cancelado").GetBoolean());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    [Fact]
    public async Task S011_R13_CancelarPagamentoJaCancelado_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        await CancelarPagamentoAsync(orcamentoId, "001");

        var resposta = await CancelarPagamentoAsync(orcamentoId, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Pagamento já está cancelado.", await TextoErroAsync(resposta));
    }

    // ----- Orçamento cancelado congela pagamentos (R14) -----

    [Fact]
    public async Task S011_R14_OrcamentoCancelado_NaoAceitaLancamentoDePagamento()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarOrcamentoAsync(orcamentoId);

        var resposta = await LancarPagamentoPadraoAsync(orcamentoId, 1.00m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S011_R14_OrcamentoCancelado_NaoAceitaCancelamentoDePagamento()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);
        await CancelarOrcamentoAsync(orcamentoId);

        var resposta = await CancelarPagamentoAsync(orcamentoId, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.False(Pagamento(doc, "001").GetProperty("cancelado").GetBoolean());
    }

    // ----- Cancelar orçamento com pagamento não cancelado (R15) -----

    [Fact]
    public async Task S011_R15_CancelarOrcamentoComPagamentoNaoCancelado_Permitido()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        var resposta = await CancelarOrcamentoAsync(orcamentoId);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(doc.RootElement.GetProperty("cancelado").GetBoolean());
        Assert.False(Pagamento(doc, "001").GetProperty("cancelado").GetBoolean());
        Assert.Equal(8.50m, doc.RootElement.GetProperty("saldo").GetDecimal());
    }

    // ----- Caixa ou banco é só referência (R16) -----

    [Fact]
    public async Task S011_R16_LancarPagamento_NaoMovimentaCaixaOuBanco()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        string antes = await TextoAsync($"/api/caixas-bancos/{caixaId}");

        await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(antes, await TextoAsync($"/api/caixas-bancos/{caixaId}"));
    }

    // ----- Número do pagamento (R20) -----

    [Fact]
    public async Task S011_R20_NumeroDoPagamentoEhSequencial()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await LancarPagamentoPadraoAsync(orcamentoId, 5.00m);

        using var doc = await LerAsync(await LancarPagamentoPadraoAsync(orcamentoId, 3.00m));

        Assert.Equal("002", doc.RootElement.GetProperty("pagamentos")[1].GetProperty("numeroPagamento").GetString());
    }

    // ----- Valor obrigatório (R18) -----

    [Fact]
    public async Task S011_R18_ValorAusente_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, caixaBancoId = caixaId });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Valor do pagamento é obrigatório.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    // ----- Forma e caixa obrigatórios (R19) -----

    [Fact]
    public async Task S011_R19_FormaAusente_RecusadaENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int caixaId = await CriarCaixaAsync("Caixa geral", "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { caixaBancoId = caixaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Forma de pagamento é obrigatória.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    [Fact]
    public async Task S011_R19_CaixaAusente_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int formaId = await CriarFormaAsync("Dinheiro", 1, "Ativo");

        var resposta = await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = formaId, valor = 5.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Caixa ou banco é obrigatório.", await TextoErroAsync(resposta));
        Assert.Equal(13.50m, await SaldoAsync(orcamentoId));
    }

    // ----- Pagamento inexistente (R21) -----

    [Fact]
    public async Task S011_R21_CancelarPagamentoInexistente_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await CancelarPagamentoAsync(orcamentoId, "099");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Pagamento não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    // Orçamento 1 com total 13.50: 001 = P001 x2 (7.00), 002 = P002 x3 (3.00), 003 = P001 x1 (3.50).
    private async Task<int> CriarOrcamentoComTresItensAsync()
    {
        int clienteId = await CriarClienteAsync("Maria Silva");
        int vendedorId = await CriarVendedorAsync("João Lima");
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, codigoProduto = "P001", quantidade = 2 }));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        await IncluirItemAsync(orcamentoId, "P002", 3);
        await IncluirItemAsync(orcamentoId, "P001", 1);
        return orcamentoId;
    }

    // Forma e caixa padrão são criadas uma vez por teste, porque o cadastro não aceita descrição repetida.
    private async Task<HttpResponseMessage> LancarPagamentoPadraoAsync(int orcamentoId, decimal valor)
    {
        _formaPadraoId ??= await CriarFormaAsync("Dinheiro", 1, "Ativo");
        _caixaPadraoId ??= await CriarCaixaAsync("Caixa geral", "Ativo");

        return await LancarPagamentoAsync(orcamentoId, new { formaPagamentoId = _formaPadraoId, caixaBancoId = _caixaPadraoId, valor });
    }

    private async Task<decimal> SaldoAsync(int orcamentoId)
    {
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        return doc.RootElement.GetProperty("saldo").GetDecimal();
    }

    private async Task<int> CriarClienteAsync(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { nome, endereco = "Rua A, 1", telefone = "11 90000-0001", status = "ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { nome, status = "Ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarFormaAsync(string descricao, int tipo, string status)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/formas-pagamento", new { descricao, tipo, status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarCaixaAsync(string descricao, string status)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/caixas-bancos", new { descricao, status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda)
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { codigo, descricao = "Produto " + codigo, precoVenda, status = "ativo" });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int orcamentoId, string codigoProduto, decimal quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens", new { codigoProduto, quantidade });
    }

    private Task<HttpResponseMessage> AlterarItemAsync(int orcamentoId, string numeroItem, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}", corpo);
    }

    private Task<HttpResponseMessage> CancelarItemAsync(int orcamentoId, string numeroItem)
    {
        return _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}/cancelar", null);
    }

    private Task<HttpResponseMessage> ReativarItemAsync(int orcamentoId, string numeroItem)
    {
        return _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}/reativar", null);
    }

    private Task<HttpResponseMessage> AlterarOrcamentoAsync(int orcamentoId, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{orcamentoId}", corpo);
    }

    private Task<HttpResponseMessage> FecharAsync(int orcamentoId, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/fechar", corpo);
    }

    private Task<HttpResponseMessage> CancelarOrcamentoAsync(int orcamentoId)
    {
        return _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/cancelar", null);
    }

    private Task<HttpResponseMessage> LancarPagamentoAsync(int orcamentoId, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/pagamentos", corpo);
    }

    private Task<HttpResponseMessage> CancelarPagamentoAsync(int orcamentoId, string numeroPagamento)
    {
        return _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/pagamentos/{numeroPagamento}/cancelar", null);
    }

    private Task<HttpResponseMessage> ObterAsync(int orcamentoId)
    {
        return _cliente.GetAsync($"/api/orcamentos/{orcamentoId}");
    }

    private async Task<string> TextoAsync(string url)
    {
        return await (await _cliente.GetAsync(url)).Content.ReadAsStringAsync();
    }

    private static JsonElement Pagamento(JsonDocument doc, string numeroPagamento)
    {
        foreach (JsonElement pagamento in doc.RootElement.GetProperty("pagamentos").EnumerateArray())
            if (pagamento.GetProperty("numeroPagamento").GetString() == numeroPagamento)
                return pagamento.Clone();

        throw new InvalidOperationException($"Pagamento {numeroPagamento} não encontrado na resposta.");
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
