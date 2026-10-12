using System.Globalization;
using System.Text;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Common;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Application.Clientes;

public class ClientesService : IClientesService
{
    private readonly IClienteRepository _repositorio;
    private readonly IEnderecoAdicionalRepository _enderecosRepositorio;

    public ClientesService(IClienteRepository repositorio, IEnderecoAdicionalRepository enderecosRepositorio)
    {
        _repositorio = repositorio;
        _enderecosRepositorio = enderecosRepositorio;
    }

    public async Task<EnderecoAdicionalResponse> IncluirEnderecoAdicionalAsync(IncluirEnderecoAdicionalRequest request)
    {
        await GarantirClienteExisteAsync(request.ClienteId);

        string? rotulo = Limpar(request.Rotulo);
        string? endereco = Limpar(request.Endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        List<EnderecoAdicional> existentes = await _enderecosRepositorio.ListarPorClienteAsync(request.ClienteId);

        if (existentes.Count >= ClienteValidacao.LimiteEnderecosAdicionais)
            throw new ErroDeNegocio($"Cliente já possui {ClienteValidacao.LimiteEnderecosAdicionais} endereços adicionais", TipoErroDeNegocio.Validacao);

        EnderecoAdicional enderecoAdicional = new EnderecoAdicional
        {
            ClienteId = request.ClienteId,
            Rotulo = rotulo!,
            Endereco = endereco!
        };

        enderecoAdicional.Id = await _enderecosRepositorio.InserirAsync(enderecoAdicional);

        return EnderecoAdicionalMapeamento.ParaResposta(enderecoAdicional);
    }

    public async Task<ClienteResponse> IncluirAsync(IncluirClienteRequest request)
    {
        string? nome = Limpar(request.Nome);
        string? endereco = Limpar(request.Endereco);
        string? telefone = Limpar(request.Telefone);

        string? status = Limpar(request.Status) ?? "ativo";

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = new Cliente
        {
            Nome = nome!,
            Endereco = endereco!,
            Telefone = telefone!,
            Status = status
        };

        cliente.Id = await _repositorio.InserirAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task<ClienteResponse> AlterarAsync(AlterarClienteRequest request)
    {
        Cliente? existente = await _repositorio.ObterPorIdAsync(request.Id);

        if (existente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? nome = Limpar(request.Nome);
        string? endereco = Limpar(request.Endereco);
        string? telefone = Limpar(request.Telefone);
        string? status = Limpar(request.Status) ?? existente.Status;

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = new Cliente
        {
            Id = request.Id,
            Nome = nome!,
            Endereco = endereco!,
            Telefone = telefone!,
            Status = status
        };

        await _repositorio.AtualizarAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task ExcluirAsync(ExcluirClienteRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Id))
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    public async Task DesativarAsync(DesativarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (cliente.Status == "inativo")
            throw new ErroDeNegocio("Cliente já está inativo", TipoErroDeNegocio.Validacao);

        cliente.Status = "inativo";

        await _repositorio.AtualizarAsync(cliente);
    }

    public async Task AtivarAsync(AtivarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (cliente.Status == "ativo")
            throw new ErroDeNegocio("Cliente já está ativo", TipoErroDeNegocio.Validacao);

        cliente.Status = "ativo";

        await _repositorio.AtualizarAsync(cliente);
    }

    public async Task<ClienteDetalheResponse> ObterAsync(ObterClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        List<EnderecoAdicional> enderecos = await _enderecosRepositorio.ListarPorClienteAsync(request.Id);

        return ClienteMapeamento.ParaDetalhe(cliente, enderecos);
    }

    public async Task<List<Cliente>> ListarAsync(ListarClientesRequest request)
    {
        string status = Limpar(request.Status) ?? "ativo";

        string? erro = ClienteValidacao.ValidarFiltroStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        List<Cliente> clientes = await _repositorio.ListarAsync();

        if (status != "todos")
            clientes = clientes.Where(cliente => cliente.Status == status).ToList();

        return clientes.OrderBy(cliente => ChaveDeOrdenacao(cliente.Nome)).ToList();
    }

    public async Task<EnderecoAdicionalResponse> AlterarEnderecoAdicionalAsync(AlterarEnderecoAdicionalRequest request)
    {
        await GarantirClienteExisteAsync(request.ClienteId);

        EnderecoAdicional? existente = await _enderecosRepositorio.ObterAsync(request.ClienteId, request.Id);

        if (existente == null)
            throw new ErroDeNegocio("Endereço não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? rotulo = Limpar(request.Rotulo);
        string? endereco = Limpar(request.Endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        existente.Rotulo = rotulo!;
        existente.Endereco = endereco!;

        await _enderecosRepositorio.AtualizarAsync(existente);

        return EnderecoAdicionalMapeamento.ParaResposta(existente);
    }

    public async Task ExcluirEnderecoAdicionalAsync(ExcluirEnderecoAdicionalRequest request)
    {
        await GarantirClienteExisteAsync(request.ClienteId);

        if (!await _enderecosRepositorio.ExcluirAsync(request.ClienteId, request.Id))
            throw new ErroDeNegocio("Endereço não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    private async Task GarantirClienteExisteAsync(int clienteId)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(clienteId);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    private static string? Limpar(string? valor)
    {
        return valor?.Trim();
    }

    private static string ChaveDeOrdenacao(string nome)
    {
        string decomposto = nome.Normalize(NormalizationForm.FormD);
        StringBuilder semAcentos = new StringBuilder();

        foreach (char letra in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                semAcentos.Append(letra);

        return semAcentos.ToString().ToLowerInvariant();
    }
}
