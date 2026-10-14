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

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(1, endereco!.Ende_id);
        Assert.Equal("Entrega", endereco.Ende_rotulo);
        Assert.Equal("Av. Brasil, 200, Centro", endereco.Ende_endereco);
    }

    // ----- Campos obrigatórios (R2) -----

    [Fact]
    public async Task S003_R2_CadastrarSemRotulo_RecusadoENaoGrava()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo é obrigatório", await TextoErroAsync(resposta));
        var proximo = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });
        Assert.Equal(1, (await proximo.Content.ReadFromJsonAsync<EnderecoResposta>())!.Ende_id);
    }

    [Fact]
    public async Task S003_R2_CadastrarSemTexto_RecusadoENaoGrava()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
        var proximo = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });
        Assert.Equal(1, (await proximo.Content.ReadFromJsonAsync<EnderecoResposta>())!.Ende_id);
    }

    // ----- Rótulo (R3) -----

    [Fact]
    public async Task S003_R3_CadastrarComRotuloSomenteEspacos_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "   ", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R3_CadastrarComRotuloDe31Caracteres_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = new string('R', 31), Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Rótulo deve ter no máximo 30 caracteres", await TextoErroAsync(resposta));
    }

    // ----- Texto de endereço (R4) -----

    [Fact]
    public async Task S003_R4_CadastrarComTextoSomenteEspacos_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R4_CadastrarComTextoDe201Caracteres_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = new string('A', 201) });

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
            var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = $"Local {i}", Ende_endereco = $"Rua {i}, 1" });
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        }
    }

    [Fact]
    public async Task S003_R5_Cadastrar11oEnderecoAdicional_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { Ende_rotulo = $"Local {i}", Ende_endereco = $"Rua {i}, 1" });

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Local 11", Ende_endereco = "Rua 11, 1" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente já possui 10 endereços adicionais", await TextoErroAsync(resposta));
    }

    // ----- Cliente inexistente (R6) -----

    [Fact]
    public async Task S003_R6_CadastrarEnderecoAdicionalEmClienteInexistente_NaoEncontrado()
    {
        var resposta = await IncluirEnderecoAsync(99, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Alteração de endereço adicional (R9, R10) -----

    [Fact]
    public async Task S003_R9_AlterarEnderecoAdicional_MantemOIdEAtualizaRotulo()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var resposta = await AlterarEnderecoAsync(1, 1, new { Ende_rotulo = "Entrega principal", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(1, endereco!.Ende_id);
        Assert.Equal("Entrega principal", endereco.Ende_rotulo);
    }

    [Fact]
    public async Task S003_R9_AlterarComTextoVazio_Recusado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var resposta = await AlterarEnderecoAsync(1, 1, new { Ende_rotulo = "Entrega", Ende_endereco = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Endereço é obrigatório", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S003_R10_AlterarIdInexistente_NaoEncontrado()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");

        var resposta = await AlterarEnderecoAsync(1, 99, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Endereço não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Exclusão de endereço adicional (R10, R11) -----

    [Fact]
    public async Task S003_R11_ExcluirEnderecoAdicional_RemoveDefinitivamente()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var resposta = await _cliente.DeleteAsync("/api/clientes/1/enderecos/1");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var depois = await AlterarEnderecoAsync(1, 1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });
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
        var resposta = await AlterarEnderecoAsync(99, 1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado", await TextoErroAsync(resposta));
    }

    // ----- Consulta do cliente com endereços adicionais (R12, R8, R13) -----

    [Fact]
    public async Task S003_R12_ConsultarClienteComAdicionais_RetornaCadastroELista()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Cobrança", Ende_endereco = "Rua C, 3" });

        var resposta = await _cliente.GetAsync("/api/clientes/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var cliente = await resposta.Content.ReadFromJsonAsync<ClienteDetalheResposta>();
        Assert.Equal("Maria Silva", cliente!.Clie_nome);
        Assert.Equal(2, cliente.EnderecosAdicionais.Count);
        Assert.Equal(1, cliente.EnderecosAdicionais[0].Ende_id);
        Assert.Equal("Entrega", cliente.EnderecosAdicionais[0].Ende_rotulo);
        Assert.Equal(2, cliente.EnderecosAdicionais[1].Ende_id);
        Assert.Equal("Cobrança", cliente.EnderecosAdicionais[1].Ende_rotulo);
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
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();

        Assert.Equal("Rua das Flores, 10, Centro", cliente!.Clie_endereco);
    }

    [Fact]
    public async Task S003_R13_Listagem_NaoMostraEnderecosAdicionais()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var texto = await (await _cliente.GetAsync("/api/clientes?status=todos")).Content.ReadAsStringAsync();

        Assert.DoesNotContain("enderecosAdicionais", texto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Entrega", texto);
    }

    [Fact]
    public async Task S003_R5_11oRecusado_NaoGravaNada()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { Ende_rotulo = $"Local {i}", Ende_endereco = $"Rua {i}, 1" });
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Local 11", Ende_endereco = "Rua 11, 1" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();

        Assert.Equal(10, cliente!.EnderecosAdicionais.Count);
    }

    [Fact]
    public async Task S003_R11_ExcluirUmEDepoisCadastrarOutro_RespeitaOLimite()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        for (int i = 1; i <= 10; i++)
            await IncluirEnderecoAsync(1, new { Ende_rotulo = $"Local {i}", Ende_endereco = $"Rua {i}, 1" });
        await _cliente.DeleteAsync("/api/clientes/1/enderecos/2");

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Novo", Ende_endereco = "Rua Nova, 1" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task S003_R7_SegundoEnderecoAdicional_RecebeIdSequencial()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        var resposta = await IncluirEnderecoAsync(1, new { Ende_rotulo = "Cobrança", Ende_endereco = "Rua C, 3" });

        var endereco = await resposta.Content.ReadFromJsonAsync<EnderecoResposta>();
        Assert.Equal(2, endereco!.Ende_id);
    }

    [Fact]
    public async Task S003_R9_AlterarComTextoVazio_NaoAlteraOEndereco()
    {
        await CriarClienteAsync("Maria Silva", "Rua das Flores, 10, Centro", "11 91234-5678");
        await IncluirEnderecoAsync(1, new { Ende_rotulo = "Entrega", Ende_endereco = "Av. Brasil, 200, Centro" });

        await AlterarEnderecoAsync(1, 1, new { Ende_rotulo = "Outro", Ende_endereco = "" });

        var cliente = await (await _cliente.GetAsync("/api/clientes/1")).Content.ReadFromJsonAsync<ClienteDetalheResposta>();
        Assert.Equal("Entrega", cliente!.EnderecosAdicionais[0].Ende_rotulo);
        Assert.Equal("Av. Brasil, 200, Centro", cliente.EnderecosAdicionais[0].Ende_endereco);
    }

    // ----- Auxiliares -----

    private async Task CriarClienteAsync(string nome, string endereco, string telefone)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { Clie_nome = nome, Clie_endereco = endereco, Clie_telefone = telefone });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> IncluirEnderecoAsync(int Clie_id, object corpo)
    {
        return _cliente.PostAsJsonAsync($"/api/clientes/{Clie_id}/enderecos", corpo);
    }

    private Task<HttpResponseMessage> AlterarEnderecoAsync(int Clie_id, int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/clientes/{Clie_id}/enderecos/{Clie_id}", corpo);
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record EnderecoResposta(int Ende_id, string Ende_rotulo, string Ende_endereco);

    private record ClienteDetalheResposta(int Clie_id, string Clie_nome, string Clie_endereco, string Clie_telefone, string Clie_status, List<EnderecoResposta> EnderecosAdicionais);

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
