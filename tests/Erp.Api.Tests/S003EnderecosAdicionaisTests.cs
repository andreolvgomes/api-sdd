using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S003EnderecosAdicionaisTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S003EnderecosAdicionaisTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Cadastro de endereço adicional (R7) -----

    [Fact]
    public async Task S003_R7_CadastrarEnderecoAdicional_CriaComIdGeradoPeloSistema()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(1, endereco!.Id);
        Assert.Equal("Entrega", endereco.Rotulo);
        Assert.Equal("Av. Brasil, 200, Centro", endereco.Endereco);
    }

    // ----- Campos obrigatórios (R2) -----

    [Fact]
    public async Task S003_R2_CadastrarSemRotulo_RecusadoENaoGrava()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo é obrigatório", await TextoErroAsync(resposta));
        var proximo = await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });
        Assert.Equal(1, (await proximo.Content.ReadFromJsonAsync<EnderecoResposta>())!.Id);
    }

    [Fact]
    public async Task S003_R2_CadastrarSemTexto_RecusadoENaoGrava()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Entrega" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
        var proximo = await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });
        Assert.Equal(1, (await proximo.Content.ReadFromJsonAsync<EnderecoResposta>())!.Id);
    }

    // ----- Rótulo (R3) -----

    [Fact]
    public async Task S003_R3_CadastrarComRotuloSomenteEspacos_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "   ", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R3_CadastrarComRotuloDe31Caracteres_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = new string('R', 31), endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo deve ter no máximo 30 caracteres", await TextoErroAsync(resposta));
    }

    // ----- Texto de endereço (R4) -----

    [Fact]
    public async Task S003_R4_CadastrarComTextoSomenteEspacos_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R4_CadastrarComTextoDe201Caracteres_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = new string('A', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço deve ter no máximo 200 caracteres", await TextoErroAsync(resposta));
    }

    // ----- Limite de endereços adicionais (R1, R5) -----

    [Fact]
    public async Task S003_R1_CadastrarAte10EnderecosAdicionais_EAceito()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        for (int i = 1; i <= 10; i++)
        {
            var resposta = await IncluirEnderecoAsync(1, new { rotulo = $"Local {i}", endereco = $"Rua {i}, 1" });
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        }
    }

    [Fact]
    public async Task S003_R5_Cadastrar11oEnderecoAdicional_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { rotulo = $"Local {i}", endereco = $"Rua {i}, 1" });

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Local 11", endereco = "Rua 11, 1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente já possui 10 endereços adicionais", await TextoErroAsync(resposta));
    }

    // ----- Cliente inexistente (R6) -----

    [Fact]
    public async Task S003_R6_CadastrarEnderecoAdicionalEmClienteInexistente_NaoEncontrado()
    {
        var resposta = await IncluirEnderecoAsync(99, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Alteração de endereço adicional (R9, R10) -----

    [Fact]
    public async Task S003_R9_AlterarEnderecoAdicional_MantemOIdEAtualizaRotulo()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var resposta = await AlterarEnderecoAsync(1, 1, new { rotulo = "Entrega principal", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(1, endereco!.Id);
        Assert.Equal("Entrega principal", endereco.Rotulo);
    }

    [Fact]
    public async Task S003_R9_AlterarComTextoVazio_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var resposta = await AlterarEnderecoAsync(1, 1, new { rotulo = "Entrega", endereco = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R10_AlterarIdInexistente_NaoEncontrado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await AlterarEnderecoAsync(1, 99, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Endereço não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Exclusão de endereço adicional (R10, R11) -----

    [Fact]
    public async Task S003_R11_ExcluirEnderecoAdicional_RemoveDefinitivamente()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var resposta = await _cliente.DeleteAsync("/api/clientes/1/enderecos/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var depois = await AlterarEnderecoAsync(1, 1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });
        Assert.Equal(HttpStatusCode.NotFound, depois.StatusCode);
        Assert.Contains("Endereço não encontrado", await TextoErroAsync(depois));
    }

    [Fact]
    public async Task S003_R10_ExcluirIdInexistente_NaoEncontrado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await _cliente.DeleteAsync("/api/clientes/1/enderecos/99");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Endereço não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R6_ExcluirEnderecoAdicionalDeClienteInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/clientes/99/enderecos/1");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R6_ConsultarClienteInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.GetAsync("/api/clientes/99");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R6_AlterarEnderecoAdicionalDeClienteInexistente_NaoEncontrado()
    {
        var resposta = await AlterarEnderecoAsync(99, 1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Consulta do cliente com endereços adicionais (R12, R8, R13) -----

    [Fact]
    public async Task S003_R12_ConsultarClienteComAdicionais_RetornaCadastroELista()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });
        await IncluirEnderecoAsync(1, new { rotulo = "Cobrança", endereco = "Rua C, 3" });

        var resposta = await _cliente.GetAsync("/api/clientes/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var cliente = await resposta.Content.ReadFromJsonAsync<ClienteDetalheResposta>();
        Assert.Equal("Maria Silva", cliente!.Nome);
        Assert.Equal(2, cliente.EnderecosAdicionais.Count);
        Assert.Equal(1, cliente.EnderecosAdicionais[0].Id);
        Assert.Equal("Entrega", cliente.EnderecosAdicionais[0].Rotulo);
        Assert.Equal(2, cliente.EnderecosAdicionais[1].Id);
        Assert.Equal("Cobrança", cliente.EnderecosAdicionais[1].Rotulo);
    }

    [Fact]
    public async Task S003_R12_ConsultarClienteSemAdicionais_RetornaListaVazia()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();

        Assert.Empty(cliente!.EnderecosAdicionais);
    }

    [Fact]
    public async Task S003_R8_EnderecoAdicional_NaoAlteraOEnderecoPrincipal()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();

        Assert.Equal("Rua das Flores, 10, Centro", cliente!.Endereco);
    }

    [Fact]
    public async Task S003_R13_Listagem_NaoMostraEnderecosAdicionais()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var texto = await (await _cliente.GetAsync("/api/clientes?status=todos")).Content.ReadAsStringAsync();

        Assert.DoesNotContain("enderecosAdicionais", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Entrega", texto);
    }

    [Fact]
    public async Task S003_R5_11oRecusado_NaoGravaNada()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { rotulo = $"Local {i}", endereco = $"Rua {i}, 1" });
        await IncluirEnderecoAsync(1, new { rotulo = "Local 11", endereco = "Rua 11, 1" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();

        Assert.Equal(10, cliente!.EnderecosAdicionais.Count);
    }

    [Fact]
    public async Task S003_R11_ExcluirUmEDepoisCadastrarOutro_RespeitaOLimite()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { rotulo = $"Local {i}", endereco = $"Rua {i}, 1" });
        await _cliente.DeleteAsync("/api/clientes/1/enderecos/2");

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Novo", endereco = "Rua Nova, 1" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task S003_R7_SegundoEnderecoAdicional_RecebeIdSequencial()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        var resposta = await IncluirEnderecoAsync(1, new { rotulo = "Cobrança", endereco = "Rua C, 3" });

        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(2, endereco!.Id);
    }

    [Fact]
    public async Task S003_R9_AlterarComTextoVazio_NaoAlteraOEndereco()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { rotulo = "Entrega", endereco = "Av. Brasil, 200, Centro" });

        await AlterarEnderecoAsync(1, 1, new { rotulo = "Outro", endereco = "" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();
        Assert.Equal("Entrega", cliente!.EnderecosAdicionais[0].Rotulo);
        Assert.Equal("Av. Brasil, 200, Centro", cliente.EnderecosAdicionais[0].Endereco);
    }

    // ----- Auxiliares -----

    private async Task CriarClienteAsync(string nome, string endereco, string telefone)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { nome, endereco, telefone });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> IncluirEnderecoAsync(int clienteId, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/clientes/{clienteId}/enderecos", corpo);
    }

    private Task<HttpResponseMessage> AlterarEnderecoAsync(int clienteId, int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/clientes/{clienteId}/enderecos/{id}", corpo);
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record EnderecoResposta(int Id, string Rotulo, string Endereco);

    private record ClienteDetalheResposta(int Id, string Nome, string Endereco, string Telefone, string Status, List<EnderecoResposta> EnderecosAdicionais);

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
