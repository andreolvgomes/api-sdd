using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S001ClientesStatusTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S001ClientesStatusTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Cadastro e Clie_status (R1, R2, R3) -----

    [Fact]
    public async Task S001_R2_CadastrarSemStatus_NasceAtivo()
    {
        var resposta = await CriarAsync(new { Clie_nome = "Maria Silva", Clie_endereco = "Rua das Flores, 10, Centro", Clie_telefone = "11 91234-5678" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var cliente = await LerClienteAsync(resposta);
        Assert.Equal("ativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R1_CadastrarComStatusInativo_CriaInativo()
    {
        var resposta = await CriarAsync(new { Clie_nome = "Ana Lima", Clie_endereco = "Rua C, 3", Clie_telefone = "31 90000-0003", Clie_status = "inativo" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var cliente = await LerClienteAsync(resposta);
        Assert.Equal("inativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R3_CadastrarComStatusRegistrado_RecusadoENaoGrava()
    {
        var resposta = await CriarAsync(new { Clie_nome = "João Souza", Clie_endereco = "Av. Brasil, 200", Clie_telefone = "21 99876-5432", Clie_status = "registrado" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status deve ser ativo ou inativo", await TextoErroAsync(resposta));
        var consulta = await _cliente.GetFromJsonAsync<List<ClienteResposta>>("/api/clientes?clie_status=todos");
        Assert.Empty(consulta!);
    }

    [Fact]
    public async Task S001_R10_AtivarNaoExigeMotivo()
    {
        await CriarAsync(new { Clie_nome = "João Souza", Clie_endereco = "Av. Brasil, 200", Clie_telefone = "21 99876-5432", Clie_status = "inativo" });

        var resposta = await _cliente.PostAsJsonAsync("/api/clientes/1/ativar", new { });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Ativar (R6, R7, R8, R9) -----

    [Fact]
    public async Task S001_R6_AtivarClienteInativo_MudaStatusParaAtivo()
    {
        await CriarAsync(new { Clie_nome = "João Souza", Clie_endereco = "Av. Brasil, 200", Clie_telefone = "21 99876-5432", Clie_status = "inativo" });

        var resposta = await _cliente.PostAsync("/api/clientes/1/ativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await resposta.Content.ReadAsStringAsync());
        var cliente = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("ativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R7_AtivarClienteJaAtivo_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.PostAsync("/api/clientes/1/ativar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente já está ativo", await TextoErroAsync(resposta));
        var cliente = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("ativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R8_AtivarIdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.PostAsync("/api/clientes/99/ativar", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Desativar (R4, R5, R8, R9, R10) -----

    [Fact]
    public async Task S001_R4_DesativarClienteAtivo_MudaStatusParaInativo()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.PostAsync("/api/clientes/1/desativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var cliente = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("Maria Silva", cliente.Clie_nome);
        Assert.Equal("inativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R5_DesativarClienteJaInativo_Recusado()
    {
        await CriarAsync(new { Clie_nome = "João Souza", Clie_endereco = "Av. Brasil, 200", Clie_telefone = "21 99876-5432", Clie_status = "inativo" });

        var resposta = await _cliente.PostAsync("/api/clientes/1/desativar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente já está inativo", await TextoErroAsync(resposta));
        var cliente = await LerClienteAsync(await ObterAsync(1));
        Assert.Equal("inativo", cliente.Clie_status);
    }

    [Fact]
    public async Task S001_R8_DesativarIdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.PostAsync("/api/clientes/99/desativar", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S001_R9_DesativarComSucesso_RespondeSemCorpo()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.PostAsync("/api/clientes/1/desativar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task S001_R10_DesativarNaoExigeMotivo()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.PostAsJsonAsync("/api/clientes/1/desativar", new { });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Exclusão definitiva (R11, R12, R13) -----

    [Fact]
    public async Task S001_R11_ExcluirClienteAtivo_RemoveODefinitivamente()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.DeleteAsync("/api/clientes/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync(1);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task S001_R11_ExcluirClienteInativo_RemoveODefinitivamente()
    {
        await CriarAsync(new { Clie_nome = "Maria Silva", Clie_endereco = "Rua A, 1", Clie_telefone = "11 90000-0001" });
        await CriarAsync(new { Clie_nome = "João Souza", Clie_endereco = "Av. Brasil, 200", Clie_telefone = "21 99876-5432", Clie_status = "inativo" });

        var resposta = await _cliente.DeleteAsync("/api/clientes/2");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var consulta = await ObterAsync(2);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(consulta));
    }

    [Fact]
    public async Task S001_R12_ExcluirClienteRecemCadastradoPorEngano_EPermitido()
    {
        await CriarClienteAsync("Mariana Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.DeleteAsync("/api/clientes/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact]
    public async Task S001_R13_ExcluirIdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/clientes/99");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> CriarAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/clientes", corpo);
    }

    private async Task CriarClienteAsync(string Clie_nome, string Clie_endereco, string Clie_telefone)
    {
        var resposta = await CriarAsync(new { Clie_nome, Clie_endereco, Clie_telefone });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> ObterAsync(int clie_id)
    {
        return _cliente.GetAsync($"/api/clientes/{clie_id}");
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

    private record ClienteResposta(int Clie_id, string Clie_nome, string Clie_endereco, string Clie_telefone, string Clie_status);

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
