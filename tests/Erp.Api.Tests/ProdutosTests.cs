using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class ProdutosTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public ProdutosTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Cadastro (criar) -----

    [Fact]
    public async Task Cadastrar_SemStatus_CriaComoAtivo()
    {
        var resposta = await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal("ativo", produto.Status);
    }

    [Fact]
    public async Task Cadastrar_CodigoJaExistente_RecusadoENaoGrava()
    {
        await CriarAsync(new { codigo = "P001", descricao = "Caneta azul", precoVenda = 3.50m });

        var resposta = await CriarAsync(new { codigo = "P001", descricao = "Caneta vermelha", precoVenda = 4.00m });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Código já cadastrado", await TextoErroAsync(resposta));

        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul", atual.Descricao);
        Assert.Equal(3.50m, atual.PrecoVenda);
    }

    [Fact]
    public async Task Cadastrar_PrecoZero_EAceito()
    {
        var resposta = await CriarAsync(new { codigo = "P002", descricao = "Borracha", precoVenda = 0m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal(0.00m, produto.PrecoVenda);
    }

    [Fact]
    public async Task Cadastrar_DescricaoSomenteEspacos_Recusada()
    {
        var resposta = await CriarAsync(new { codigo = "P005", descricao = "   ", precoVenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_CodigoComMaisDe20Caracteres_Recusado()
    {
        var resposta = await CriarAsync(new { codigo = new string('A', 21), descricao = "Régua", precoVenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Código deve ter no máximo 20 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_DescricaoCom101Caracteres_Recusada()
    {
        var resposta = await CriarAsync(new { codigo = "P008", descricao = new string('a', 101), precoVenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter no máximo 100 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_DescricaoCom100Caracteres_EAceita()
    {
        var resposta = await CriarAsync(new { codigo = "P009", descricao = new string('a', 100), precoVenda = 1.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Theory]
    [InlineData(null, "Caneta", 1.0, null, "Código é obrigatório")]
    [InlineData("P-007", "Régua", 1.0, null, "Código aceita apenas letras e números")]
    [InlineData("P003", "Lápis", -1.0, null, "Preço não pode ser negativo")]
    [InlineData("P004", "Lápis", 2.555, null, "Preço aceita no máximo 2 casas decimais")]
    [InlineData("P001", "Régua", 1.0, "pausado", "Status deve ser ativo ou inativo")]
    [InlineData("P001", null, 1.0, null, "Descrição é obrigatória")]
    [InlineData("P001", "Régua", null, null, "Preço de venda é obrigatório")]
    public async Task Cadastrar_DadosInvalidos_SaoRecusadosComMensagem(
        string? codigo, string? descricao, double? precoVenda, string? status, string mensagem)
    {
        var resposta = await CriarAsync(new { codigo, descricao, precoVenda, status });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(mensagem, await TextoErroAsync(resposta));
        Assert.Equal(0, await QuantidadeProdutosAsync());
    }

    // ----- Listagem -----

    [Fact]
    public async Task Listar_SemFiltro_RetornaTodos()
    {
        await CriarProdutoAsync("P001", "Caneta", 1.00m, "ativo");
        await CriarProdutoAsync("P002", "Borracha", 2.00m, "ativo");
        await CriarProdutoAsync("P003", "Lápis", 3.00m, "inativo");

        var produtos = await ListarAsync("");

        Assert.Equal(3, produtos.Count);
    }

    [Fact]
    public async Task Listar_FiltroInativo_RetornaSomenteInativos()
    {
        await CriarProdutoAsync("P001", "Caneta", 1.00m, "ativo");
        await CriarProdutoAsync("P002", "Borracha", 2.00m, "ativo");
        await CriarProdutoAsync("P003", "Lápis", 3.00m, "inativo");

        var produtos = await ListarAsync("?status=inativo");

        Assert.Single(produtos);
        Assert.Equal("P003", produtos[0].Codigo);
    }

    [Fact]
    public async Task Listar_FiltroStatusInvalido_Recusado()
    {
        var resposta = await _cliente.GetAsync("/api/produtos?status=pausado");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status deve ser ativo ou inativo", await TextoErroAsync(resposta));
    }

    // ----- Consulta -----

    [Fact]
    public async Task Obter_CodigoExistente_RetornaTodosOsCampos()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var produto = await LerProdutoAsync(await ObterAsync("P001"));

        Assert.Equal("P001", produto.Codigo);
        Assert.Equal("Caneta azul", produto.Descricao);
        Assert.Equal(3.50m, produto.PrecoVenda);
        Assert.Equal("ativo", produto.Status);
    }

    [Fact]
    public async Task Obter_CodigoInexistente_NaoEncontrado()
    {
        var resposta = await ObterAsync("X999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Alteração -----

    [Fact]
    public async Task Alterar_DadosValidos_AtualizaEMantemCodigo()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var resposta = await AlterarAsync("P001", new
        {
            codigo = "P999",
            descricao = "Caneta azul fina",
            precoVenda = 3.90m,
            status = "inativo"
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul fina", atual.Descricao);
        Assert.Equal(3.90m, atual.PrecoVenda);
        Assert.Equal("inativo", atual.Status);
        Assert.Equal(HttpStatusCode.NotFound, (await ObterAsync("P999")).StatusCode);
    }

    [Fact]
    public async Task Alterar_PrecoNegativo_RecusadoENaoAltera()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var resposta = await AlterarAsync("P001", new { descricao = "Caneta azul", precoVenda = -2.00m, status = "ativo" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Preço não pode ser negativo", await TextoErroAsync(resposta));
        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal(3.50m, atual.PrecoVenda);
    }

    [Fact]
    public async Task Alterar_CodigoInexistente_NaoEncontrado()
    {
        var resposta = await AlterarAsync("X999", new { descricao = "Caneta", precoVenda = 1.00m, status = "ativo" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Exclusão -----

    [Fact]
    public async Task Excluir_CodigoExistente_RemoveDoBanco()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var resposta = await _cliente.DeleteAsync("/api/produtos/P001");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync("P001");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task Excluir_CodigoInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/produtos/X999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> CriarAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/produtos", corpo);
    }

    private Task<HttpResponseMessage> ObterAsync(string codigo)
    {
        return _cliente.GetAsync($"/api/produtos/{codigo}");
    }

    private Task<HttpResponseMessage> AlterarAsync(string codigo, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/produtos/{codigo}", corpo);
    }

    private async Task CriarProdutoAsync(string codigo, string descricao, decimal precoVenda, string status)
    {
        var resposta = await CriarAsync(new { codigo, descricao, precoVenda, status });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private async Task<List<ProdutoResposta>> ListarAsync(string query)
    {
        var produtos = await _cliente.GetFromJsonAsync<List<ProdutoResposta>>($"/api/produtos{query}");
        return produtos!;
    }

    private async Task<int> QuantidadeProdutosAsync()
    {
        var produtos = await ListarAsync("");
        return produtos.Count;
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

    private record ProdutoResposta(string Codigo, string Descricao, decimal PrecoVenda, string Status);
}
