using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Erp.Api.Tests;

public class S008IncluirItensOrcamentoTests : IDisposable
{
    private readonly ApiFixture _api;
    private readonly HttpClient _cliente;

    public S008IncluirItensOrcamentoTests()
    {
        _api = new ApiFixture();
        _cliente = _api.CreateClient();
    }

    public void Dispose()
    {
        _cliente.Dispose();
        _api.Dispose();
    }

    // ----- Criação do orçamento pelo primeiro item (R1) -----

    [Fact]
    public async Task S008_R1_PrimeiroItem_CriaOrcamentoComNumeroEDataDeHoje()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(1, doc.RootElement.GetProperty("orca_id").GetInt32());
        Assert.Equal(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), doc.RootElement.GetProperty("orca_data").GetString());
    }

    [Fact]
    public async Task S008_R1_OrcamentosCriadosEmSequencia_TemNumerosDiferentes()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        using var primeiro = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1));
        using var segundo = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1));

        Assert.Equal(1, primeiro.RootElement.GetProperty("orca_id").GetInt32());
        Assert.Equal(2, segundo.RootElement.GetProperty("orca_id").GetInt32());
    }

    [Fact]
    public async Task S008_R1_OrcamentosCriadosAoMesmoTempo_NaoRecebemMesmoNumero()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var tarefas = Enumerable.Range(0, 5).Select(_ => IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1));
        var respostas = await Task.WhenAll(tarefas);

        var numeros = new List<int>();

        foreach (var resposta in respostas)
        {
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
            using var doc = await LerAsync(resposta);
            numeros.Add(doc.RootElement.GetProperty("orca_id").GetInt32());
        }

        Assert.Equal(5, numeros.Distinct().Count());
    }

    // ----- Cliente e vendedor obrigatórios (R2) -----

    [Fact]
    public async Task S008_R2_SemCliente_RecusadoENaoGrava()
    {
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Vend_id, Prod_codigo = "P001", Orci_quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R2_SemVendedor_RecusadoENaoGrava()
    {
        int Clie_id = await CriarClienteAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Prod_codigo = "P001", Orci_quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Vendedor é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Cliente não encontrado (R3) -----

    [Fact]
    public async Task S008_R3_ClienteInexistente_RecusadoENaoCriaOrcamento()
    {
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(99, Vend_id, "P001", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Vendedor não encontrado (R4) -----

    [Fact]
    public async Task S008_R4_VendedorInexistente_RecusadoENaoCriaOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, 99, "P001", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Cadastro inativo (R5) -----

    [Fact]
    public async Task S008_R5_ClienteInativo_RecusadoENaoCriaOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        await _cliente.PostAsync($"/api/clientes/{Clie_id}/desativar", null);
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R5_VendedorInativo_RecusadoENaoCriaOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync("João Lima", "Inativo");
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Vendedor inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto obrigatório e inexistente (R6) -----

    [Fact]
    public async Task S008_R6_ItemSemProduto_RecusadoENaoCriaOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Orci_quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R6_ProdutoInexistente_RecusadoENaoCriaOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P009", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto inativo (R7) -----

    [Fact]
    public async Task S008_R7_ProdutoInativo_RecusadoENaoEntraNoOrcamento()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P003", 2.00m, "inativo");

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P003", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser incluído.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto sem estoque (R8) -----

    [Fact]
    public async Task S008_R8_ProdutoSemEstoque_AceitoEEstoqueContinuaZero()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P004", 4.00m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P004", 1);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("prod_estoque").GetDecimal());
    }

    // ----- Quantidade (R9) -----

    [Fact]
    public async Task S008_R9_QuantidadeZero_RecusadaENaoGrava()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P002", 0);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R9_QuantidadeNegativa_RecusadaENaoGrava()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P002", -1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S008_R9_QuantidadeComTresCasas_Recusada()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P002", 1.234m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S008_R9_QuantidadeFracionada_Aceita()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "P002", 0.5m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(0.50m, doc.RootElement.GetProperty("itens")[0].GetProperty("orci_total").GetDecimal());
    }

    [Fact]
    public async Task S008_R9_QuantidadeAusente_RecusadaENaoGrava()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo = "P002" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade é obrigatória.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Preço copiado do produto (R10) -----

    [Fact]
    public async Task S008_R10_AlteracaoDoPrecoDoProduto_NaoMudaOPrecoDoItemJaGravado()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        await _cliente.PutAsJsonAsync("/api/produtos/P001", new { Prod_descricao = "Caneta azul", Prod_precovenda = 9.00m, Prod_status = "ativo" });

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{Orca_id}"));
        Assert.Equal(3.50m, consulta.RootElement.GetProperty("itens")[0].GetProperty("orci_preco").GetDecimal());
    }

    // ----- Total do item (R11) -----

    [Fact]
    public async Task S008_R11_TotalDoItemEQuantidadeVezesPreco()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        using var doc = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));

        Assert.Equal(7.00m, doc.RootElement.GetProperty("itens")[0].GetProperty("orci_total").GetDecimal());
    }

    [Fact]
    public async Task S008_R11_TotalArredondadoParaDuasCasas_MeioParaCima()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P005", 0.50m);

        using var doc = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P005", 0.25m));

        Assert.Equal(0.13m, doc.RootElement.GetProperty("itens")[0].GetProperty("orci_total").GetDecimal());
    }

    // ----- Subtotal (R12) -----

    [Fact]
    public async Task S008_R12_SubtotalEASomaDosTotaisDosItens()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        await IncluirItemAsync(Orca_id, "P002", 3);
        await IncluirItemAsync(Orca_id, "P001", 1);

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{Orca_id}"));
        Assert.Equal(13.50m, consulta.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Totais enviados são ignorados (R13) -----

    [Fact]
    public async Task S008_R13_TotalEnviadoNaRequisicao_EIgnorado()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo = "P002", Orci_quantidade = 1, total = 999.00m, subtotal = 999.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(1.00m, doc.RootElement.GetProperty("itens")[0].GetProperty("orci_total").GetDecimal());
        Assert.Equal(1.00m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Mesmo produto em itens independentes (R14) -----

    [Fact]
    public async Task S008_R14_MesmoProdutoIncluidoDeNovo_CriaItemIndependente()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await IncluirItemAsync(Orca_id, "P002", 3);

        using var doc = await LerAsync(await IncluirItemAsync(Orca_id, "P001", 1));

        var itens = doc.RootElement.GetProperty("itens");
        Assert.Equal(3, itens.GetArrayLength());
        Assert.Equal(3.50m, itens[2].GetProperty("orci_total").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Cada item é salvo na hora (R15) -----

    [Fact]
    public async Task S008_R15_CadaItemAparece_LogoApósSerIncluido()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();

        await IncluirItemAsync(Orca_id, "P002", 3);

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{Orca_id}"));
        Assert.Equal(2, consulta.RootElement.GetProperty("itens").GetArrayLength());
    }

    // ----- Falha na gravação do item (R16) -----

    [Fact]
    public async Task S008_R16_FalhaAoGravarPrimeiroItem_OrcamentoContinuaSemItens()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("FALHA", 1.00m);
        CriarGatilhoDeFalhaParaProduto("FALHA");

        var resposta = await IncluirOrcamentoAsync(Clie_id, Vend_id, "FALHA", 1);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        using var consulta = await LerAsync(await _cliente.GetAsync("/api/orcamentos/1"));
        Assert.Equal(1, consulta.RootElement.GetProperty("orca_id").GetInt32());
        Assert.Empty(consulta.RootElement.GetProperty("itens").EnumerateArray());
        Assert.Equal(0m, consulta.RootElement.GetProperty("orca_total").GetDecimal());
    }

    [Fact]
    public async Task S008_R16_FalhaAoGravarItemSeguinte_OrcamentoEItensJaGravadosPermanecem()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("FALHA", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        CriarGatilhoDeFalhaParaProduto("FALHA");

        var resposta = await IncluirItemAsync(Orca_id, "FALHA", 1);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{Orca_id}"));
        Assert.Equal(1, consulta.RootElement.GetProperty("itens").GetArrayLength());
        Assert.Equal(7.00m, consulta.RootElement.GetProperty("orca_total").GetDecimal());
    }

    // ----- Consulta do orçamento (R17) -----

    [Fact]
    public async Task S008_R17_ConsultarOrcamento_RetornaCabecalhoItensESubtotal()
    {
        int Clie_id = await CriarClienteAsync("Maria Silva");
        int Vend_id = await CriarVendedorAsync("João Lima");
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2));
        int Orca_id = criado.RootElement.GetProperty("orca_id").GetInt32();
        await IncluirItemAsync(Orca_id, "P002", 3);
        await IncluirItemAsync(Orca_id, "P001", 1);

        var resposta = await _cliente.GetAsync($"/api/orcamentos/{Orca_id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        var raiz = doc.RootElement;
        Assert.Equal(1, raiz.GetProperty("orca_id").GetInt32());
        Assert.Equal(Clie_id, raiz.GetProperty("cliente").GetProperty("clie_id").GetInt32());
        Assert.Equal("Maria Silva", raiz.GetProperty("cliente").GetProperty("clie_nome").GetString());
        Assert.Equal(Vend_id, raiz.GetProperty("vendedor").GetProperty("vend_id").GetInt32());
        Assert.Equal("João Lima", raiz.GetProperty("vendedor").GetProperty("vend_nome").GetString());
        Assert.Equal(3, raiz.GetProperty("itens").GetArrayLength());
        Assert.Equal(13.50m, raiz.GetProperty("orca_total").GetDecimal());
    }

    [Fact]
    public async Task S008_R17_OrcamentoInexistente_RetornaErro()
    {
        var resposta = await _cliente.GetAsync("/api/orcamentos/50");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Orçamento não encontrado.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S008_R17_IncluirItemEmOrcamentoInexistente_RetornaErro()
    {
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirItemAsync(50, "P001", 1);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Orçamento não encontrado.", await TextoErroAsync(resposta));
    }

    // ----- Listagem (R18) -----

    [Fact]
    public async Task S008_R18_ListarSemFiltro_MostraTodosOrdenadosPeloNumero()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1);
        await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1);

        using var doc = await LerAsync(await _cliente.GetAsync("/api/orcamentos"));

        var lista = doc.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, lista.Count);
        Assert.Equal(1, lista[0].GetProperty("orca_id").GetInt32());
        Assert.Equal(2, lista[1].GetProperty("orca_id").GetInt32());
        Assert.Equal("Maria Silva", lista[0].GetProperty("cliente").GetProperty("clie_nome").GetString());
    }

    [Fact]
    public async Task S008_R18_FiltroPorCliente_MostraSomenteDoCliente()
    {
        int cliente1 = await CriarClienteAsync("Maria Silva");
        int cliente2 = await CriarClienteAsync("Ana Lima");
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(cliente1, Vend_id, "P001", 1);
        await IncluirOrcamentoAsync(cliente2, Vend_id, "P001", 1);

        var lista = await ListarAsync($"?Clie_id={cliente1}");

        Assert.Single(lista);
        Assert.Equal(cliente1, lista[0].GetProperty("cliente").GetProperty("clie_id").GetInt32());
    }

    [Fact]
    public async Task S008_R18_FiltroPorClienteEVendedor_MostraApenasOsDoisJuntos()
    {
        int cliente1 = await CriarClienteAsync("Maria Silva");
        int cliente2 = await CriarClienteAsync("Ana Lima");
        int vendedor1 = await CriarVendedorAsync("João Lima");
        int vendedor2 = await CriarVendedorAsync("Paulo Souza");
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(cliente1, vendedor1, "P001", 1);
        await IncluirOrcamentoAsync(cliente1, vendedor2, "P001", 1);
        await IncluirOrcamentoAsync(cliente2, vendedor1, "P001", 1);

        var lista = await ListarAsync($"?Clie_id={cliente1}&Vend_id={vendedor1}");

        Assert.Single(lista);
        Assert.Equal(vendedor1, lista[0].GetProperty("vendedor").GetProperty("vend_id").GetInt32());
    }

    [Fact]
    public async Task S008_R18_FiltroPorClienteInexistente_RetornaListaVazia()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 1);

        var resposta = await _cliente.GetAsync("/api/orcamentos?Clie_id=99");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await ListarAsync("?Clie_id=99"));
    }

    // ----- Incluir item não altera estoque nem caixa (R19) -----

    [Fact]
    public async Task S008_R19_IncluirItem_NaoAlteraEstoqueDoProduto()
    {
        int Clie_id = await CriarClienteAsync();
        int Vend_id = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        await IncluirOrcamentoAsync(Clie_id, Vend_id, "P001", 2);

        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P001"));
        Assert.Equal(0m, produto.RootElement.GetProperty("prod_estoque").GetDecimal());
    }

    // ----- Auxiliares -----

    private async Task<int> CriarClienteAsync(string nome = "Maria Silva", string status = "ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { Clie_nome = nome, Clie_endereco = "Rua A, 1", Clie_telefone = "11 90000-0001", Clie_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("clie_id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome = "João Lima", string status = "Ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { Vend_nome = nome, Vend_status = status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("vend_id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda, string status = "ativo")
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { Prod_codigo = codigo, Prod_descricao = "Produto " + codigo, Prod_precovenda = precoVenda, Prod_status = status });
    }

    private Task<HttpResponseMessage> IncluirOrcamentoAsync(int Clie_id, int Vend_id, string Prod_codigo, decimal Orci_quantidade)
    {
        return _cliente.PostAsJsonAsync("/api/orcamentos", new { Clie_id, Vend_id, Prod_codigo, Orci_quantidade });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int Orca_id, string Prod_codigo, decimal Orci_quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{Orca_id}/itens", new { Prod_codigo, Orci_quantidade });
    }

    private async Task<List<JsonElement>> ListarAsync(string filtro)
    {
        using var doc = await LerAsync(await _cliente.GetAsync("/api/orcamentos" + filtro));
        return doc.RootElement.EnumerateArray().Select(elemento => elemento.Clone()).ToList();
    }

    private void CriarGatilhoDeFalhaParaProduto(string Prod_codigo)
    {
        using var conexao = new SqliteConnection($"Data Source={_api.ArquivoBanco};Pooling=False");
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = $"create trigger falha_item before insert on orcamento_item when new.codigo_produto = '{Prod_codigo}' begin select raise(abort, 'falha simulada'); end;";
        comando.ExecuteNonQuery();
    }

    private static async Task<JsonDocument> LerAsync(HttpResponseMessage resposta)
    {
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
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
}
