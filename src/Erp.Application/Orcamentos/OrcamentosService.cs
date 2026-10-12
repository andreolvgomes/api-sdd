using System.Globalization;
using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Orcamentos;
using Erp.Application.Abstractions.Orcamentos.Dtos;
using Erp.Core.Entities;
using Erp.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Erp.Application.Orcamentos;

public class OrcamentosService : IOrcamentosService
{
    private readonly IOrcamentoRepository _repositorio;
    private readonly IClienteRepository _clientes;
    private readonly IVendedorRepository _vendedores;
    private readonly IProdutoRepository _produtos;
    private readonly ILogger<OrcamentosService> _log;

    public OrcamentosService(
        IOrcamentoRepository repositorio,
        IClienteRepository clientes,
        IVendedorRepository vendedores,
        IProdutoRepository produtos,
        ILogger<OrcamentosService> log)
    {
        _repositorio = repositorio;
        _clientes = clientes;
        _vendedores = vendedores;
        _produtos = produtos;
        _log = log;
    }

    public async Task<OrcamentoResponse> IncluirAsync(IncluirOrcamentoRequest request)
    {
        if (request.ClienteId == null)
            throw new ErroDeNegocio("Cliente é obrigatório.", TipoErroDeNegocio.Validacao);

        if (request.VendedorId == null)
            throw new ErroDeNegocio("Vendedor é obrigatório.", TipoErroDeNegocio.Validacao);

        string? erro = OrcamentoValidacao.ValidarItem(request.CodigoProduto, request.Quantidade);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = await ValidarClienteAsync(request.ClienteId.Value);
        Vendedor vendedor = await ValidarVendedorAsync(request.VendedorId.Value);
        Produto produto = await ValidarProdutoAsync(request.CodigoProduto!, "Produto inativo não pode ser incluído.");

        Orcamento orcamento = new Orcamento
        {
            Data = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ClienteId = cliente.Id,
            VendedorId = vendedor.Id
        };

        orcamento.Id = await _repositorio.InserirAsync(orcamento);

        await GravarItemAsync(orcamento.Id, produto, request.Quantidade!.Value);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> IncluirItemAsync(IncluirItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Id);

        string? erro = OrcamentoValidacao.ValidarItem(request.CodigoProduto, request.Quantidade);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Produto produto = await ValidarProdutoAsync(request.CodigoProduto!, "Produto inativo não pode ser incluído.");

        await GravarItemAsync(orcamento.Id, produto, request.Quantidade!.Value);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> AlterarAsync(AlterarOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Id);

        if (request.ClienteId == null && request.VendedorId == null)
            throw new ErroDeNegocio("Informe o cliente ou o vendedor para alterar.", TipoErroDeNegocio.Validacao);

        if (request.ClienteId != null)
        {
            Cliente cliente = await ValidarClienteAsync(request.ClienteId.Value);
            orcamento.ClienteId = cliente.Id;
        }

        if (request.VendedorId != null)
        {
            Vendedor vendedor = await ValidarVendedorAsync(request.VendedorId.Value);
            orcamento.VendedorId = vendedor.Id;
        }

        await _repositorio.AtualizarAsync(orcamento);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> AlterarItemAsync(AlterarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Id, request.NumeroItem);

        if (item.Cancelado)
            throw new ErroDeNegocio("Item cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        if (request.CodigoProduto == null && request.Quantidade == null)
            throw new ErroDeNegocio("Informe a quantidade ou o produto para alterar.", TipoErroDeNegocio.Validacao);

        if (request.CodigoProduto != null && string.IsNullOrWhiteSpace(request.CodigoProduto))
            throw new ErroDeNegocio("Produto é obrigatório.", TipoErroDeNegocio.Validacao);

        if (request.Quantidade != null)
        {
            string? erro = OrcamentoValidacao.ValidarQuantidade(request.Quantidade);

            if (erro != null)
                throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);
        }

        decimal quantidade = request.Quantidade ?? item.Quantidade;
        string codigoProduto = item.CodigoProduto;
        decimal preco = item.Preco;

        if (request.CodigoProduto != null)
        {
            Produto produto = await ValidarProdutoAsync(request.CodigoProduto, "Produto inativo não pode ser usado.");

            if (produto.Codigo != item.CodigoProduto)
                preco = produto.PrecoVenda;

            codigoProduto = produto.Codigo;
        }

        item.CodigoProduto = codigoProduto;
        item.Quantidade = quantidade;
        item.Preco = preco;
        item.Total = OrcamentoValidacao.Arredondar(quantidade * preco);

        await _repositorio.AtualizarItemAsync(item);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> CancelarItemAsync(CancelarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Id, request.NumeroItem);

        if (item.Cancelado)
            throw new ErroDeNegocio("Item já está cancelado.", TipoErroDeNegocio.Validacao);

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Id, item.NumeroItem, true);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> ReativarItemAsync(ReativarItemOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoAbertoAsync(request.Id);
        OrcamentoItem item = await ObterItemExistenteAsync(orcamento.Id, request.NumeroItem);

        if (!item.Cancelado)
            throw new ErroDeNegocio("Item não está cancelado.", TipoErroDeNegocio.Validacao);

        await ValidarProdutoAsync(item.CodigoProduto, "Produto inativo não pode ser usado.");

        await _repositorio.DefinirCanceladoItemAsync(orcamento.Id, item.NumeroItem, false);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> ObterAsync(ObterOrcamentoRequest request)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(request.Id);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<List<OrcamentoResumoResponse>> ListarAsync(ListarOrcamentosRequest request)
    {
        List<Orcamento> orcamentos = await _repositorio.ListarAsync(request.ClienteId, request.VendedorId);

        List<OrcamentoResumoResponse> resposta = new List<OrcamentoResumoResponse>();

        foreach (Orcamento orcamento in orcamentos)
        {
            Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.ClienteId);
            Vendedor? vendedor = await _vendedores.ObterPorIdAsync(orcamento.VendedorId);

            resposta.Add(OrcamentoMapeamento.ParaResumo(orcamento, cliente, vendedor));
        }

        return resposta;
    }

