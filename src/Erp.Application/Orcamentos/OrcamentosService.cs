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
        Produto produto = await ValidarProdutoAsync(request.CodigoProduto!);

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
        Orcamento? orcamento = await _repositorio.ObterPorIdAsync(request.Id);

        if (orcamento == null)
            throw new ErroDeNegocio("Orçamento não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        string? erro = OrcamentoValidacao.ValidarItem(request.CodigoProduto, request.Quantidade);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Produto produto = await ValidarProdutoAsync(request.CodigoProduto!);

        await GravarItemAsync(orcamento.Id, produto, request.Quantidade!.Value);

        return await MontarRespostaAsync(orcamento);
    }

    public async Task<OrcamentoResponse> ObterAsync(ObterOrcamentoRequest request)
    {
        Orcamento? orcamento = await _repositorio.ObterPorIdAsync(request.Id);

        if (orcamento == null)
            throw new ErroDeNegocio("Orçamento não encontrado.", TipoErroDeNegocio.NaoEncontrado);

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

    private async Task<Produto> ValidarProdutoAsync(string codigo)
    {
        Produto? produto = await _produtos.ObterPorCodigoAsync(codigo);

        if (produto == null)
            throw new ErroDeNegocio("Produto não encontrado.", TipoErroDeNegocio.NaoEncontrado);

        if (!OrcamentoValidacao.EhAtivo(produto.Status))
            throw new ErroDeNegocio("Produto inativo não pode ser incluído.", TipoErroDeNegocio.Validacao);

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
