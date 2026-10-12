using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Erp.Api.Tests;

public class S006FormasPagamentoTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S006FormasPagamentoTests()
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
    public async Task S006_R1_IncluirFormasPagamento_IdSequencialComecandoEmUm()
    {
        var primeira = await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        var segunda = await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        Assert.Equal(1, primeira.Id);
        Assert.Equal(2, segunda.Id);
    }

    [Fact]
    public async Task S006_R1_IncluirInformandoId_IdEhIgnorado()
    {
        var forma = await CriarAsync(new { id = 50, descricao = "Dinheiro", tipo = 1 });

        Assert.Equal(1, forma.Id);
    }

    // ----- Descrição obrigatória, entre 3 e 100 caracteres (R2) -----

    [Fact]
    public async Task S006_R2_IncluirComDescricaoVazia_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "", tipo = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R2_IncluirComDescricaoSoComEspacos_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "   ", tipo = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição é obrigatória.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R2_IncluirComDoisCaracteresAposTrim_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "  Pi  ", tipo = 4 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R2_IncluirComCentoEUmCaracteres_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = new string('A', 101), tipo = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Descrição deve ter entre 3 e 100 caracteres.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R2_IncluirComTresECemCaracteres_Aceita()
    {
        var tres = await CriarAsync(new { descricao = "Pix", tipo = 4 });
        var cem = await CriarAsync(new { descricao = new string('A', 100), tipo = 7 });

        Assert.Equal("Pix", tres.Descricao);
        Assert.Equal(100, cem.Descricao.Length);
    }

    [Fact]
    public async Task S006_R2_IncluirComEspacosNasPontas_SalvaDescricaoSemEspacos()
    {
        var forma = await CriarAsync(new { descricao = "  Pix Banco X  ", tipo = 4 });

        Assert.Equal("Pix Banco X", forma.Descricao);
    }

    // ----- Descrição única sem diferenciar maiúsculas (R3) -----

    [Fact]
    public async Task S006_R3_IncluirComDescricaoJaCadastrada_Recusada()
    {
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        var resposta = await IncluirAsync(new { descricao = "Pix Banco X", tipo = 4 });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe uma forma de pagamento com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R3_IncluirComDescricaoIgualSemDiferenciarMaiusculas_Recusada()
    {
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        var resposta = await IncluirAsync(new { descricao = "pix banco x", tipo = 4 });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe uma forma de pagamento com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R3_IncluirComDescricaoDiferente_Aceita()
    {
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        var forma = await CriarAsync(new { descricao = "Pix Banco Y", tipo = 4 });

        Assert.Equal("Pix Banco Y", forma.Descricao);
    }

    [Fact]
    public async Task S006_R3_AlterarParaDescricaoJaUsadaPorOutra_Recusada()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        var resposta = await AlterarAsync(2, new { descricao = "Dinheiro" });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Contains("Já existe uma forma de pagamento com esta descrição.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R3_AlterarMantendoAPropriaDescricaoComOutraCaixa_Aceita()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { descricao = "DINHEIRO" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ----- Tipo numérico da tabela (R4) -----

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task S006_R4_IncluirComTipoDaTabela_Aceita(int tipo)
    {
        var forma = await CriarAsync(new { descricao = "Forma " + tipo, tipo });

        Assert.Equal(tipo, forma.Tipo);
    }

    [Fact]
    public async Task S006_R4_IncluirComTipoForaDaTabela_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "Vale", tipo = 8 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Tipo inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R4_IncluirComTipoZero_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "Vale", tipo = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Tipo inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R4_IncluirSemTipo_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "Vale" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Tipo inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R4_IncluirComTipoEmTexto_RecusadaPeloFormatoSemMensagemDeTipo()
    {
        var resposta = await IncluirAsync(new { descricao = "Vale", tipo = "pix" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.DoesNotContain("Tipo inválido.", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task S006_R4_AlterarComTipoForaDaTabela_Recusada()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { tipo = 9 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Tipo inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R4_AlterarComTipoEmTexto_RecusadaPeloFormatoSemMensagemDeTipo()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { tipo = "pix" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.DoesNotContain("Tipo inválido.", await resposta.Content.ReadAsStringAsync());
    }

    // ----- Mais de uma forma de pagamento do mesmo tipo (R5) -----

    [Fact]
    public async Task S006_R5_IncluirDuasFormasDoMesmoTipo_AmbasAceitas()
    {
        var primeira = await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });
        var segunda = await CriarAsync(new { descricao = "Pix Banco Y", tipo = 4 });

        Assert.Equal(4, primeira.Tipo);
        Assert.Equal(4, segunda.Tipo);
        Assert.NotEqual(primeira.Id, segunda.Id);
    }

    // ----- Status Ativo ou Inativo, padrão Ativo (R6) -----

    [Fact]
    public async Task S006_R6_IncluirSemStatus_EntraComoAtivo()
    {
        var forma = await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        Assert.Equal("Ativo", forma.Status);
    }

    [Fact]
    public async Task S006_R6_IncluirComStatusInativo_SalvaInativo()
    {
        var forma = await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        Assert.Equal("Inativo", forma.Status);
    }

    [Theory]
    [InlineData("ativo")]
    [InlineData("ATIVO")]
    [InlineData("atiVO")]
    public async Task S006_R6_IncluirComStatusAtivoEmQualquerCaixa_SalvaAtivo(string status)
    {
        var forma = await CriarAsync(new { descricao = "Dinheiro", tipo = 1, status });

        Assert.Equal("Ativo", forma.Status);
    }

    [Fact]
    public async Task S006_R6_IncluirComStatusInativoEmMinusculas_SalvaInativo()
    {
        var forma = await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "inativo" });

        Assert.Equal("Inativo", forma.Status);
    }

    [Fact]
    public async Task S006_R6_IncluirComStatusInvalido_Recusada()
    {
        var resposta = await IncluirAsync(new { descricao = "Vale", tipo = 7, status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R6_AlterarComStatusInvalido_Recusada()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { status = "Pendente" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    // ----- Resposta da inclusão (R7) -----

    [Fact]
    public async Task S006_R7_IncluirFormaPagamento_RetornaIdDescricaoTipoEStatus()
    {
        var resposta = await IncluirAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var forma = await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>();
        Assert.Equal(new FormaPagamentoResposta(1, "Pix Banco X", 4, "Inativo"), forma);
    }

    // ----- Alterar descrição, tipo e/ou status pelo id (R8) -----

    [Fact]
    public async Task S006_R8_AlterarSoADescricao_MantemOsDemais()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { descricao = "Dinheiro em espécie" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var forma = await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>();
        Assert.Equal(new FormaPagamentoResposta(1, "Dinheiro em espécie", 1, "Ativo"), forma);
    }

    [Fact]
    public async Task S006_R8_AlterarSoOTipo_MantemADescricao()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { tipo = 4 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var forma = await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>();
        Assert.Equal(new FormaPagamentoResposta(1, "Dinheiro", 4, "Ativo"), forma);
    }

    [Fact]
    public async Task S006_R8_AlterarSoOStatus_MantemOsDemais()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await AlterarAsync(1, new { status = "Inativo" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var forma = await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>();
        Assert.Equal(new FormaPagamentoResposta(1, "Dinheiro", 1, "Inativo"), forma);
    }

    [Fact]
    public async Task S006_R8_AlterarInexistente_NaoEncontrada()
    {
        var resposta = await AlterarAsync(999, new { descricao = "Cheque" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R8_AlteracaoPersisteNoBanco()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await AlterarAsync(1, new { descricao = "Dinheiro em espécie", tipo = 4, status = "Inativo" });

        var forma = await (await _cliente.GetAsync("/api/formas-pagamento/1")).Content.ReadFromJsonAsync<FormaPagamentoResposta>();

        Assert.Equal(new FormaPagamentoResposta(1, "Dinheiro em espécie", 4, "Inativo"), forma);
    }

    // ----- Obter pelo id (R9) -----

    [Fact]
    public async Task S006_R9_ObterFormaExistente_RetornaSeusDados()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/formas-pagamento/2");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var forma = await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>();
        Assert.Equal(new FormaPagamentoResposta(2, "Pix Banco X", 4, "Inativo"), forma);
    }

    [Fact]
    public async Task S006_R9_ObterInexistente_NaoEncontrada()
    {
        var resposta = await _cliente.GetAsync("/api/formas-pagamento/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
    }

    // ----- Listar com filtros opcionais por status e tipo (R10) -----

    [Fact]
    public async Task S006_R10_ListarSemFiltro_MostraTodasOrdenadasPorId()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        var resposta = await _cliente.GetAsync("/api/formas-pagamento");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var formas = await resposta.Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();
        Assert.Equal(new List<FormaPagamentoResposta>
        {
            new FormaPagamentoResposta(1, "Dinheiro", 1, "Ativo"),
            new FormaPagamentoResposta(2, "Pix Banco X", 4, "Inativo")
        }, formas);
    }

    [Fact]
    public async Task S006_R10_ListarComStatusInativo_RetornaSomenteAsInativas()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        var formas = await (await _cliente.GetAsync("/api/formas-pagamento?status=Inativo")).Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();

        Assert.Equal(new List<FormaPagamentoResposta> { new FormaPagamentoResposta(2, "Pix Banco X", 4, "Inativo") }, formas);
    }

    [Fact]
    public async Task S006_R10_ListarComStatusInativoEmQualquerCaixa_RetornaAsInativas()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });

        var formas = await (await _cliente.GetAsync("/api/formas-pagamento?status=iNaTiVo")).Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();

        Assert.Equal(new List<FormaPagamentoResposta> { new FormaPagamentoResposta(2, "Pix Banco X", 4, "Inativo") }, formas);
    }

    [Fact]
    public async Task S006_R10_ListarComStatusInvalido_Recusada()
    {
        var resposta = await _cliente.GetAsync("/api/formas-pagamento?status=Pendente");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Status inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R10_ListarComTipo_RetornaSomenteOsDoTipo()
    {
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco Y", tipo = 4 });

        var formas = await (await _cliente.GetAsync("/api/formas-pagamento?tipo=4")).Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();

        Assert.Equal(new List<FormaPagamentoResposta>
        {
            new FormaPagamentoResposta(1, "Pix Banco X", 4, "Ativo"),
            new FormaPagamentoResposta(3, "Pix Banco Y", 4, "Ativo")
        }, formas);
    }

    [Fact]
    public async Task S006_R10_ListarComStatusETipo_RetornaSomenteAsDuasCondicoes()
    {
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4, status = "Inativo" });
        await CriarAsync(new { descricao = "Pix Banco Y", tipo = 4 });

        var formas = await (await _cliente.GetAsync("/api/formas-pagamento?status=Inativo&tipo=4")).Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();

        Assert.Equal(new List<FormaPagamentoResposta> { new FormaPagamentoResposta(1, "Pix Banco X", 4, "Inativo") }, formas);
    }

    [Fact]
    public async Task S006_R10_ListarComTipoForaDaTabela_Recusada()
    {
        var resposta = await _cliente.GetAsync("/api/formas-pagamento?tipo=8");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Tipo inválido.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R10_ListarComTipoEmTexto_RecusadaPeloFormatoSemMensagemDeTipo()
    {
        var resposta = await _cliente.GetAsync("/api/formas-pagamento?tipo=pix");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.DoesNotContain("Tipo inválido.", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task S006_R10_ListarComTipoSemFormasDesseTipo_RetornaListaVazia()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });

        var resposta = await _cliente.GetAsync("/api/formas-pagamento?tipo=6");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var formas = await resposta.Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();
        Assert.Empty(formas!);
    }

    [Fact]
    public async Task S006_R10_ListarSemNenhumaFormaCadastrada_RetornaListaVazia()
    {
        var resposta = await _cliente.GetAsync("/api/formas-pagamento");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var formas = await resposta.Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();
        Assert.Empty(formas!);
    }

    // ----- Excluir definitivamente (R11) -----

    [Fact]
    public async Task S006_R11_ExcluirFormaPagamento_RemoveDaListagem()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });

        var resposta = await _cliente.DeleteAsync("/api/formas-pagamento/2");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var formas = await (await _cliente.GetAsync("/api/formas-pagamento")).Content.ReadFromJsonAsync<List<FormaPagamentoResposta>>();
        Assert.Equal(new List<FormaPagamentoResposta> { new FormaPagamentoResposta(1, "Dinheiro", 1, "Ativo") }, formas);
    }

    // ----- Excluir id inexistente (R12) -----

    [Fact]
    public async Task S006_R12_ExcluirIdInexistente_NaoEncontrada()
    {
        var resposta = await _cliente.DeleteAsync("/api/formas-pagamento/999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
    }

    // ----- Depois de excluída (R13) -----

    [Fact]
    public async Task S006_R13_ObterDepoisDeExcluida_NaoEncontrada()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });
        await _cliente.DeleteAsync("/api/formas-pagamento/2");

        var resposta = await _cliente.GetAsync("/api/formas-pagamento/2");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S006_R13_AlterarDepoisDeExcluida_NaoEncontrada()
    {
        await CriarAsync(new { descricao = "Dinheiro", tipo = 1 });
        await CriarAsync(new { descricao = "Pix Banco X", tipo = 4 });
        await _cliente.DeleteAsync("/api/formas-pagamento/2");

        var resposta = await AlterarAsync(2, new { descricao = "Cheque" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Forma de pagamento não encontrada.", await TextoErroAsync(resposta));
    }

    // ----- Auxiliares -----

    private Task<HttpResponseMessage> AlterarAsync(int id, object corpo)
    {
        return _cliente.PutAsJsonAsync($"/api/formas-pagamento/{id}", corpo);
    }

    private Task<HttpResponseMessage> IncluirAsync(object corpo)
    {
        return _cliente.PostAsJsonAsync("/api/formas-pagamento", corpo);
    }

    private async Task<FormaPagamentoResposta> CriarAsync(object corpo)
    {
        var resposta = await IncluirAsync(corpo);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<FormaPagamentoResposta>())!;
    }

    private static async Task<string> TextoErroAsync(HttpResponseMessage resposta)
    {
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return ReunirTextos(documento.RootElement);
    }

    private record FormaPagamentoResposta(int Id, string Descricao, int Tipo, string Status);

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
