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
        var resposta = await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal("ativo", produto.Prod_status);
    }

    [Fact]
    public async Task Cadastrar_CodigoJaExistente_RecusadoENaoGrava()
    {
        await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta azul", Prod_precovenda = 3.50m });

        var resposta = await CriarAsync(new { Prod_codigo = "P001", Prod_descricao = "Caneta vermelha", Prod_precovenda = 4.00m });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Código já cadastrado", await TextoErroAsync(resposta));

        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul", atual.Prod_descricao);
        Assert.Equal(3.50m, atual.Prod_precovenda);
    }

    [Fact]
    public async Task Cadastrar_PrecoZero_EAceito()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P002", Prod_descricao = "Borracha", Prod_precovenda = 0m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var produto = await LerProdutoAsync(resposta);
        Assert.Equal(0.00m, produto.Prod_precovenda);
    }

    [Fact]
    public async Task Cadastrar_DescricaoSomenteEspacos_Recusada()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P005", Prod_descricao = "   ", Prod_precovenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_CodigoComMaisDe20Caracteres_Recusado()
    {
        var resposta = await CriarAsync(new { Prod_codigo = new string('A', 21), Prod_descricao = "Régua", Prod_precovenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Código deve ter no máximo 20 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_DescricaoCom101Caracteres_Recusada()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P008", Prod_descricao = new string('a', 101), Prod_precovenda = 1.00m });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter no máximo 100 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_DescricaoCom100Caracteres_EAceita()
    {
        var resposta = await CriarAsync(new { Prod_codigo = "P009", Prod_descricao = new string('a', 100), Prod_precovenda = 1.00m });

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
        string? Prod_codigo, string? Prod_descricao, double? Prod_precovenda, string? Prod_status, string mensagem)
    {
        var resposta = await CriarAsync(new { Prod_codigo, Prod_descricao, Prod_precovenda, Prod_status });

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

        var produtos = await ListarAsync("?Prod_status=inativo");

        Assert.Single(produtos);
        Assert.Equal("P003", produtos[0].Prod_codigo);
    }

    [Fact]
    public async Task Listar_FiltroStatusInvalido_Recusado()
    {
        var resposta = await _cliente.GetAsync("/api/produtos?Prod_status=pausado");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status deve ser ativo ou inativo", await TextoErroAsync(resposta));
    }

    // ----- Consulta -----

    [Fact]
    public async Task Obter_CodigoExistente_RetornaTodosOsCampos()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var produto = await LerProdutoAsync(await ObterAsync("P001"));

        Assert.Equal("P001", produto.Prod_codigo);
        Assert.Equal("Caneta azul", produto.Prod_descricao);
        Assert.Equal(3.50m, produto.Prod_precovenda);
        Assert.Equal("ativo", produto.Prod_status);
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
            Prod_codigo = "P999",
            Prod_descricao = "Caneta azul fina",
            Prod_precovenda = 3.90m,
            Prod_status = "inativo"
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal("Caneta azul fina", atual.Prod_descricao);
        Assert.Equal(3.90m, atual.Prod_precovenda);
        Assert.Equal("inativo", atual.Prod_status);
        Assert.Equal(HttpStatusCode.NotFound, (await ObterAsync("P999")).StatusCode);
    }

    [Fact]
    public async Task Alterar_PrecoNegativo_RecusadoENaoAltera()
    {
        await CriarProdutoAsync("P001", "Caneta azul", 3.50m, "ativo");

        var resposta = await AlterarAsync("P001", new { Prod_descricao = "Caneta azul", Prod_precovenda = -2.00m, Prod_status = "ativo" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Preço não pode ser negativo", await TextoErroAsync(resposta));
        var atual = await LerProdutoAsync(await ObterAsync("P001"));
        Assert.Equal(3.50m, atual.Prod_precovenda);
    }

    [Fact]
    public async Task Alterar_CodigoInexistente_NaoEncontrado()
    {
        var resposta = await AlterarAsync("X999", new { Prod_descricao = "Caneta", Prod_precovenda = 1.00m, Prod_status = "ativo" });

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

    private Task<HttpResponseMessage> ObterAsync(string Prod_codigo)
    {
        return _cliente.GetAsync($"/api/produtos/{Prod_codigo}");
    }

    private Task<HttpResponseMessage> AlterarAsync(string Prod_codigo, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/produtos/{Prod_codigo}", corpo);
    }

    private async Task CriarProdutoAsync(string Prod_codigo, string Prod_descricao, decimal Prod_precovenda, string Prod_status)
    {
        var resposta = await CriarAsync(new { Prod_codigo, Prod_descricao, Prod_precovenda, Prod_status });
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

    private record ProdutoResposta(string Prod_codigo, string Prod_descricao, decimal Prod_precovenda, string Prod_status);
}
