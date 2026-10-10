using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class ClientesTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public ClientesTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Cadastro (R1, R2, R6) -----

    [Fact]
    public async Task Cadastrar_DadosValidos_CriaComIdGeradoEDevolveCampos()
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = "Rua das Flores, 10, Centro", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var cliente = await LerClienteAsync(resposta);
        Assert.Equal(1, cliente.Id);
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("Rua das Flores, 10, Centro", cliente.Endereco);
        Assert.Equal("11 91234-5678", cliente.Telefone);
    }

    [Fact]
    public async Task Cadastrar_SegundoCliente_RecebeProximoIdSemAlterarOPrimeiro()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var resposta = await CriarAsync(new { nome = "João Souza", endereco = "Av. Brasil, 200", telefone = "21 99876-5432" });

        var cliente = await LerClienteAsync(resposta);
        Assert.Equal(2, cliente.Id);
        var primeiro = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("Maria Silva", primeiro.Nome);
    }

    [Fact]
    public async Task Cadastrar_IdInformadoNoCorpo_EIgnorado()
    {
        var resposta = await CriarAsync(new { id = 50, nome = "Maria Silva", endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        var cliente = await LerClienteAsync(resposta);
        Assert.Equal(1, cliente.Id);
    }

    // ----- Consulta (R7, R8) -----

    [Fact]
    public async Task Obter_IdExistente_RetornaTodosOsCampos()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var cliente = await LerClienteAsync(await ObterAsync(1));

        Assert.Equal(1, cliente.Id);
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("Rua das Flores, 10", cliente.Endereco);
        Assert.Equal("11 91234-5678", cliente.Telefone);
    }

    [Fact]
    public async Task Obter_IdInexistente_NaoEncontrado()
    {
        var resposta = await ObterAsync(99);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Validação de cadastro (R3, R4, R5, R14, R15) -----

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Cadastrar_NomeVazio_RecusadoComMensagem(string? nome)
    {
        var resposta = await CriarAsync(new { nome, endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome completo é obrigatório", await TextoErroAsync(resposta));
        Assert.Equal(0, await QuantidadeClientesAsync());
    }

    [Fact]
    public async Task Cadastrar_NomeCom101Caracteres_Recusado()
    {
        var resposta = await CriarAsync(new { nome = new string('a', 101), endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome completo deve ter no máximo 100 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_NomeCom100Caracteres_EAceito()
    {
        var resposta = await CriarAsync(new { nome = new string('a', 100), endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Cadastrar_EnderecoVazio_RecusadoComMensagem(string? endereco)
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco, telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
        Assert.Equal(0, await QuantidadeClientesAsync());
    }

    [Fact]
    public async Task Cadastrar_EnderecoCom201Caracteres_Recusado()
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = new string('a', 201), telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço deve ter no máximo 200 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_EnderecoCom200Caracteres_EAceito()
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = new string('a', 200), telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Cadastrar_TelefoneVazio_RecusadoComMensagem(string? telefone)
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = "Rua das Flores, 10", telefone });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Telefone é obrigatório", await TextoErroAsync(resposta));
        Assert.Equal(0, await QuantidadeClientesAsync());
    }

    [Fact]
    public async Task Cadastrar_TelefoneCom21Caracteres_Recusado()
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = "Rua das Flores, 10", telefone = new string('1', 21) });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Telefone deve ter no máximo 20 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Cadastrar_TelefoneComLetras_EAceito_SemValidarFormato()
    {
        var resposta = await CriarAsync(new { nome = "Maria Silva", endereco = "Rua das Flores, 10", telefone = "abc" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastrar_EspacosNasBordas_SaoRemovidosAntesDeSalvar()
    {
        var resposta = await CriarAsync(new { nome = "  Maria Silva  ", endereco = "  Rua das Flores, 10  ", telefone = "  11 91234-5678  " });

        var cliente = await LerClienteAsync(resposta);
        Assert.Equal("Maria Silva", cliente.Nome);
        Assert.Equal("Rua das Flores, 10", cliente.Endereco);
        Assert.Equal("11 91234-5678", cliente.Telefone);
    }

    [Fact]
    public async Task Cadastrar_NomeCom100CaracteresMaisEspacosNasBordas_EAceito()
    {
        var resposta = await CriarAsync(new { nome = "  " + new string('a', 100) + "  ", endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // ----- Alteração (R9, R10, R8) -----

    [Fact]
    public async Task Alterar_DadosValidos_AtualizaEMantemId()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var resposta = await AlterarAsync(1, new { nome = "Maria Silva Santos", endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var atual = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal(1, atual.Id);
        Assert.Equal("Maria Silva Santos", atual.Nome);
    }

    [Fact]
    public async Task Alterar_EnderecoVazio_RecusadoENaoAltera()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var resposta = await AlterarAsync(1, new { nome = "Maria Silva", endereco = "", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
        var atual = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("Rua das Flores, 10", atual.Endereco);
    }

    [Fact]
    public async Task Alterar_NomeCom101Caracteres_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var resposta = await AlterarAsync(1, new { nome = new string('a', 101), endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome completo deve ter no máximo 100 caracteres", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task Alterar_IdInexistente_NaoEncontrado()
    {
        var resposta = await AlterarAsync(99, new { nome = "Maria Silva", endereco = "Rua das Flores, 10", telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Exclusão (R11, R8) -----

    [Fact]
    public async Task Excluir_IdExistente_RemoveDoBanco()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10", "11 91234-5678");

        var resposta = await _cliente.DeleteAsync("/api/clientes/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync(1);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task Excluir_IdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/clientes/99");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Listagem (R12, R13) -----

    [Fact]
    public async Task Listar_SemClientes_RetornaListaVazia()
    {
        var clientes = await ListarAsync();

        Assert.Empty(clientes);
    }

    [Fact]
    public async Task Listar_OrdenaPeloNomeCompleto()
    {
        await CriarClienteAsync("Maria Silva", "Rua A, 1", "11 90000-0001");
        await CriarClienteAsync("João Souza", "Rua B, 2", "21 90000-0002");
        await CriarClienteAsync("Ana Lima", "Rua C, 3", "31 90000-0003");

        var clientes = await ListarAsync();

        Assert.Equal(new[] { "Ana Lima", "João Souza", "Maria Silva" }, clientes.Select(c => c.Nome).ToArray());
    }

    [Fact]
    public async Task Listar_IgnoraMaiusculasEAcentos_NaOrdenacao()
    {
        await CriarClienteAsync("Bruno Costa", "Rua A, 1", "11 90000-0001");
        await CriarClienteAsync("Álvaro Dias", "Rua B, 2", "21 90000-0002");
        await CriarClienteAsync("ana Lima", "Rua C, 3", "31 90000-0003");

        var clientes = await ListarAsync();

        Assert.Equal(new[] { "Álvaro Dias", "ana Lima", "Bruno Costa" }, clientes.Select(c => c.Nome).ToArray());
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> CriarAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/clientes", corpo);
    }

    private async Task CriarClienteAsync(string nome, string endereco, string telefone)
    {
        var resposta = await CriarAsync(new { nome, endereco, telefone });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> ObterAsync(int id)
    {
        return _cliente.GetAsync($"/api/clientes/{id}");
    }

    private Task<HttpResponseMessage> AlterarAsync(int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/clientes/{id}", corpo);
    }

    private async Task<List<ClienteResposta>> ListarAsync()
    {
        var clientes = await _cliente.GetFromJsonAsync<List<ClienteResposta>>("/api/clientes");
        return clientes!;
    }

    private async Task<int> QuantidadeClientesAsync()
    {
        var clientes = await ListarAsync();
        return clientes.Count;
    }

    private static async Task<ClienteResposta> LerClienteAsync(HttpResponseMessage resposta)
    {
        var cliente = await resposta.Content.ReadFromJsonAsync<ClienteResposta>();
        return cliente!;
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

    private record ClienteResposta(int Id, string Nome, string Endereco, string Telefone);
}
