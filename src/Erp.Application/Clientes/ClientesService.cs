using System.Globalization;
using System.Text;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
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
        await GarantirClienteExisteAsync(request.Clie_id);

        string? rotulo = Limpar(request.Ende_rotulo);
        string? endereco = Limpar(request.Ende_endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        List<EnderecoAdicional> existentes = await _enderecosRepositorio.ListarPorClienteAsync(request.Clie_id);

        if (existentes.Count >= ClienteValidacao.LimiteEnderecosAdicionais)
            throw new ErroDeNegocio($"Cliente já possui {ClienteValidacao.LimiteEnderecosAdicionais} endereços adicionais", TipoErroDeNegocio.Validacao);

        EnderecoAdicional enderecoAdicional = new EnderecoAdicional
        {
            Clie_id = request.Clie_id,
            Ende_rotulo = rotulo!,
            Ende_endereco = endereco!
        };

        enderecoAdicional.Ende_id = await _enderecosRepositorio.InserirAsync(enderecoAdicional);

        return EnderecoAdicionalMapeamento.ParaResposta(enderecoAdicional);
    }

    public async Task<ClienteResponse> IncluirAsync(IncluirClienteRequest request)
    {
        string? nome = Limpar(request.Clie_nome);
        string? endereco = Limpar(request.Clie_endereco);
        string? telefone = Limpar(request.Clie_telefone);

        string? status = Limpar(request.Clie_status) ?? "ativo";

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = new Cliente
        {
            Clie_nome = nome!,
            Clie_endereco = endereco!,
            Clie_telefone = telefone!,
            Clie_status = status,
            Clie_bloqueio = "liberado"
        };

        cliente.Clie_id = await _repositorio.InserirAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task<ClienteResponse> AlterarAsync(AlterarClienteRequest request)
    {
        Cliente? existente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (existente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? nome = Limpar(request.Clie_nome);
        string? endereco = Limpar(request.Clie_endereco);
        string? telefone = Limpar(request.Clie_telefone);
        string? status = Limpar(request.Clie_status) ?? existente.Clie_status;

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        Cliente cliente = new Cliente
        {
            Clie_id = request.Clie_id,
            Clie_nome = nome!,
            Clie_endereco = endereco!,
            Clie_telefone = telefone!,
            Clie_status = status,
            Clie_bloqueio = existente.Clie_bloqueio
        };

        await _repositorio.AtualizarAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task ExcluirAsync(ExcluirClienteRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Clie_id))
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);
    }

    public async Task DesativarAsync(DesativarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (cliente.Clie_status == "inativo")
            throw new ErroDeNegocio("Cliente já está inativo", TipoErroDeNegocio.Validacao);

        cliente.Clie_status = "inativo";

        await _repositorio.AtualizarAsync(cliente);
    }

    public async Task AtivarAsync(AtivarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (cliente.Clie_status == "ativo")
            throw new ErroDeNegocio("Cliente já está ativo", TipoErroDeNegocio.Validacao);

        cliente.Clie_status = "ativo";

        await _repositorio.AtualizarAsync(cliente);
    }

    public async Task<ClienteResponse> AlterarBloqueioAsync(AlterarBloqueioClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? erro = ClienteValidacao.ValidarBloqueio(request.Clie_bloqueio);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        cliente.Clie_bloqueio = request.Clie_bloqueio;

        await _repositorio.AtualizarAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task<ClienteDetalheResponse> ObterAsync(ObterClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        List<EnderecoAdicional> enderecos = await _enderecosRepositorio.ListarPorClienteAsync(request.Clie_id);

        return ClienteMapeamento.ParaDetalhe(cliente, enderecos);
    }

    public async Task<List<Cliente>> ListarAsync(ListarClientesRequest request)
    {
        string status = Limpar(request.Clie_status) ?? "ativo";

        string? erro = ClienteValidacao.ValidarFiltroStatus(status);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        List<Cliente> clientes = await _repositorio.ListarAsync();

        if (status != "todos")
            clientes = clientes.Where(cliente => cliente.Clie_status == status).ToList();

        return clientes.OrderBy(cliente => ChaveDeOrdenacao(cliente.Clie_nome)).ToList();
    }

    public async Task<EnderecoAdicionalResponse> AlterarEnderecoAdicionalAsync(AlterarEnderecoAdicionalRequest request)
    {
        await GarantirClienteExisteAsync(request.Clie_id);

        EnderecoAdicional? existente = await _enderecosRepositorio.ObterAsync(request.Clie_id, request.Ende_id);

        if (existente == null)
            throw new ErroDeNegocio("Endereço não encontrado", TipoErroDeNegocio.NaoEncontrado);

        string? rotulo = Limpar(request.Ende_rotulo);
        string? endereco = Limpar(request.Ende_endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            throw new ErroDeNegocio(erro, TipoErroDeNegocio.Validacao);

        existente.Ende_rotulo = rotulo!;
        existente.Ende_endereco = endereco!;

        await _enderecosRepositorio.AtualizarAsync(existente);

        return EnderecoAdicionalMapeamento.ParaResposta(existente);
    }

    public async Task ExcluirEnderecoAdicionalAsync(ExcluirEnderecoAdicionalRequest request)
    {
        await GarantirClienteExisteAsync(request.Clie_id);

        if (!await _enderecosRepositorio.ExcluirAsync(request.Clie_id, request.Ende_id))
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
