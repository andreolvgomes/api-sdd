using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S012BloqueioClientesTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S012BloqueioClientesTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Nível padrão (R1) -----

    [Fact]
    public async Task S012_R1_ClienteNovo_NasceLiberado()
    {
        int clienteId = await CriarClienteAsync();

        using var doc = await LerAsync(await _cliente.GetAsync($"/api/clientes/{clienteId}"));

        Assert.Equal("liberado", doc.RootElement.GetProperty("clie_bloqueio").GetString());
    }

    // ----- Alteração do nível (R2 e R3) -----

    [Fact]
    public async Task S012_R2_BloquearCliente_GravaOBloqueio()
    {
        int clienteId = await CriarClienteAsync();

        var resposta = await BloquearAsync(clienteId, "bloqueado");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal("bloqueado", doc.RootElement.GetProperty("clie_bloqueio").GetString());
    }

    [Fact]
    public async Task S012_R2_BloquearCliente_NaoAlteraOsOutrosDadosDoCliente()
    {
        int clienteId = await CriarClienteAsync();

        await BloquearAsync(clienteId, "bloqueado");

        using var doc = await LerAsync(await _cliente.GetAsync($"/api/clientes/{clienteId}"));
        Assert.Equal("Maria Silva", doc.RootElement.GetProperty("clie_nome").GetString());
        Assert.Equal("ativo", doc.RootElement.GetProperty("clie_status").GetString());
    }

    [Fact]
    public async Task S012_R3_BloquearClienteInexistente_NaoEncontradoENaoGrava()
    {
        var resposta = await BloquearAsync(99, "bloqueado");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S012_R3_NivelPausado_Recusado()
    {
        int clienteId = await CriarClienteAsync();

        var resposta = await BloquearAsync(clienteId, "pausado");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nível de bloqueio deve ser liberado ou bloqueado", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await _cliente.GetAsync($"/api/clientes/{clienteId}"));
        Assert.Equal("liberado", doc.RootElement.GetProperty("clie_bloqueio").GetString());
    }

    // ----- Pagamento de cliente liberado (R4) -----

    [Fact]
    public async Task S012_R4_ClienteLiberado_PagaComCartaoDeCredito()
    {
        var (orcamentoId, _, _) = await CriarOrcamentoDoClienteAsync("liberado");
        int formaId = await CriarFormaAsync("Cartão de crédito", 2);
        int caixaId = await CriarCaixaAsync();

        var resposta = await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Pagamento de cliente bloqueado (R5) -----

    [Fact]
    public async Task S012_R5_ClienteBloqueado_PagaComDinheiro()
    {
        var (orcamentoId, _, _) = await CriarOrcamentoDoClienteAsync("bloqueado");
        int formaId = await CriarFormaAsync("Dinheiro", 1);
        int caixaId = await CriarCaixaAsync();

        var resposta = await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task S012_R5_ClienteBloqueado_PagaComPix()
    {
        var (orcamentoId, _, _) = await CriarOrcamentoDoClienteAsync("bloqueado");
        int formaId = await CriarFormaAsync("Pix", 4);
        int caixaId = await CriarCaixaAsync();

        var resposta = await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Pagamento a prazo ou cartão recusado (R6 e R7) -----

    [Theory]
    [InlineData("Cartão de crédito", 2)]
    [InlineData("Cartão de débito", 3)]
    [InlineData("Boleto", 5)]
    [InlineData("Crediário", 6)]
    public async Task S012_R6_ClienteBloqueado_FormaNaoPermitida_RecusadaComValidacaoENaoGrava(string descricao, int tipo)
    {
        var (orcamentoId, _, _) = await CriarOrcamentoDoClienteAsync("bloqueado");
        int formaId = await CriarFormaAsync(descricao, tipo);
        int caixaId = await CriarCaixaAsync();

        var resposta = await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente bloqueado só pode pagar à vista (dinheiro ou Pix)", await TextoErroAsync(resposta));
        Assert.Empty(await PagamentosAsync(orcamentoId));
    }

    // ----- Pagamento lançado antes do bloqueio (R8) -----

    [Fact]
    public async Task S012_R8_PagamentoComCartaoLancadoAntesDoBloqueio_PermaneceGravado()
    {
        var (orcamentoId, clienteId, _) = await CriarOrcamentoDoClienteAsync("liberado");
        int formaId = await CriarFormaAsync("Cartão de crédito", 2);
        int caixaId = await CriarCaixaAsync();
        await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);

        await BloquearAsync(clienteId, "bloqueado");

        var pagamentos = await PagamentosAsync(orcamentoId);
        Assert.Single(pagamentos);
        Assert.Equal(50.00m, pagamentos[0].GetProperty("orcp_valor").GetDecimal());
        Assert.False(pagamentos[0].GetProperty("orcp_cancelado").GetBoolean());
    }

    // ----- Regra só vale para pagamento (R9) -----

    [Fact]
    public async Task S012_R9_ClienteBloqueado_AlteraODescontoNormalmente()
    {
        var (orcamentoId, _, _) = await CriarOrcamentoDoClienteAsync("bloqueado");

        var resposta = await _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/fechar", new { Orca_desconto = 10.00m });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Desbloqueio (R10) -----

    [Fact]
    public async Task S012_R10_ClienteDesbloqueado_PagaComCartaoDeCredito()
    {
        var (orcamentoId, clienteId, _) = await CriarOrcamentoDoClienteAsync("bloqueado");
        int formaId = await CriarFormaAsync("Cartão de crédito", 2);
        int caixaId = await CriarCaixaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m)).StatusCode);

        await BloquearAsync(clienteId, "liberado");

        var resposta = await LancarPagamentoAsync(orcamentoId, formaId, caixaId, 50.00m);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Auxiliares -----

    private async Task<(int OrcamentoId, int ClienteId, int VendedorId)> CriarOrcamentoDoClienteAsync(string nivel)
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 100.00m);

        if (nivel != "liberado")
            await BloquearAsync(clienteId, nivel);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id = clienteId, Vend_id = vendedorId, Prod_codigo = "P001", Orci_quantidade = 1 });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        return (doc.RootElement.GetProperty("orca_id").GetInt32(), clienteId, vendedorId);
    }

    private async Task<int> CriarClienteAsync()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { Clie_nome = "Maria Silva", Clie_endereco = "Rua A, 1", Clie_telefone = "11 90000-0001", Clie_status = "ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("clie_id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { Vend_nome = "João Lima", Vend_status = "Ativo" });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("vend_id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/produtos", new { Prod_codigo = codigo, Prod_descricao = "Produto " + codigo, Prod_precovenda = precoVenda, Prod_status = "ativo" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private async Task<int> CriarFormaAsync(string descricao, int tipo)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/formas-pagamento", new { Form_descricao = descricao, Form_tipo = tipo, Form_status = "Ativo" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("form_id").GetInt32();
    }

    private async Task<int> CriarCaixaAsync()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/caixas-bancos", new { Caix_descricao = "Caixa geral", Caix_status = "Ativo" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("caix_id").GetInt32();
    }

    private Task<HttpResponseMessage> BloquearAsync(int clienteId, string nivel)
    {
        return _cliente.PutAsJsonAsync($"/api/clientes/{clienteId}/bloqueio", new { Clie_bloqueio = nivel });
    }

    private Task<HttpResponseMessage> LancarPagamentoAsync(int orcamentoId, int formaId, int caixaId, decimal valor)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/pagamentos", new { Form_id = formaId, Caix_id = caixaId, Orcp_valor = valor });
    }

    private async Task<List<JsonElement>> PagamentosAsync(int orcamentoId)
    {
        using var doc = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}"));
        return doc.RootElement.GetProperty("pagamentos").EnumerateArray().Select(p => p.Clone()).ToList();
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
