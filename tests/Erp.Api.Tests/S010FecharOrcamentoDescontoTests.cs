using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S010FecharOrcamentoDescontoTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S010FecharOrcamentoDescontoTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Gravar Orca_desconto no total (R1, R3) -----

    [Fact]
    public async Task S010_R1_FecharComDesconto_GravaDescontoNoOrcamento()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(1.50m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
    }

    [Fact]
    public async Task S010_R3_TotalEhSubtotalMenosDesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await FecharAsync(Orca_id, new { Orca_desconto = 1.50m }));

        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
        Assert.Equal(12.00m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    // ----- Desconto aplicado ao total, não aos itens (R2) -----

    [Fact]
    public async Task S010_R2_DescontoAplicadoAoTotal_ItensFicamComOsMesmosTotais()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await FecharAsync(Orca_id, new { Orca_desconto = 1.50m }));

        Assert.Equal(7.00m, Item(doc, "001").GetProperty("orci_total").GetDecimal());
        Assert.Equal(3.00m, Item(doc, "002").GetProperty("orci_total").GetDecimal());
        Assert.Equal(3.50m, Item(doc, "003").GetProperty("orci_total").GetDecimal());
    }

    // ----- Outro Orca_desconto substitui o anterior (R4) -----

    [Fact]
    public async Task S010_R4_OutroDesconto_SubstituiOAnterior()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await FecharAsync(Orca_id, new { Orca_desconto = 2.00m }));

        Assert.Equal(2.00m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(11.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    [Fact]
    public async Task S010_R4_DescontoZero_RemoveODesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await FecharAsync(Orca_id, new { Orca_desconto = 0 }));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    // ----- Desconto negativo (R5) -----

    [Fact]
    public async Task S010_R5_DescontoNegativo_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = -1m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Desconto não pode ser negativo.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
    }

    // ----- Desconto maior que o subtotal (R6) -----

    [Fact]
    public async Task S010_R6_DescontoMaiorQueSubtotal_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 14.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Desconto não pode ser maior que o subtotal.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
    }

    // ----- Desconto com mais de 2 casas (R7) -----

    [Fact]
    public async Task S010_R7_DescontoComTresCasas_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 1.234m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Desconto deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
    }

    // ----- Orçamento sem itens ativos (R8) -----

    [Fact]
    public async Task S010_R8_OrcamentoComTodosItensCancelados_NaoFecha()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "001");
        await CancelarItemAsync(Orca_id, "002");
        await CancelarItemAsync(Orca_id, "003");

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento sem itens ativos não pode ser fechado.", await TextoErroAsync(resposta));
    }

    // ----- Orçamento cancelado (R9) -----

    [Fact]
    public async Task S010_R9_OrcamentoCancelado_NaoFecha()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await _cliente.PostAsync($"/api/orcamentos/{Orca_id}/cancelar", null);

        var resposta = await FecharAsync(Orca_id, new { Orca_desconto = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    // ----- Alterar itens refaz os totais e zera o Orca_desconto (R10) -----

    [Fact]
    public async Task S010_R10_IncluirItem_ZeraODesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await IncluirItemAsync(Orca_id, "P002", 1));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(14.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
        Assert.Equal(14.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    [Fact]
    public async Task S010_R10_AlterarItem_ZeraODesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 }));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(15.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    [Fact]
    public async Task S010_R10_CancelarItem_ZeraODesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await CancelarItemAsync(Orca_id, "002"));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(10.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
        Assert.Equal(10.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    [Fact]
    public async Task S010_R10_ReativarItem_ZeraODesconto()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        using var doc = await LerAsync(await ReativarItemAsync(Orca_id, "002"));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    // ----- Fechamento não trava alteração de itens (R11) -----

    [Fact]
    public async Task S010_R11_OrcamentoFechado_AceitaAlteracaoDeItem()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await FecharAsync(Orca_id, new { Orca_desconto = 1.50m });

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Consulta mostra Orca_desconto e total (R12) -----

    [Fact]
    public async Task S010_R12_ConsultaSemDescontoGravado_MostraZeroETotalIgualSubtotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await ObterAsync(Orca_id));

        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total_com_desconto").GetDecimal());
    }

    // ----- Desconto obrigatório (R13) -----

    [Fact]
    public async Task S010_R13_FecharSemDesconto_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await FecharAsync(Orca_id, new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Desconto é obrigatório.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(0m, doc.RootElement.GetProperty("orca_desconto").GetDecimal());
    }

    // ----- Auxiliares -----

    // Orçamento 1 com subtotal 13.50: 001 = P001 x2 (7.00), 002 = P002 x3 (3.00), 003 = P001 x1 (3.50).
    private async Task<int> CriarOrcamentoComTresItensAsync()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo = "P001", Orci_quantidade = 2 }));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await IncluirItemAsync(Orca_id, "P002", 3);
        await IncluirItemAsync(Orca_id, "P001", 1);
        return Orca_id;
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
        await _cliente.PostAsJsonAsync("/api/produtos", new { Prod_codigo = codigo, Prod_descricao = "Produto " + codigo, Prod_precovenda = precoVenda, Prod_status = "ativo" });
    }

    private Task<HttpResponseMessage> FecharAsync(int Orca_id, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/fechar", corpo);
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

    private Task<HttpResponseMessage> ObterAsync(int Orca_id)
    {
        return _cliente.GetAsync($"/api/orcamentos/{Orca_id}");
    }

    private static JsonElement Item(JsonDocument doc, string Orci_numero_item)
    {
        foreach (JsonElement item in doc.RootElement.GetProperty("itens").EnumerateArray())
            if (item.GetProperty("orci_numero_item").GetString() == Orci_numero_item)
                return item.Clone();

        throw new InvalidOperationException($"Item {Orci_numero_item} não encontrado na resposta.");
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
