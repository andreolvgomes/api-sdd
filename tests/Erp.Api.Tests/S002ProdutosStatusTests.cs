using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S002ProdutosStatusTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S002ProdutosStatusTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Desativar (R1) -----

    [Fact]
    public async Task S002_R1_DesativarProdutoAtivo_MudaStatusParaInativo()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m);

        var resposta = await _cliente.PostAsync("/api/produtos/P001/desativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul", produto.Descricao);
        Assert.Equal("inativo", produto.Status);
    }

    [Fact]
    public async Task S002_R2_DesativarProdutoJaInativo_Recusado()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.PostAsync("/api/produtos/P001/desativar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto já está inativo", await TextoErroAsync(resposta));
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("inativo", produto.Status);
    }

    // ----- Ativar (R3) -----

    [Fact]
    public async Task S002_R3_AtivarProdutoInativo_MudaStatusParaAtivo()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.PostAsync("/api/produtos/P001/ativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("ativo", produto.Status);
    }

    [Fact]
    public async Task S002_R4_AtivarProdutoJaAtivo_Recusado()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m);

        var resposta = await _cliente.PostAsync("/api/produtos/P001/ativar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto já está ativo", await TextoErroAsync(resposta));
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("ativo", produto.Status);
    }

    [Fact]
    public async Task S002_R5_DesativarCodigoInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.PostAsync("/api/produtos/X999/desativar", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S002_R5_AtivarCodigoInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.PostAsync("/api/produtos/X999/ativar", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S002_R1_ListarComFiltroInativo_RetornaSomenteOsDesativados()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m);
        await CriarProdutoAsync("P002", "Borracha", 1.00m);
        await _cliente.PostAsync("/api/produtos/P001/desativar", null);

        var produtos = await _cliente.GetFromJsonAsync<List<ProdutoResposta>>("/api/produtos?status=inativo");

        Assert.Single(produtos!);
        Assert.Equal("P001", produtos![0].Codigo);
    }

    // ----- Sem corpo de resposta e sem motivo (R6, R7) -----

    [Fact]
    public async Task S002_R6_DesativarComSucesso_RespondeSemCorpo()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m);

        var resposta = await _cliente.PostAsync("/api/produtos/P001/desativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task S002_R6_AtivarComSucesso_RespondeSemCorpo()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.PostAsync("/api/produtos/P001/ativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task S002_R7_DesativarNaoExigeMotivo()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m);

        var resposta = await _cliente.PostAsJsonAsync("/api/produtos/P001/desativar", new { });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task S002_R7_AtivarNaoExigeMotivo()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.PostAsJsonAsync("/api/produtos/P001/ativar", new { });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Exclusão definitiva (R8) e alteração de status (R9) -----

    [Fact]
    public async Task S002_R8_ExcluirProdutoInativo_RemoveODefinitivamente()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.DeleteAsync("/api/produtos/P001");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync("P001");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task S002_R9_AlterarProdutoInativoComStatusAtivo_ReativaEMantemOsDemaisCampos()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m, status = "inativo" });

        var resposta = await _cliente.PutAsJsonAsync("/api/produtos/P001", new { descricao = "Caneta azul", precoVenda = 3.50m, status = "ativo" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var produto = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul", produto.Descricao);
        Assert.Equal(3.50m, produto.PrecoVenda);
        Assert.Equal("ativo", produto.Status);
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> CriarAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/produtos", corpo);
    }

    private async Task CriarProdutoAsync(string codigo, string descricao, decimal precoVenda)
    {
        var resposta = await CriarAsync(new { codigo, descricao, precoVenda });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> ObterAsync(string codigo)
    {
        return _cliente.GetAsync($"/api/produtos/{codigo}");
    }

    private static async Task<ProdutoResposta> LerProdutoAsync(HttpResponseMessage resposta)
    {
        var produto = await resposta.Content.ReadFromJsonAsync<ProdutoResposta>();
        return produto!;
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record ProdutoResposta(string Codigo, string Descricao, decimal PrecoVenda, string Status);

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
