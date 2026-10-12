using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S007CaixasBancosTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S007CaixasBancosTests()
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
    public async Task S007_R1_IncluirCaixasBancos_IdSequencialComecandoEmUm()
    {
        var primeiro = await CriarAsync(new { descricao = "Caixa Loja Centro" });
        var segundo = await CriarAsync(new { descricao = "Banco do Brasil" });

        Assert.Equal(1, primeiro.Id);
        Assert.Equal(2, segundo.Id);
    }

    [Fact]
    public async Task S007_R1_IncluirInformandoId_IdEhIgnorado()
    {
        var caixa = await CriarAsync(new { id = 50, descricao = "Caixa Loja Centro" });

        Assert.Equal(1, caixa.Id);
    }

    // ----- Descrição obrigatória, entre 3 e 100 caracteres (R2) -----

    [Fact]
    public async Task S007_R2_IncluirComDescricaoVazia_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R2_IncluirComDescricaoSoComEspacos_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R2_IncluirComDoisCaracteresAposTrim_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "  Ca  " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R2_IncluirComCentoEUmCaracteres_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = new string('A', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R2_IncluirComTresECemCaracteres_Aceita()
    {
        var tres = await CriarAsync(new { descricao = "Caixa" });
        var cem = await CriarAsync(new { descricao = new string('A', 100) });

        Assert.Equal("Caixa", tres.Descricao);
        Assert.Equal(100, cem.Descricao.Length);
    }

    [Fact]
    public async Task S007_R2_IncluirComEspacosNasPontas_SalvaDescricaoSemEspacos()
    {
        var caixa = await CriarAsync(new { descricao = "  Caixa Loja Centro  " });

        Assert.Equal("Caixa Loja Centro", caixa.Descricao);
    }

    // ----- Descrição única sem diferenciar maiúsculas (R3) -----

    [Fact]
    public async Task S007_R3_IncluirComDescricaoJaCadastrada_Recusada()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await IncluirAsync(new { descricao = "Caixa Loja Centro" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um caixa ou banco com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R3_IncluirComDescricaoIgualSemDiferenciarMaiusculas_Recusada()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await IncluirAsync(new { descricao = "caixa loja centro" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um caixa ou banco com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R3_IncluirComDescricaoDiferente_Aceita()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var caixa = await CriarAsync(new { descricao = "Caixa Loja Centro 2" });

        Assert.Equal("Caixa Loja Centro 2", caixa.Descricao);
    }

    [Fact]
    public async Task S007_R3_AlterarParaDescricaoJaUsadaPorOutro_Recusada()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil" });

        var resposta = await AlterarAsync(2, new { descricao = "Caixa Loja Centro" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe um caixa ou banco com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R3_AlterarMantendoAPropriaDescricaoComOutraCaixa_Aceita()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await AlterarAsync(1, new { descricao = "CAIXA LOJA CENTRO" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Status Ativo ou Inativo, padrão Ativo (R4) -----

    [Fact]
    public async Task S007_R4_IncluirSemStatus_EntraComoAtivo()
    {
        var caixa = await CriarAsync(new { descricao = "Caixa Loja Centro" });

        Assert.Equal("Ativo", caixa.Status);
    }

    [Fact]
    public async Task S007_R4_IncluirComStatusInativo_SalvaInativo()
    {
        var caixa = await CriarAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        Assert.Equal("Inativo", caixa.Status);
    }

    [Theory]
    [InlineData("ativo")]
    [InlineData("ATIVO")]
    [InlineData("atiVO")]
    public async Task S007_R4_IncluirComStatusAtivoEmQualquerCaixa_SalvaAtivo(string status)
    {
        var caixa = await CriarAsync(new { descricao = "Caixa Loja Centro", status });

        Assert.Equal("Ativo", caixa.Status);
    }

    [Fact]
    public async Task S007_R4_IncluirComStatusInativoEmMinusculas_SalvaInativo()
    {
        var caixa = await CriarAsync(new { descricao = "Banco do Brasil", status = "inativo" });

        Assert.Equal("Inativo", caixa.Status);
    }

    [Fact]
    public async Task S007_R4_IncluirComStatusInvalido_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "Caixa Filial", status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R4_AlterarComStatusInvalido_Recusada()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await AlterarAsync(1, new { status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    // ----- Resposta da inclusão (R5) -----

    [Fact]
    public async Task S007_R5_IncluirCaixaBanco_RetornaIdDescricaoEStatus()
    {
        var resposta = await IncluirAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaBancoResposta>();
        Assert.Equal(new CaixaBancoResposta(1, "Banco do Brasil", "Inativo"), caixa);
    }

    // ----- Alterar descrição e/ou status pelo id (R6) -----

    [Fact]
    public async Task S007_R6_AlterarSoADescricao_MantemOStatus()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await AlterarAsync(1, new { descricao = "Caixa Loja Centro 2" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaBancoResposta>();
        Assert.Equal(new CaixaBancoResposta(1, "Caixa Loja Centro 2", "Ativo"), caixa);
    }

    [Fact]
    public async Task S007_R6_AlterarSoOStatus_MantemADescricao()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });

        var resposta = await AlterarAsync(1, new { status = "Inativo" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaBancoResposta>();
        Assert.Equal(new CaixaBancoResposta(1, "Caixa Loja Centro", "Inativo"), caixa);
    }

    [Fact]
    public async Task S007_R6_AlterarInexistente_NaoEncontrado()
    {
        var resposta = await AlterarAsync(999, new { descricao = "Caixa Filial" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R6_AlteracaoPersisteNoBanco()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await AlterarAsync(1, new { descricao = "Caixa Loja Centro 2", status = "Inativo" });

        var caixa = await (await _cliente.GetAsync("/api/caixas-bancos/1")).Content.ReadFromJsonAsync<CaixaBancoResposta>();

        Assert.Equal(new CaixaBancoResposta(1, "Caixa Loja Centro 2", "Inativo"), caixa);
    }

    // ----- Obter pelo id (R7) -----

    [Fact]
    public async Task S007_R7_ObterCaixaExistente_RetornaSeusDados()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/caixas-bancos/2");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaBancoResposta>();
        Assert.Equal(new CaixaBancoResposta(2, "Banco do Brasil", "Inativo"), caixa);
    }

    [Fact]
    public async Task S007_R7_ObterInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.GetAsync("/api/caixas-bancos/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Listar com filtro opcional por status (R8) -----

    [Fact]
    public async Task S007_R8_ListarSemFiltro_MostraTodosOrdenadosPorId()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/caixas-bancos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caixas = await resposta.Content.ReadFromJsonAsync<List<CaixaBancoResposta>>();
        Assert.Equal(new List<CaixaBancoResposta>
        {
            new CaixaBancoResposta(1, "Caixa Loja Centro", "Ativo"),
            new CaixaBancoResposta(2, "Banco do Brasil", "Inativo")
        }, caixas);
    }

    [Fact]
    public async Task S007_R8_ListarComStatusInativo_RetornaSomenteOsInativos()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        var caixas = await (await _cliente.GetAsync("/api/caixas-bancos?status=Inativo")).Content.ReadFromJsonAsync<List<CaixaBancoResposta>>();

        Assert.Equal(new List<CaixaBancoResposta> { new CaixaBancoResposta(2, "Banco do Brasil", "Inativo") }, caixas);
    }

    [Fact]
    public async Task S007_R8_ListarComStatusInativoEmQualquerCaixa_RetornaOsInativos()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil", status = "Inativo" });

        var caixas = await (await _cliente.GetAsync("/api/caixas-bancos?status=iNaTiVo")).Content.ReadFromJsonAsync<List<CaixaBancoResposta>>();

        Assert.Equal(new List<CaixaBancoResposta> { new CaixaBancoResposta(2, "Banco do Brasil", "Inativo") }, caixas);
    }

    [Fact]
    public async Task S007_R8_ListarComStatusInvalido_Recusada()
    {
        var resposta = await _cliente.GetAsync("/api/caixas-bancos?status=Pendente");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R8_ListarSemNenhumCaixaOuBanco_RetornaListaVazia()
    {
        var resposta = await _cliente.GetAsync("/api/caixas-bancos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var caixas = await resposta.Content.ReadFromJsonAsync<List<CaixaBancoResposta>>();
        Assert.Empty(caixas!);
    }

    // ----- Excluir definitivamente (R9) -----

    [Fact]
    public async Task S007_R9_ExcluirCaixaBanco_RemoveDaListagem()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil" });

        var resposta = await _cliente.DeleteAsync("/api/caixas-bancos/2");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var caixas = await (await _cliente.GetAsync("/api/caixas-bancos")).Content.ReadFromJsonAsync<List<CaixaBancoResposta>>();
        Assert.Equal(new List<CaixaBancoResposta> { new CaixaBancoResposta(1, "Caixa Loja Centro", "Ativo") }, caixas);
    }

    // ----- Excluir id inexistente (R10) -----

    [Fact]
    public async Task S007_R10_ExcluirIdInexistente_NaoEncontrado()
    {
        var resposta = await _cliente.DeleteAsync("/api/caixas-bancos/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Depois de excluído (R11) -----

    [Fact]
    public async Task S007_R11_ObterDepoisDeExcluido_NaoEncontrado()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil" });
        await _cliente.DeleteAsync("/api/caixas-bancos/2");

        var resposta = await _cliente.GetAsync("/api/caixas-bancos/2");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S007_R11_AlterarDepoisDeExcluido_NaoEncontrado()
    {
        await CriarAsync(new { descricao = "Caixa Loja Centro" });
        await CriarAsync(new { descricao = "Banco do Brasil" });
        await _cliente.DeleteAsync("/api/caixas-bancos/2");

        var resposta = await AlterarAsync(2, new { descricao = "Caixa Filial" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Caixa ou banco não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> AlterarAsync(int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/caixas-bancos/{id}", corpo);
    }

    private Task<HttpResponseMessage> IncluirAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/caixas-bancos", corpo);
    }

    private async Task<CaixaBancoResposta> CriarAsync(object corpo)
    {
        var resposta = await IncluirAsync(corpo);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<CaixaBancoResposta>())!;
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record CaixaBancoResposta(int Id, string Descricao, string Status);

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
