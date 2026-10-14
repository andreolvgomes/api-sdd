using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S009AlterarCancelarOrcamentoTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S009AlterarCancelarOrcamentoTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Alterar item (R1) -----

    [Fact]
    public async Task S009_R1_AlterarQuantidadeDoItem_GravaNovaQuantidadeETotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        JsonElement item = Item(doc, "002");
        Assert.Equal(5m, item.GetProperty("orci_quantidade").GetDecimal());
        Assert.Equal(1.00m, item.GetProperty("orci_preco").GetDecimal());
        Assert.Equal(5.00m, item.GetProperty("orci_total").GetDecimal());
    }

    [Fact]
    public async Task S009_R1_AlterarProdutoDoItem_GravaNovoProdutoETotal()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P005", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        var resposta = await AlterarItemAsync(Orca_id, "001", new { Prod_codigo = "P005" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        JsonElement item = Item(doc, "001");
        Assert.Equal("P005", item.GetProperty("prod_codigo").GetString());
        Assert.Equal(4.00m, item.GetProperty("orci_preco").GetDecimal());
        Assert.Equal(8.00m, item.GetProperty("orci_total").GetDecimal());
    }

    [Fact]
    public async Task S009_R20_SemQuantidadeNemProduto_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "002", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Informe a quantidade ou o produto para alterar.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(3.00m, Item(doc, "002").GetProperty("orci_total").GetDecimal());
    }

    // ----- Quantidade (R2) -----

    [Fact]
    public async Task S009_R2_QuantidadeZero_RecusadaENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal(3m, Item(doc, "002").GetProperty("orci_quantidade").GetDecimal());
    }

    [Fact]
    public async Task S009_R2_QuantidadeComTresCasas_Recusada()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 1.234m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    // ----- Produto alterado (R3) -----

    [Fact]
    public async Task S009_R3_ProdutoInexistente_RecusadoENaoGrava()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Prod_codigo = "P009" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal("P002", Item(doc, "002").GetProperty("prod_codigo").GetString());
    }

    [Fact]
    public async Task S009_R3_ProdutoInativo_RecusadoComMensagemDeAlteracao()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);
        await CriarProdutoAsync("P003", 2.00m, "inativo");
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P002", 3));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        var resposta = await AlterarItemAsync(Orca_id, "001", new { Prod_codigo = "P003" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser usado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R3_ProdutoSemEstoque_AceitoEEstoqueContinuaZero()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CriarProdutoAsync("P004", 4.00m);

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Prod_codigo = "P004" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("prod_estoque").GetDecimal());
    }

    // ----- Subtotal recalculado (R4) -----

    [Fact]
    public async Task S009_R4_AlterarItem_RecalculaSubtotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 }));

        Assert.Equal(15.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Preço do item (R5) -----

    [Fact]
    public async Task S009_R5_AlterarSomenteQuantidade_MantemPrecoGravado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await _cliente.PutAsJsonAsync("/api/produtos/P002", new { Prod_descricao = "Produto P002", Prod_precovenda = 9.00m, Prod_status = "ativo" });

        using var doc = await LerAsync(await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 5 }));

        Assert.Equal(1.00m, Item(doc, "002").GetProperty("orci_preco").GetDecimal());
    }

    [Fact]
    public async Task S009_R5_TrocarProduto_UsaPrecoDeVendaAtualDoNovoProduto()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P005", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        using var doc = await LerAsync(await AlterarItemAsync(Orca_id, "001", new { Prod_codigo = "P005" }));

        Assert.Equal(4.00m, Item(doc, "001").GetProperty("orci_preco").GetDecimal());
    }

    [Fact]
    public async Task S009_R5_MesmoProdutoNaTroca_NaoMudaPreco()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await _cliente.PutAsJsonAsync("/api/produtos/P001", new { Prod_descricao = "Caneta azul", Prod_precovenda = 9.00m, Prod_status = "ativo" });

        using var doc = await LerAsync(await AlterarItemAsync(Orca_id, "001", new { Prod_codigo = "P001" }));

        Assert.Equal(3.50m, Item(doc, "001").GetProperty("orci_preco").GetDecimal());
    }

    // ----- Cancelar item (R6) -----

    [Fact]
    public async Task S009_R6_CancelarItem_GravaCanceladoEItemContinuaNoBanco()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await CancelarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(Item(doc, "002").GetProperty("orci_cancelado").GetBoolean());
        Assert.Equal(3, doc.RootElement.GetProperty("itens").GetArrayLength());
    }

    // ----- Item cancelado sai do subtotal (R7) -----

    [Fact]
    public async Task S009_R7_ItemCancelado_SaiDoSubtotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await CancelarItemAsync(Orca_id, "002"));

        Assert.Equal(10.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Reativar item (R8) -----

    [Fact]
    public async Task S009_R8_ReativarItemCancelado_VoltaAoSubtotal()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");

        var resposta = await ReativarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.False(Item(doc, "002").GetProperty("orci_cancelado").GetBoolean());
        Assert.Equal(1.00m, Item(doc, "002").GetProperty("orci_preco").GetDecimal());
        Assert.Equal(3m, Item(doc, "002").GetProperty("orci_quantidade").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    [Fact]
    public async Task S009_R8_ProdutoInativo_ImpedeReativacao()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");
        await _cliente.PutAsJsonAsync("/api/produtos/P002", new { Prod_descricao = "Produto P002", Prod_precovenda = 1.00m, Prod_status = "inativo" });

        var resposta = await ReativarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser usado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.True(Item(doc, "002").GetProperty("orci_cancelado").GetBoolean());
    }

    // ----- Cancelar e reativar em estado errado (R9) -----

    [Fact]
    public async Task S009_R9_CancelarItemJaCancelado_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");

        var resposta = await CancelarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item já está cancelado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R9_ReativarItemNaoCancelado_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await ReativarItemAsync(Orca_id, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item não está cancelado.", await TextoErroAsync(resposta));
    }

    // ----- Item cancelado não pode ser alterado (R10) -----

    [Fact]
    public async Task S009_R10_AlterarItemCancelado_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");

        var resposta = await AlterarItemAsync(Orca_id, "002", new { Orci_quantidade = 4 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    // ----- Número do item (R11) -----

    [Fact]
    public async Task S009_R11_NumeroItem_EhSequencialComTresDigitos()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await ObterAsync(Orca_id));

        var numeros = doc.RootElement.GetProperty("itens").EnumerateArray()
            .Select(item => item.GetProperty("orci_numero_item").GetString())
            .ToList();
        Assert.Equal(new[] { "001", "002", "003" }, numeros);
    }

    // ----- Número do item não se repete (R12) -----

    [Fact]
    public async Task S009_R12_NumeroNaoReutilizadoAposCancelamento()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "003");

        using var doc = await LerAsync(await IncluirItemAsync(Orca_id, "P002", 1));

        Assert.Equal("004", doc.RootElement.GetProperty("itens")[3].GetProperty("orci_numero_item").GetString());
    }

    // ----- Item inexistente (R13) -----

    [Fact]
    public async Task S009_R13_ItemInexistente_RecusadoComMensagem()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(Orca_id, "005", new { Orci_quantidade = 2 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Item não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Cliente e vendedor do orçamento (R14) -----

    [Fact]
    public async Task S009_R14_TrocarClienteInativo_RecusadoENaoTroca()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int clienteInativo = await CriarClienteAsync("Ana Lima", "inativo");

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Clie_id = clienteInativo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente inativo não pode ser usado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(Orca_id));
        Assert.Equal("Maria Silva", doc.RootElement.GetProperty("cliente").GetProperty("clie_nome").GetString());
    }

    [Fact]
    public async Task S009_R14_TrocarVendedorInexistente_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Vend_id = 99 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R21_AlterarOrcamentoSemClienteNemVendedor_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Informe o cliente ou o vendedor para alterar.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R14_TrocarVendedorAtivo_Aceito()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Vend_id = vendedorNovo });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal("Paulo Souza", doc.RootElement.GetProperty("vendedor").GetProperty("vend_nome").GetString());
    }

    // ----- Orçamento cancelado é congelado (R17) -----

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaInclusaoDeItem()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await IncluirItemAsync(Orca_id, "P002", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaAlteracaoDeItem()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await AlterarItemAsync(Orca_id, "001", new { Orci_quantidade = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaReativacaoDeItem()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await ReativarItemAsync(Orca_id, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaTrocaDeVendedor()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await AlterarOrcamentoAsync(Orca_id, new { Vend_id = vendedorNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    // ----- Cancelar orçamento (R15) -----

    [Fact]
    public async Task S009_R15_CancelarOrcamento_GravaCanceladoEItensMantemEstado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");

        var resposta = await CancelarOrcamentoAsync(Orca_id);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(doc.RootElement.GetProperty("orca_cancelado").GetBoolean());
        Assert.True(Item(doc, "002").GetProperty("orci_cancelado").GetBoolean());
        Assert.False(Item(doc, "001").GetProperty("orci_cancelado").GetBoolean());
        Assert.Equal(3, doc.RootElement.GetProperty("itens").GetArrayLength());
    }

    // ----- Orçamento já cancelado (R16) -----

    [Fact]
    public async Task S009_R16_CancelarOrcamentoJaCancelado_Recusado()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarOrcamentoAsync(Orca_id);

        var resposta = await CancelarOrcamentoAsync(Orca_id);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento já está cancelado.", await TextoErroAsync(resposta));
    }

    // ----- Consulta mostra itens cancelados (R22) -----

    [Fact]
    public async Task S009_R22_ConsultaMostraItemCanceladoComIndicacao()
    {
        int Orca_id = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(Orca_id, "002");

        using var doc = await LerAsync(await ObterAsync(Orca_id));

        Assert.True(Item(doc, "002").GetProperty("orci_cancelado").GetBoolean());
        Assert.Equal(10.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Cancelar item não altera estoque (R19) -----

    [Fact]
    public async Task S009_R19_CancelarItem_NaoAlteraEstoqueDoProduto()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P004", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P004", 1));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        await CancelarItemAsync(Orca_id, "001");

        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("prod_estoque").GetDecimal());
    }

    // ----- Auxiliares -----

    private async Task<int> CriarOrcamentoComTresItensAsync()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await IncluirItemAsync(Orca_id, "P002", 3);
        await IncluirItemAsync(Orca_id, "P001", 1);
        return Orca_id;
    }

    private async Task<int> CriarClienteAsync(string nome = "Maria Silva", string status = "ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { Clie_nome = nome, Clie_endereco = "Rua A, 1", Clie_telefone = "11 90000-0001", Clie_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("clie_id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome = "João Lima", string status = "Ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { Vend_nome = nome, Vend_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("vend_id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda, string status = "ativo")
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { Prod_codigo = codigo, Prod_descricao = "Produto " + codigo, Prod_precovenda = precoVenda, Prod_status = status });
    }

    private Task<HttpResponseMessage> IncluirOrcamentoAsync(int Clie_id, int Vend_id, string Prod_codigo, decimal Orci_quantidade)
    {
        return _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo, Orci_quantidade });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int Orca_id, string Prod_codigo, decimal Orci_quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/itens", new { Prod_codigo, Orci_quantidade });
    }

    private Task<HttpResponseMessage> AlterarOrcamentoAsync(int Orca_id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{Orca_id}", corpo);
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

    private Task<HttpResponseMessage> CancelarOrcamentoAsync(int Orca_id)
    {
        return _cliente.PostAsync($"/api/orcamentos/{Orca_id}/cancelar", null);
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
