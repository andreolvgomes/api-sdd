using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S005VendedoresTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S005VendedoresTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Id sequencial gerado pelo sistema (R1) -----

    [Fact]
    public async Task S005_R1_IncluirVendedores_IdSequencialComecandoEmUm()
    {
        var primeiro = await CriarAsync(new { nome = "Ana Souza" });
        var segundo = await CriarAsync(new { nome = "Carlos Lima" });

        Assert.Equal(1, primeiro.Id);
        Assert.Equal(2, segundo.Id);
    }

    [Fact]
    public async Task S005_R1_IncluirVendedorInformandoId_IdEhIgnorado()
    {
        var vendedor = await CriarAsync(new { id = 50, nome = "Ana Souza" });

        Assert.Equal(1, vendedor.Id);
    }

    // ----- Nome obrigatório e com tamanho entre 3 e 100 (R2) -----

    [Fact]
    public async Task S005_R2_IncluirComNomeVazio_Recusado()
    {
        var resposta = await IncluirAsync(new { nome = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome é obrigatório.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R2_IncluirComNomeSoComEspacos_Recusado()
    {
        var resposta = await IncluirAsync(new { nome = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome é obrigatório.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R2_IncluirComDoisCaracteresAposTrim_Recusado()
    {
        var resposta = await IncluirAsync(new { nome = "  Al  " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R2_IncluirComCentoEUmCaracteres_Recusado()
    {
        var resposta = await IncluirAsync(new { nome = new string('A', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R2_IncluirComTresECemCaracteres_Aceito()
    {
        var tres = await CriarAsync(new { nome = "Ana" });
        var cem = await CriarAsync(new { nome = new string('A', 100) });

        Assert.Equal("Ana", tres.Nome);
        Assert.Equal(100, cem.Nome.Length);
    }

    [Fact]
    public async Task S005_R2_IncluirComEspacosNasPontas_SalvaNomeSemEspacos()
    {
        var vendedor = await CriarAsync(new { nome = "  Ana Souza  " });

        Assert.Equal("Ana Souza", vendedor.Nome);
    }

    // ----- Nome único sem diferenciar maiúsculas (R3) -----

    [Fact]
    public async Task S005_R3_IncluirComNomeJaCadastrado_Recusado()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await IncluirAsync(new { nome = "Ana Souza" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um vendedor com este nome.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R3_IncluirComNomeIgualSemDiferenciarMaiusculas_Recusado()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await IncluirAsync(new { nome = "ana souza" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um vendedor com este nome.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R3_IncluirComNomeDiferente_Aceito()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var vendedor = await CriarAsync(new { nome = "Ana Souza Lima" });

        Assert.Equal("Ana Souza Lima", vendedor.Nome);
    }

    // ----- Status Ativo ou Inativo, padrão Ativo (R4) -----

    [Fact]
    public async Task S005_R4_IncluirSemStatus_EntraComoAtivo()
    {
        var vendedor = await CriarAsync(new { nome = "Ana Souza" });

        Assert.Equal("Ativo", vendedor.Status);
    }

    [Fact]
    public async Task S005_R4_IncluirComStatusInativo_SalvaInativo()
    {
        var vendedor = await CriarAsync(new { nome = "Carlos Lima", status = "Inativo" });

        Assert.Equal("Inativo", vendedor.Status);
    }

    [Theory]
    [InlineData("ativo")]
    [InlineData("ATIVO")]
    [InlineData("atiVO")]
    public async Task S005_R4_IncluirComStatusAtivoEmQualquerCaixa_SalvaAtivo(string status)
    {
        var vendedor = await CriarAsync(new { nome = "Ana Souza", status });

        Assert.Equal("Ativo", vendedor.Status);
    }

    [Fact]
    public async Task S005_R4_IncluirComStatusInativoEmMinusculas_SalvaInativo()
    {
        var vendedor = await CriarAsync(new { nome = "Carlos Lima", status = "inativo" });

        Assert.Equal("Inativo", vendedor.Status);
    }

    [Fact]
    public async Task S005_R4_IncluirComStatusInvalido_Recusado()
    {
        var resposta = await IncluirAsync(new { nome = "Pedro Alves", status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    // ----- Resposta da inclusão (R5) -----

    [Fact]
    public async Task S005_R5_IncluirVendedor_RetornaIdNomeEStatus()
    {
        var resposta = await IncluirAsync(new { nome = "Carlos Lima", status = "Inativo" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var vendedor = await resposta.Content.ReadFromJsonAsync<VendedorResposta>();
        Assert.Equal(new VendedorResposta(1, "Carlos Lima", "Inativo"), vendedor);
    }

    // ----- Alterar nome e/ou status pelo id (R6) -----

    [Fact]
    public async Task S005_R6_AlterarSoONome_MantemOStatus()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await AlterarAsync(1, new { nome = "Ana Paula Souza" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var vendedor = await resposta.Content.ReadFromJsonAsync<VendedorResposta>();
        Assert.Equal(new VendedorResposta(1, "Ana Paula Souza", "Ativo"), vendedor);
    }

    [Fact]
    public async Task S005_R6_AlterarSoOStatus_MantemONome()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await AlterarAsync(1, new { status = "Inativo" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var vendedor = await resposta.Content.ReadFromJsonAsync<VendedorResposta>();
        Assert.Equal(new VendedorResposta(1, "Ana Souza", "Inativo"), vendedor);
    }

    [Fact]
    public async Task S005_R6_AlterarInexistente_NaoEncontrado()
    {
        var resposta = await AlterarAsync(999, new { nome = "Marcos Reis" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R6_AlterarComNomeVazio_Recusado()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await AlterarAsync(1, new { nome = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Nome é obrigatório.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R6_AlterarComStatusInvalido_Recusado()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await AlterarAsync(1, new { status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    // ----- Nome repetido na alteração (R3) -----

    [Fact]
    public async Task S005_R3_AlterarParaNomeJaUsadoPorOutro_Recusado()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima" });

        var resposta = await AlterarAsync(2, new { nome = "Ana Souza" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um vendedor com este nome.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R3_AlterarMantendoOProprioNomeComOutraCaixa_Aceito()
    {
        await CriarAsync(new { nome = "Ana Souza" });

        var resposta = await AlterarAsync(1, new { nome = "ANA SOUZA" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Obter pelo id (R7) -----

    [Fact]
    public async Task S005_R7_ObterVendedorExistente_RetornaSeusDados()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/vendedores/2");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var vendedor = await resposta.Content.ReadFromJsonAsync<VendedorResposta>();
        Assert.Equal(new VendedorResposta(2, "Carlos Lima", "Inativo"), vendedor);
    }

    [Fact]
    public async Task S005_R7_ObterInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.GetAsync("/api/vendedores/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R6_AlteracaoPersisteNoBanco()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await AlterarAsync(1, new { nome = "Ana Paula Souza", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/vendedores/1");

        var vendedor = await resposta.Content.ReadFromJsonAsync<VendedorResposta>();
        Assert.Equal(new VendedorResposta(1, "Ana Paula Souza", "Inativo"), vendedor);
    }

    // ----- Listar com filtro opcional por status (R8) -----

    [Fact]
    public async Task S005_R8_ListarSemFiltro_MostraTodosOrdenadosPorId()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/vendedores");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var vendedores = await resposta.Content.ReadFromJsonAsync<List<VendedorResposta>>();
        Assert.Equal(new List<VendedorResposta>
        {
            new VendedorResposta(1, "Ana Souza", "Ativo"),
            new VendedorResposta(2, "Carlos Lima", "Inativo")
        }, vendedores);
    }

    [Fact]
    public async Task S005_R8_ListarComStatusInativo_RetornaSomenteOsInativos()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/vendedores?status=Inativo");

        var vendedores = await resposta.Content.ReadFromJsonAsync<List<VendedorResposta>>();
        Assert.Equal(new List<VendedorResposta> { new VendedorResposta(2, "Carlos Lima", "Inativo") }, vendedores);
    }

    [Fact]
    public async Task S005_R8_ListarComStatusInvalido_Recusado()
    {
        var resposta = await _cliente.GetAsync("/api/vendedores?status=Pendente");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R8_ListarComStatusInativoEmQualquerCaixa_RetornaOsInativos()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/vendedores?status=iNaTiVo");

        var vendedores = await resposta.Content.ReadFromJsonAsync<List<VendedorResposta>>();
        Assert.Equal(new List<VendedorResposta> { new VendedorResposta(2, "Carlos Lima", "Inativo") }, vendedores);
    }

    [Fact]
    public async Task S005_R8_ListarSemNenhumVendedor_RetornaListaVazia()
    {
        var resposta = await _cliente.GetAsync("/api/vendedores");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var vendedores = await resposta.Content.ReadFromJsonAsync<List<VendedorResposta>>();
        Assert.Empty(vendedores!);
    }

    // ----- Excluir definitivamente (R9) -----

    [Fact]
    public async Task S005_R9_ExcluirVendedor_RemoveDaListagem()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima" });

        var resposta = await _cliente.DeleteAsync("/api/vendedores/2");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var vendedores = await (await _cliente.GetAsync("/api/vendedores")).Content.ReadFromJsonAsync<List<VendedorResposta>>();
        Assert.Equal(new List<VendedorResposta> { new VendedorResposta(1, "Ana Souza", "Ativo") }, vendedores);
    }

    // ----- Excluir id inexistente (R10) -----

    [Fact]
    public async Task S005_R10_ExcluirIdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/vendedores/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Depois de excluído (R11) -----

    [Fact]
    public async Task S005_R11_ObterDepoisDeExcluido_NaoEncontrado()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima" });
        await _cliente.DeleteAsync("/api/vendedores/2");

        var resposta = await _cliente.GetAsync("/api/vendedores/2");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S005_R11_AlterarDepoisDeExcluido_NaoEncontrado()
    {
        await CriarAsync(new { nome = "Ana Souza" });
        await CriarAsync(new { nome = "Carlos Lima" });
        await _cliente.DeleteAsync("/api/vendedores/2");

        var resposta = await AlterarAsync(2, new { nome = "Marcos Reis" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> AlterarAsync(int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/vendedores/{id}", corpo);
    }

    private Task<HttpResponseMessage> IncluirAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/vendedores", corpo);
    }

    private async Task<VendedorResposta> CriarAsync(object corpo)
    {
        var resposta = await IncluirAsync(corpo);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<VendedorResposta>())!;
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record VendedorResposta(int Id, string Nome, string Status);

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
