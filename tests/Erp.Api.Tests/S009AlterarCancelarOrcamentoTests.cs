using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
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
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { quantidade = 5 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        JsonElement item = Item(doc, "002");
        Assert.Equal(5m, item.GetProperty("quantidade").GetDecimal());
        Assert.Equal(1.00m, item.GetProperty("preco").GetDecimal());
        Assert.Equal(5.00m, item.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task S009_R1_AlterarProdutoDoItem_GravaNovoProdutoETotal()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P005", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        var resposta = await AlterarItemAsync(orcamentoId, "001", new { codigoProduto = "P005" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        JsonElement item = Item(doc, "001");
        Assert.Equal("P005", item.GetProperty("codigoProduto").GetString());
        Assert.Equal(4.00m, item.GetProperty("preco").GetDecimal());
        Assert.Equal(8.00m, item.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task S009_R1_SemQuantidadeNemProduto_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Informe a quantidade ou o produto para alterar.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal(3.00m, Item(doc, "002").GetProperty("total").GetDecimal());
    }

    // ----- Quantidade (R2) -----

    [Fact]
    public async Task S009_R2_QuantidadeZero_RecusadaENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { quantidade = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal(3m, Item(doc, "002").GetProperty("quantidade").GetDecimal());
    }

    [Fact]
    public async Task S009_R2_QuantidadeComTresCasas_Recusada()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { quantidade = 1.234m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    // ----- Produto alterado (R3) -----

    [Fact]
    public async Task S009_R3_ProdutoInexistente_RecusadoENaoGrava()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { codigoProduto = "P009" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal("P002", Item(doc, "002").GetProperty("codigoProduto").GetString());
    }

    [Fact]
    public async Task S009_R3_ProdutoInativo_RecusadoComMensagemDeAlteracao()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);
        await CriarProdutoAsync("P003", 2.00m, "inativo");
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P002", 3));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        var resposta = await AlterarItemAsync(orcamentoId, "001", new { codigoProduto = "P003" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser usado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R3_ProdutoSemEstoque_AceitoEEstoqueContinuaZero()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CriarProdutoAsync("P004", 4.00m);

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { codigoProduto = "P004" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("estoque").GetDecimal());
    }

    // ----- Subtotal recalculado (R4) -----

    [Fact]
    public async Task S009_R4_AlterarItem_RecalculaSubtotal()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await AlterarItemAsync(orcamentoId, "002", new { quantidade = 5 }));

        Assert.Equal(15.50m, doc.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Preço do item (R5) -----

    [Fact]
    public async Task S009_R5_AlterarSomenteQuantidade_MantemPrecoGravado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await _cliente.PutAsJsonAsync("/api/produtos/P002", new { descricao = "Produto P002", precoVenda = 9.00m, status = "ativo" });

        using var doc = await LerAsync(await AlterarItemAsync(orcamentoId, "002", new { quantidade = 5 }));

        Assert.Equal(1.00m, Item(doc, "002").GetProperty("preco").GetDecimal());
    }

    [Fact]
    public async Task S009_R5_TrocarProduto_UsaPrecoDeVendaAtualDoNovoProduto()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P005", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        using var doc = await LerAsync(await AlterarItemAsync(orcamentoId, "001", new { codigoProduto = "P005" }));

        Assert.Equal(4.00m, Item(doc, "001").GetProperty("preco").GetDecimal());
    }

    [Fact]
    public async Task S009_R5_MesmoProdutoNaTroca_NaoMudaPreco()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        await _cliente.PutAsJsonAsync("/api/produtos/P001", new { descricao = "Caneta azul", precoVenda = 9.00m, status = "ativo" });

        using var doc = await LerAsync(await AlterarItemAsync(orcamentoId, "001", new { codigoProduto = "P001" }));

        Assert.Equal(3.50m, Item(doc, "001").GetProperty("preco").GetDecimal());
    }

    // ----- Cancelar item (R6) -----

    [Fact]
    public async Task S009_R6_CancelarItem_GravaCanceladoEItemContinuaNoBanco()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await CancelarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.True(Item(doc, "002").GetProperty("cancelado").GetBoolean());
        Assert.Equal(3, doc.RootElement.GetProperty("itens").GetArrayLength());
    }

    // ----- Item cancelado sai do subtotal (R7) -----

    [Fact]
    public async Task S009_R7_ItemCancelado_SaiDoSubtotal()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await CancelarItemAsync(orcamentoId, "002"));

        Assert.Equal(10.50m, doc.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Reativar item (R8) -----

    [Fact]
    public async Task S009_R8_ReativarItemCancelado_VoltaAoSubtotal()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");

        var resposta = await ReativarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.False(Item(doc, "002").GetProperty("cancelado").GetBoolean());
        Assert.Equal(1.00m, Item(doc, "002").GetProperty("preco").GetDecimal());
        Assert.Equal(3m, Item(doc, "002").GetProperty("quantidade").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("subtotal").GetDecimal());
    }

    [Fact]
    public async Task S009_R8_ProdutoInativo_ImpedeReativacao()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");
        await _cliente.PutAsJsonAsync("/api/produtos/P002", new { descricao = "Produto P002", precoVenda = 1.00m, status = "inativo" });

        var resposta = await ReativarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser usado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.True(Item(doc, "002").GetProperty("cancelado").GetBoolean());
    }

    // ----- Cancelar e reativar em estado errado (R9) -----

    [Fact]
    public async Task S009_R9_CancelarItemJaCancelado_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");

        var resposta = await CancelarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item já está cancelado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R9_ReativarItemNaoCancelado_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await ReativarItemAsync(orcamentoId, "001");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item não está cancelado.", await TextoErroAsync(resposta));
    }

    // ----- Item cancelado não pode ser alterado (R10) -----

    [Fact]
    public async Task S009_R10_AlterarItemCancelado_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");

        var resposta = await AlterarItemAsync(orcamentoId, "002", new { quantidade = 4 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Item cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    // ----- Número do item (R11) -----

    [Fact]
    public async Task S009_R11_NumeroItem_EhSequencialComTresDigitos()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        using var doc = await LerAsync(await ObterAsync(orcamentoId));

        var numeros = doc.RootElement.GetProperty("itens").EnumerateArray()
            .Select(item => item.GetProperty("numeroItem").GetString())
            .ToList();
        Assert.Equal(new[] { "001", "002", "003" }, numeros);
    }

    // ----- Número do item não se repete (R12) -----

    [Fact]
    public async Task S009_R12_NumeroNaoReutilizadoAposCancelamento()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "003");

        using var doc = await LerAsync(await IncluirItemAsync(orcamentoId, "P002", 1));

        Assert.Equal("004", doc.RootElement.GetProperty("itens")[3].GetProperty("numeroItem").GetString());
    }

    // ----- Item inexistente (R13) -----

    [Fact]
    public async Task S009_R13_ItemInexistente_RecusadoComMensagem()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarItemAsync(orcamentoId, "005", new { quantidade = 2 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Item não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Cliente e vendedor do orçamento (R14) -----

    [Fact]
    public async Task S009_R14_TrocarClienteInativo_RecusadoENaoTroca()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int clienteInativo = await CriarClienteAsync("Ana Lima", "inativo");

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { clienteId = clienteInativo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente inativo não pode ser usado.", await TextoErroAsync(resposta));
        using var doc = await LerAsync(await ObterAsync(orcamentoId));
        Assert.Equal("Maria Silva", doc.RootElement.GetProperty("cliente").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task S009_R14_TrocarVendedorInexistente_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { vendedorId = 99 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R14_AlterarOrcamentoSemClienteNemVendedor_Recusado()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Informe o cliente ou o vendedor para alterar.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R14_TrocarVendedorAtivo_Aceito()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { vendedorId = vendedorNovo });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal("Paulo Souza", doc.RootElement.GetProperty("vendedor").GetProperty("nome").GetString());
    }

    // ----- Orçamento cancelado é congelado (R17) -----

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaInclusaoDeItem()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        CancelarOrcamentoNoBanco(orcamentoId);

        var resposta = await IncluirItemAsync(orcamentoId, "P002", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaAlteracaoDeItem()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        CancelarOrcamentoNoBanco(orcamentoId);

        var resposta = await AlterarItemAsync(orcamentoId, "001", new { quantidade = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaReativacaoDeItem()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        await CancelarItemAsync(orcamentoId, "002");
        CancelarOrcamentoNoBanco(orcamentoId);

        var resposta = await ReativarItemAsync(orcamentoId, "002");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S009_R17_OrcamentoCancelado_NaoAceitaTrocaDeVendedor()
    {
        int orcamentoId = await CriarOrcamentoComTresItensAsync();
        int vendedorNovo = await CriarVendedorAsync("Paulo Souza");
        CancelarOrcamentoNoBanco(orcamentoId);

        var resposta = await AlterarOrcamentoAsync(orcamentoId, new { vendedorId = vendedorNovo });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Orçamento cancelado não pode ser alterado.", await TextoErroAsync(resposta));
    }

    // ----- Cancelar item não altera estoque (R19) -----

    [Fact]
    public async Task S009_R19_CancelarItem_NaoAlteraEstoqueDoProduto()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P004", 4.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P004", 1));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        await CancelarItemAsync(orcamentoId, "001");

        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("estoque").GetDecimal());
    }

    // ----- Auxiliares -----

    private async Task<int> CriarOrcamentoComTresItensAsync()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        await IncluirItemAsync(orcamentoId, "P002", 3);
        await IncluirItemAsync(orcamentoId, "P001", 1);
        return orcamentoId;
    }

    private async Task<int> CriarClienteAsync(string nome = "Maria Silva", string status = "ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { nome, endereco = "Rua A, 1", telefone = "11 90000-0001", status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome = "João Lima", string status = "Ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { nome, status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda, string status = "ativo")
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { codigo, descricao = "Produto " + codigo, precoVenda, status });
    }

    private Task<HttpResponseMessage> IncluirOrcamentoAsync(int clienteId, int vendedorId, string codigoProduto, decimal quantidade)
    {
        return _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, codigoProduto, quantidade });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int orcamentoId, string codigoProduto, decimal quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens", new { codigoProduto, quantidade });
    }

    private Task<HttpResponseMessage> AlterarOrcamentoAsync(int orcamentoId, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/orcamentos/{orcamentoId}", corpo);
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

    private Task<HttpResponseMessage> ObterAsync(int orcamentoId)
    {
        return _cliente.GetAsync($"/api/orcamentos/{orcamentoId}");
    }

    // Cancelar o orçamento pela API ainda não está implementado (aguarda decisão da R16/R17).
    // Este helper só prepara o cenário de orçamento cancelado para testar a R17.
    private void CancelarOrcamentoNoBanco(int orcamentoId)
    {
        using var conexao = new SqliteConnection($"Data Source={_api.ArquivoBanco};Pooling=False");
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = "update orcamento set cancelado = 1 where id = $id";
        comando.Parameters.AddWithValue("$id", orcamentoId);
        comando.ExecuteNonQuery();
    }

    private static JsonElement Item(JsonDocument doc, string numeroItem)
    {
        foreach (JsonElement item in doc.RootElement.GetProperty("itens").EnumerateArray())
            if (item.GetProperty("numeroItem").GetString() == numeroItem)
                return item.Clone();

        throw new InvalidOperationException($"Item {numeroItem} não encontrado na resposta.");
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
