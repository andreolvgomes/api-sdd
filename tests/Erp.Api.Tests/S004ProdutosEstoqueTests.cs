using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using Erp.Infra.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Erp.Api.Tests;

public class S004ProdutosEstoqueTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S004ProdutosEstoqueTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Campo Prod_estoque (R1, R2) -----

    [Fact]
    public async Task S004_R1_ConsultarProduto_RetornaCampoEstoque()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var produto = await LerProdutoAsync(await ObterAsync("P001"));

        Assert.Equal(0m, produto.Prod_estoque);
    }

    [Fact]
    public async Task S004_R2_CadastrarSemEstoque_CriaComEstoqueZero()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal(0.00m, produto.Prod_estoque);
    }

    // ----- Prod_estoque no cadastro (R4) -----

    [Fact]
    public async Task S004_R4_CadastrarComEstoque5_RecusadoENaoGrava()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P002", Prod_descricao = "Borracha", Prod_precovenda = 1.00m, Prod_estoque = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Estoque não pode ser informado nesta operação", await TextoErroAsync(resposta));
        Assert.Equal(HttpStatusCode.NotFound, (await ObterAsync("P002")).StatusCode);
    }

    [Fact]
    public async Task S004_R4_CadastrarComEstoque0_RecusadoENaoGrava()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P003", Prod_descricao = "Lápis", Prod_precovenda = 2.00m, Prod_estoque = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Estoque não pode ser informado nesta operação", await TextoErroAsync(resposta));
        Assert.Equal(HttpStatusCode.NotFound, (await ObterAsync("P003")).StatusCode);
    }

    [Fact]
    public async Task S004_R4_CadastrarComEstoqueNull_TratadoComoNaoInformado()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P004", Prod_descricao = "Régua", Prod_precovenda = 4.00m, Prod_estoque = (decimal?)null });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal(0.00m, produto.Prod_estoque);
    }

    // ----- Prod_estoque na alteração (R5) -----

    [Fact]
    public async Task S004_R5_AlterarComEstoque10_RecusadoENaoAltera()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await _cliente.PutAsJsonAsync("/api/produtos/P001", new { Prod_descricao = "Caneta azul fina", Prod_precovenda = 3.90m, Prod_status = "ativo", Prod_estoque = 10 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Estoque não pode ser informado nesta operação", await TextoErroAsync(resposta));
        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul", atual.Prod_descricao);
        Assert.Equal(0.00m, atual.Prod_estoque);
    }

    [Fact]
    public async Task S004_R5_AlterarComEstoqueNull_TratadoComoNaoInformado()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await _cliente.PutAsJsonAsync("/api/produtos/P001", new { Prod_descricao = "Caneta azul fina", Prod_precovenda = 3.90m, Prod_status = "ativo", Prod_estoque = (decimal?)null });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Consulta e listagem (R6, R7) -----

    [Fact]
    public async Task S004_R6_ConsultarProduto_RetornaEstoque()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await ObterAsync("P001");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(0.00m, (await LerProdutoAsync(resposta)).Prod_estoque);
    }

    [Fact]
    public async Task S004_R7_Listagem_MostraEstoqueDeCadaProduto()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });
        await CriarAsync(new { Prod_codigo = "P002", Prod_descricao = "Borracha", Prod_precovenda = 1.00m });

        var produtos = await _cliente.GetFromJsonAsync<List<ProdutoResposta>>("/api/produtos");

        Assert.Equal(2, produtos!.Count);
        Assert.All(produtos, produto => Assert.Equal(0.00m, produto.Prod_estoque));
    }

    // ----- Ativar, desativar, alterar e excluir (R8, R9, R10) -----

    [Fact]
    public async Task S004_R8_DesativarProduto_NaoAlteraEstoque()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        await _cliente.PostAsync("/api/produtos/P001/desativar", null);

        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("inativo", produto.Prod_status);
        Assert.Equal(0.00m, produto.Prod_estoque);
    }

    [Fact]
    public async Task S004_R9_ExcluirProduto_RemoveOEstoqueJunto()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await _cliente.DeleteAsync("/api/produtos/P001");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync("P001");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task S004_R10_AlterarSemEstoque_MantemEstoqueZero()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await _cliente.PutAsJsonAsync("/api/produtos/P001", new { Prod_descricao = "Caneta azul fina", Prod_precovenda = 3.90m, Prod_status = "ativo" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul fina", produto.Prod_descricao);
        Assert.Equal(0.00m, produto.Prod_estoque);
    }

    // ----- Produtos já cadastrados (R3) -----

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> CriarAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/produtos", corpo);
    }

    private Task<HttpResponseMessage> ObterAsync(string prod_codigo)
    {
        return _cliente.GetAsync($"/api/produtos/{prod_codigo}");
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

    private static async Task<ProdutoResposta> LerProdutoAsync(HttpResponseMessage resposta)
    {
        var produto = await resposta.Content.ReadFromJsonAsync<ProdutoResposta>();
        return produto!;
    }

    private record ProdutoResposta(string Prod_codigo, string Prod_descricao, decimal Prod_precovenda, string Prod_status, decimal Prod_estoque);
}
