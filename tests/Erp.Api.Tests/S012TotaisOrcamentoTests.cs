using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S012TotaisOrcamentoTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S012TotaisOrcamentoTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Totais guardados acompanham itens (R1) -----

    [Fact]
    public async Task S012_R1_TotaisGuardadosBatemComOsItens_AoIncluirAlterarCancelarEReativar()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 10.00m);
        await CriarProdutoAsync("P002", 2.50m);

        int orcamentoId = await CriarOrcamentoAsync(clienteId, vendedorId, "P001", 1);
        await ConferirTotaisAsync(orcamentoId, "criado");

        await IncluirItemAsync(orcamentoId, "P002", 2);
        await ConferirTotaisAsync(orcamentoId, "item incluido");

        await AlterarItemAsync(orcamentoId, "002", 4);
        await ConferirTotaisAsync(orcamentoId, "item alterado");

        await CancelarItemAsync(orcamentoId, "001");
        await ConferirTotaisAsync(orcamentoId, "item cancelado");

        await ReativarItemAsync(orcamentoId, "001");
        await ConferirTotaisAsync(orcamentoId, "item reativado");
    }

    [Fact]
    public async Task S012_R2_TotalComDescontoBateComOsItensMenosODesconto_AposFechar()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 10.00m);
        await CriarProdutoAsync("P002", 2.50m);

        int orcamentoId = await CriarOrcamentoAsync(clienteId, vendedorId, "P001", 1);
        await IncluirItemAsync(orcamentoId, "P002", 2);

        await FecharAsync(orcamentoId, 5.00m);
        using var fechado = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal(10.00m, fechado.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
        await ConferirTotaisAsync(orcamentoId, "fechado com desconto");

        await IncluirItemAsync(orcamentoId, "P002", 1);
        await ConferirTotaisAsync(orcamentoId, "item incluido depois do fechamento");
    }

    // ----- Auxiliares -----

    private async Task ConferirTotaisAsync(int orcamentoId, string momento)
    {
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        JsonElement raiz = doc.RootElement;

        decimal somaDosItensAtivos = 0m;

        foreach (JsonElement item in raiz.GetProperty("itens").EnumerateArray())
            if (!item.GetProperty("orci_cancelado").GetBoolean())
                somaDosItensAtivos += item.GetProperty("orci_total").GetDecimal();

        decimal desconto = raiz.GetProperty("orca_desconto").GetDecimal();

        Assert.True(somaDosItensAtivos == raiz.GetProperty("orca_total").GetDecimal(), $"orca_total diferente da soma dos itens ({momento})");
        Assert.True(somaDosItensAtivos - desconto == raiz.GetProperty("orca_total_com_desconto").GetDecimal(), $"orca_total_com_desconto diferente do esperado ({momento})");
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

    private async Task<int> CriarOrcamentoAsync(int clienteId, int vendedorId, string codigo, decimal quantidade)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id = clienteId, Vend_id = vendedorId, Prod_codigo = codigo, Orci_quantidade = quantidade });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("orca_id").GetInt32();
    }

    private async Task IncluirItemAsync(int orcamentoId, string codigo, decimal quantidade)
    {
        var resposta = await _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens", new { Prod_codigo = codigo, Orci_quantidade = quantidade });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private async Task AlterarItemAsync(int orcamentoId, string numeroItem, decimal quantidade)
    {
        var resposta = await _cliente.PutAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}", new { Orci_quantidade = quantidade });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    private async Task CancelarItemAsync(int orcamentoId, string numeroItem)
    {
        var resposta = await _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}/cancelar", null);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    private async Task ReativarItemAsync(int orcamentoId, string numeroItem)
    {
        var resposta = await _cliente.PostAsync($"/api/orcamentos/{orcamentoId}/itens/{numeroItem}/reativar", null);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    private async Task FecharAsync(int orcamentoId, decimal desconto)
    {
        var resposta = await _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/fechar", new { Orca_desconto = desconto });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> ObterAsync(int orcamentoId)
    {
        return _cliente.GetAsync($"/api/orcamentos/{orcamentoId}");
    }

    private static async Task<JsonDocument> LerAsync(HttpResponseMessage resposta)
    {
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }
}
