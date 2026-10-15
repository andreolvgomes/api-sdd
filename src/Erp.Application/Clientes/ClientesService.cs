using System.Globalization;
using System.Text;
using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
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

    public async Task<AppResult<EnderecoAdicionalResponse>> IncluirEnderecoAdicionalAsync(IncluirEnderecoAdicionalRequest request)
    {
        if (await _repositorio.ObterPorIdAsync(request.Clie_id) == null)
            return new AppNotFound("Cliente não encontrado");

        string? rotulo = Limpar(request.Ende_rotulo);
        string? endereco = Limpar(request.Ende_endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            return new AppValidation(erro);

        List<EnderecoAdicional> existentes = await _enderecosRepositorio.ListarPorClienteAsync(request.Clie_id);

        if (existentes.Count >= ClienteValidacao.LimiteEnderecosAdicionais)
            return new AppValidation($"Cliente já possui {ClienteValidacao.LimiteEnderecosAdicionais} endereços adicionais");

        EnderecoAdicional enderecoAdicional = new EnderecoAdicional
        {
            Clie_id = request.Clie_id,
            Ende_rotulo = rotulo!,
            Ende_endereco = endereco!
        };

        enderecoAdicional.Ende_id = await _enderecosRepositorio.InserirAsync(enderecoAdicional);

        return EnderecoAdicionalMapeamento.ParaResposta(enderecoAdicional);
    }

    public async Task<AppResult<ClienteResponse>> IncluirAsync(IncluirClienteRequest request)
    {
        string? nome = Limpar(request.Clie_nome);
        string? endereco = Limpar(request.Clie_endereco);
        string? telefone = Limpar(request.Clie_telefone);

        string? status = Limpar(request.Clie_status) ?? "ativo";

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            return new AppValidation(erro);

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

    public async Task<AppResult<ClienteResponse>> AlterarAsync(AlterarClienteRequest request)
    {
        Cliente? existente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (existente == null)
            return new AppNotFound("Cliente não encontrado");

        string? nome = Limpar(request.Clie_nome);
        string? endereco = Limpar(request.Clie_endereco);
        string? telefone = Limpar(request.Clie_telefone);
        string? status = Limpar(request.Clie_status) ?? existente.Clie_status;

        string? erro = ClienteValidacao.Validar(nome, endereco, telefone);

        if (erro == null)
            erro = ClienteValidacao.ValidarStatus(status);

        if (erro != null)
            return new AppValidation(erro);

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

    public async Task<AppResult<bool>> ExcluirAsync(ExcluirClienteRequest request)
    {
        if (!await _repositorio.ExcluirAsync(request.Clie_id))
            return new AppNotFound("Cliente não encontrado");

        return true;
    }

    public async Task<AppResult<bool>> DesativarAsync(DesativarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            return new AppNotFound("Cliente não encontrado");

        if (cliente.Clie_status == "inativo")
            return new AppValidation("Cliente já está inativo");

        cliente.Clie_status = "inativo";

        await _repositorio.AtualizarAsync(cliente);

        return true;
    }

    public async Task<AppResult<bool>> AtivarAsync(AtivarClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            return new AppNotFound("Cliente não encontrado");

        if (cliente.Clie_status == "ativo")
            return new AppValidation("Cliente já está ativo");

        cliente.Clie_status = "ativo";

        await _repositorio.AtualizarAsync(cliente);

        return true;
    }

    public async Task<AppResult<ClienteResponse>> AlterarBloqueioAsync(AlterarBloqueioClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            return new AppNotFound("Cliente não encontrado");

        string? erro = ClienteValidacao.ValidarBloqueio(request.Clie_bloqueio);

        if (erro != null)
            return new AppValidation(erro);

        cliente.Clie_bloqueio = request.Clie_bloqueio;

        await _repositorio.AtualizarAsync(cliente);

        return ClienteMapeamento.ParaResposta(cliente);
    }

    public async Task<AppResult<ClienteDetalheResponse>> ObterAsync(ObterClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Clie_id);

        if (cliente == null)
            return new AppNotFound("Cliente não encontrado");

        List<EnderecoAdicional> enderecos = await _enderecosRepositorio.ListarPorClienteAsync(request.Clie_id);

        return ClienteMapeamento.ParaDetalhe(cliente, enderecos);
    }

    public async Task<AppResult<List<Cliente>>> ListarAsync(ListarClientesRequest request)
    {
        string status = Limpar(request.Clie_status) ?? "ativo";

        string? erro = ClienteValidacao.ValidarFiltroStatus(status);

        if (erro != null)
            return new AppValidation(erro);

        List<Cliente> clientes = await _repositorio.ListarAsync();

        if (status != "todos")
            clientes = clientes.Where(cliente => cliente.Clie_status == status).ToList();

        return clientes.OrderBy(cliente => ChaveDeOrdenacao(cliente.Clie_nome)).ToList();
    }

    public async Task<AppResult<EnderecoAdicionalResponse>> AlterarEnderecoAdicionalAsync(AlterarEnderecoAdicionalRequest request)
    {
        if (await _repositorio.ObterPorIdAsync(request.Clie_id) == null)
            return new AppNotFound("Cliente não encontrado");

        EnderecoAdicional? existente = await _enderecosRepositorio.ObterAsync(request.Clie_id, request.Ende_id);

        if (existente == null)
            return new AppNotFound("Endereço não encontrado");

        string? rotulo = Limpar(request.Ende_rotulo);
        string? endereco = Limpar(request.Ende_endereco);

        string? erro = ClienteValidacao.ValidarRotulo(rotulo);

        if (erro == null)
            erro = ClienteValidacao.ValidarEndereco(endereco);

        if (erro != null)
            return new AppValidation(erro);

        existente.Ende_rotulo = rotulo!;
        existente.Ende_endereco = endereco!;

        await _enderecosRepositorio.AtualizarAsync(existente);

        return EnderecoAdicionalMapeamento.ParaResposta(existente);
    }

    public async Task<AppResult<bool>> ExcluirEnderecoAdicionalAsync(ExcluirEnderecoAdicionalRequest request)
    {
        if (await _repositorio.ObterPorIdAsync(request.Clie_id) == null)
            return new AppNotFound("Cliente não encontrado");

        if (!await _enderecosRepositorio.ExcluirAsync(request.Clie_id, request.Ende_id))
            return new AppNotFound("Endereço não encontrado");

        return true;
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
