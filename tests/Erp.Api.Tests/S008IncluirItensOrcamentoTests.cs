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
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(1, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Equal(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), doc.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task S008_R1_OrcamentosCriadosEmSequencia_TemNumerosDiferentes()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        using var primeiro = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1));
        using var segundo = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1));

        Assert.Equal(1, primeiro.RootElement.GetProperty("id").GetInt32());
        Assert.Equal(2, segundo.RootElement.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task S008_R1_OrcamentosCriadosAoMesmoTempo_NaoRecebemMesmoNumero()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var tarefas = Enumerable.Range(0, 5).Select(_ => IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1));
        var respostas = await Task.WhenAll(tarefas);

        var numeros = new List<int>();

        foreach (var resposta in respostas)
        {
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
            using var doc = await LerAsync(resposta);
            numeros.Add(doc.RootElement.GetProperty("id").GetInt32());
        }

        Assert.Equal(5, numeros.Distinct().Count());
    }

    // ----- Cliente e vendedor obrigatórios (R2) -----

    [Fact]
    public async Task S008_R2_SemCliente_RecusadoENaoGrava()
    {
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { vendedorId, codigoProduto = "P001", quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R2_SemVendedor_RecusadoENaoGrava()
    {
        int clienteId = await CriarClienteAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, codigoProduto = "P001", quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Vendedor é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Cliente não encontrado (R3) -----

    [Fact]
    public async Task S008_R3_ClienteInexistente_RecusadoENaoCriaOrcamento()
    {
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(99, vendedorId, "P001", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Cliente não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Vendedor não encontrado (R4) -----

    [Fact]
    public async Task S008_R4_VendedorInexistente_RecusadoENaoCriaOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(clienteId, 99, "P001", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Vendedor não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Cadastro inativo (R5) -----

    [Fact]
    public async Task S008_R5_ClienteInativo_RecusadoENaoCriaOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        await _cliente.PostAsync($"/api/clientes/{clienteId}/desativar", null);
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Cliente inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R5_VendedorInativo_RecusadoENaoCriaOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync("João Lima", "Inativo");
        await CriarProdutoAsync("P001", 3.50m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Vendedor inativo não pode ser usado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto obrigatório e inexistente (R6) -----

    [Fact]
    public async Task S008_R6_ItemSemProduto_RecusadoENaoCriaOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, quantidade = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto é obrigatório.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R6_ProdutoInexistente_RecusadoENaoCriaOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P009", 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Produto não encontrado.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto inativo (R7) -----

    [Fact]
    public async Task S008_R7_ProdutoInativo_RecusadoENaoEntraNoOrcamento()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P003", 2.00m, "inativo");

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P003", 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Produto inativo não pode ser incluído.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Produto sem estoque (R8) -----

    [Fact]
    public async Task S008_R8_ProdutoSemEstoque_AceitoEEstoqueContinuaZero()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P004", 4.00m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P004", 1);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P004"));
        Assert.Equal(0m, produto.RootElement.GetProperty("estoque").GetDecimal());
    }

    // ----- Quantidade (R9) -----

    [Fact]
    public async Task S008_R9_QuantidadeZero_RecusadaENaoGrava()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P002", 0);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    [Fact]
    public async Task S008_R9_QuantidadeNegativa_RecusadaENaoGrava()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P002", -1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ser maior que zero.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S008_R9_QuantidadeComTresCasas_Recusada()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P002", 1.234m);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade deve ter até 2 casas decimais.", await TextoErroAsync(resposta));
    }

    [Fact]
    public async Task S008_R9_QuantidadeFracionada_Aceita()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "P002", 0.5m);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(0.50m, doc.RootElement.GetProperty("itens")[0].GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task S008_R9_QuantidadeAusente_RecusadaENaoGrava()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, codigoProduto = "P002" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("Quantidade é obrigatória.", await TextoErroAsync(resposta));
        Assert.Empty(await ListarAsync(""));
    }

    // ----- Preço copiado do produto (R10) -----

    [Fact]
    public async Task S008_R10_AlteracaoDoPrecoDoProduto_NaoMudaOPrecoDoItemJaGravado()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        await _cliente.PutAsJsonAsync("/api/produtos/P001", new { descricao = "Caneta azul", precoVenda = 9.00m, status = "ativo" });

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}"));
        Assert.Equal(3.50m, consulta.RootElement.GetProperty("itens")[0].GetProperty("preco").GetDecimal());
    }

    // ----- Total do item (R11) -----

    [Fact]
    public async Task S008_R11_TotalDoItemEQuantidadeVezesPreco()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        using var doc = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));

        Assert.Equal(7.00m, doc.RootElement.GetProperty("itens")[0].GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task S008_R11_TotalArredondadoParaDuasCasas_MeioParaCima()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P005", 0.50m);

        using var doc = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P005", 0.25m));

        Assert.Equal(0.13m, doc.RootElement.GetProperty("itens")[0].GetProperty("total").GetDecimal());
    }

    // ----- Subtotal (R12) -----

    [Fact]
    public async Task S008_R12_SubtotalEASomaDosTotaisDosItens()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        await IncluirItemAsync(orcamentoId, "P002", 3);
        await IncluirItemAsync(orcamentoId, "P001", 1);

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}"));
        Assert.Equal(13.50m, consulta.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Totais enviados são ignorados (R13) -----

    [Fact]
    public async Task S008_R13_TotalEnviadoNaRequisicao_EIgnorado()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P002", 1.00m);

        var resposta = await _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, codigoProduto = "P002", quantidade = 1, total = 999.00m, subtotal = 999.00m });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        Assert.Equal(1.00m, doc.RootElement.GetProperty("itens")[0].GetProperty("total").GetDecimal());
        Assert.Equal(1.00m, doc.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Mesmo produto em itens independentes (R14) -----

    [Fact]
    public async Task S008_R14_MesmoProdutoIncluidoDeNovo_CriaItemIndependente()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        await IncluirItemAsync(orcamentoId, "P002", 3);

        using var doc = await LerAsync(await IncluirItemAsync(orcamentoId, "P001", 1));

        var itens = doc.RootElement.GetProperty("itens");
        Assert.Equal(3, itens.GetArrayLength());
        Assert.Equal(3.50m, itens[2].GetProperty("total").GetDecimal());
        Assert.Equal(13.50m, doc.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Cada item é salvo na hora (R15) -----

    [Fact]
    public async Task S008_R15_CadaItemAparece_LogoApósSerIncluido()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();

        await IncluirItemAsync(orcamentoId, "P002", 3);

        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}"));
        Assert.Equal(2, consulta.RootElement.GetProperty("itens").GetArrayLength());
    }

    // ----- Falha na gravação do item (R16) -----

    [Fact]
    public async Task S008_R16_FalhaAoGravarPrimeiroItem_OrcamentoContinuaSemItens()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("FALHA", 1.00m);
        CriarGatilhoDeFalhaParaProduto("FALHA");

        var resposta = await IncluirOrcamentoAsync(clienteId, vendedorId, "FALHA", 1);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        using var consulta = await LerAsync(await _cliente.GetAsync("/api/orcamentos/1"));
        Assert.Equal(1, consulta.RootElement.GetProperty("id").GetInt32());
        Assert.Empty(consulta.RootElement.GetProperty("itens").EnumerateArray());
        Assert.Equal(0m, consulta.RootElement.GetProperty("subtotal").GetDecimal());
    }

    [Fact]
    public async Task S008_R16_FalhaAoGravarItemSeguinte_OrcamentoEItensJaGravadosPermanecem()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("FALHA", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        CriarGatilhoDeFalhaParaProduto("FALHA");

        var resposta = await IncluirItemAsync(orcamentoId, "FALHA", 1);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        using var consulta = await LerAsync(await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}"));
        Assert.Equal(1, consulta.RootElement.GetProperty("itens").GetArrayLength());
        Assert.Equal(7.00m, consulta.RootElement.GetProperty("subtotal").GetDecimal());
    }

    // ----- Consulta do orçamento (R17) -----

    [Fact]
    public async Task S008_R17_ConsultarOrcamento_RetornaCabecalhoItensESubtotal()
    {
        int clienteId = await CriarClienteAsync("Maria Silva");
        int vendedorId = await CriarVendedorAsync("João Lima");
        await CriarProdutoAsync("P001", 3.50m);
        await CriarProdutoAsync("P002", 1.00m);
        using var criado = await LerAsync(await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2));
        int orcamentoId = criado.RootElement.GetProperty("id").GetInt32();
        await IncluirItemAsync(orcamentoId, "P002", 3);
        await IncluirItemAsync(orcamentoId, "P001", 1);

        var resposta = await _cliente.GetAsync($"/api/orcamentos/{orcamentoId}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var doc = await LerAsync(resposta);
        var raiz = doc.RootElement;
        Assert.Equal(1, raiz.GetProperty("id").GetInt32());
        Assert.Equal(clienteId, raiz.GetProperty("cliente").GetProperty("id").GetInt32());
        Assert.Equal("Maria Silva", raiz.GetProperty("cliente").GetProperty("nome").GetString());
        Assert.Equal(vendedorId, raiz.GetProperty("vendedor").GetProperty("id").GetInt32());
        Assert.Equal("João Lima", raiz.GetProperty("vendedor").GetProperty("nome").GetString());
        Assert.Equal(3, raiz.GetProperty("itens").GetArrayLength());
        Assert.Equal(13.50m, raiz.GetProperty("subtotal").GetDecimal());
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
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1);
        await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1);

        using var doc = await LerAsync(await _cliente.GetAsync("/api/orcamentos"));

        var lista = doc.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, lista.Count);
        Assert.Equal(1, lista[0].GetProperty("id").GetInt32());
        Assert.Equal(2, lista[1].GetProperty("id").GetInt32());
        Assert.Equal("Maria Silva", lista[0].GetProperty("cliente").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task S008_R18_FiltroPorCliente_MostraSomenteDoCliente()
    {
        int cliente1 = await CriarClienteAsync("Maria Silva");
        int cliente2 = await CriarClienteAsync("Ana Lima");
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(cliente1, vendedorId, "P001", 1);
        await IncluirOrcamentoAsync(cliente2, vendedorId, "P001", 1);

        var lista = await ListarAsync($"?clienteId={cliente1}");

        Assert.Single(lista);
        Assert.Equal(cliente1, lista[0].GetProperty("cliente").GetProperty("id").GetInt32());
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

        var lista = await ListarAsync($"?clienteId={cliente1}&vendedorId={vendedor1}");

        Assert.Single(lista);
        Assert.Equal(vendedor1, lista[0].GetProperty("vendedor").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task S008_R18_FiltroPorClienteInexistente_RetornaListaVazia()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);
        await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 1);

        var resposta = await _cliente.GetAsync("/api/orcamentos?clienteId=99");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(await ListarAsync("?clienteId=99"));
    }

    // ----- Incluir item não altera estoque nem caixa (R19) -----

    [Fact]
    public async Task S008_R19_IncluirItem_NaoAlteraEstoqueDoProduto()
    {
        int clienteId = await CriarClienteAsync();
        int vendedorId = await CriarVendedorAsync();
        await CriarProdutoAsync("P001", 3.50m);

        await IncluirOrcamentoAsync(clienteId, vendedorId, "P001", 2);

        using var produto = await LerAsync(await _cliente.GetAsync("/api/produtos/P001"));
        Assert.Equal(0m, produto.RootElement.GetProperty("estoque").GetDecimal());
    }

    // ----- Auxiliares -----

    private async Task<int> CriarClienteAsync(string nome = "Maria Silva", string status = "ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/clientes", new { nome, endereco = "Rua A, 1", telefone = "11 90000-0001", status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarVendedorAsync(string nome = "João Lima", string status = "Ativo")
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/vendedores", new { nome, status });
        using var doc = await LerAsync(resposta);
        return doc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task CriarProdutoAsync(string codigo, decimal precoVenda, string status = "ativo")
    {
        await _cliente.PostAsJsonAsync("/api/produtos", new { codigo, descricao = "Produto " + codigo, precoVenda, status });
    }

    private Task<HttpResponseMessage> IncluirOrcamentoAsync(int clienteId, int vendedorId, string codigoProduto, decimal quantidade)
    {
        return _cliente.PostAsJsonAsync("/api/orcamentos", new { clienteId, vendedorId, codigoProduto, quantidade });
    }

    private Task<HttpResponseMessage> IncluirItemAsync(int orcamentoId, string codigoProduto, decimal quantidade)
    {
        return _cliente.PostAsJsonAsync($"/api/orcamentos/{orcamentoId}/itens", new { codigoProduto, quantidade });
    }

    private async Task<List<JsonElement>> ListarAsync(string filtro)
    {
        using var doc = await LerAsync(await _cliente.GetAsync("/api/orcamentos" + filtro));
        return doc.RootElement.EnumerateArray().Select(elemento => elemento.Clone()).ToList();
    }

    private void CriarGatilhoDeFalhaParaProduto(string codigoProduto)
    {
        using var conexao = new SqliteConnection($"Data Source={_api.ArquivoBanco}");
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = $"create trigger falha_item before insert on orcamento_item when new.codigo_produto = '{codigoProduto}' begin select raise(abort, 'falha simulada'); end;";
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