    private async Task<Orcamento> ObterOrcamentoExistenteAsync(int id)
    {
        Orcamento? orcamento = await _repositorio.ObterPorIdAsync(id);

        if (orcamento == null)
            throw new ErroDeNegocio("Orçamento não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return orcamento;
    }

    private async Task<Orcamento> ObterOrcamentoAbertoAsync(int id)
    {
        Orcamento orcamento = await ObterOrcamentoExistenteAsync(id);

        if (orcamento.Cancelado)
            throw new ErroDeNegocio("Orçamento cancelado não pode ser alterado.", TipoErroDeNegocio.Validacao);

        return orcamento;
    }

    private async Task<OrcamentoItem> ObterItemExistenteAsync(int orcamentoId, string numeroItem)
    {
        OrcamentoItem? item = await _repositorio.ObterItemAsync(orcamentoId, numeroItem);

        if (item == null)
            throw new ErroDeNegocio("Item não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        return item;
    }

    private async Task<Cliente> ValidarClienteAsync(int id)
    {
        Cliente? cliente = await _clientes.ObterPorIdAsync(id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(cliente.Status))
            throw new ErroDeNegocio("Cliente inativo não pode ser usado.", TipoErroDeNegocio.Validacao);

        return cliente;
    }

    private async Task<Vendedor> ValidarVendedorAsync(int id)
    {
        Vendedor? vendedor = await _vendedores.ObterPorIdAsync(id);

        if (vendedor == null)
            throw new ErroDeNegocio("Vendedor não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(vendedor.Status))
            throw new ErroDeNegocio("Vendedor inativo não pode ser usado.", TipoErroDeNegocio.Validacao);

        return vendedor;
    }

    private async Task<Produto> ValidarProdutoAsync(string codigo, string mensagemInativo)
    {
        Produto? produto = await _produtos.ObterPorCodigoAsync(codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(produto.Status))
            throw new ErroDeNegocio(mensagemInativo, TipoErroDeNegocio.Validacao);

        return produto;
    }

    private async Task GravarItemAsync(int orcamentoId, Produto produto, decimal quantidade)
    {
        OrcamentoItem item = new OrcamentoItem
        {
            OrcamentoId = orcamentoId,
            CodigoProduto = produto.Codigo,
            Quantidade = quantidade,
            Preco = produto.PrecoVenda,
            Total = OrcamentoValidacao.Arredondar(quantidade * produto.PrecoVenda)
        };

        try
        {
            await _repositorio.InserirItemAsync(item);
        }
        catch (Exception erro)
        {
            _log.LogError(erro, "Falha ao gravar item do orçamento {OrcamentoId} (produto {CodigoProduto})", orcamentoId, produto.Codigo);
            throw;
        }
    }

    private async Task<OrcamentoResponse> MontarRespostaAsync(Orcamento orcamento)
    {
        Cliente? cliente = await _clientes.ObterPorIdAsync(orcamento.ClienteId);
        Vendedor? vendedor = await _vendedores.ObterPorIdAsync(orcamento.VendedorId);
        List<OrcamentoItem> itens = await _repositorio.ListarItensAsync(orcamento.Id);

        return OrcamentoMapeamento.ParaResposta(orcamento, cliente, vendedor, itens);
    }
}
