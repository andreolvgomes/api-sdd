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

    public ClientesService(IClienteRepository repositorio)
    {
        _repositorio = repositorio;
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
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        if (cliente.Status == "inativo")
            throw new ErroDeNegocio("Cliente já está inativo", TipoErroDeNegocio.Validacao);

        cliente.Status = "inativo";

        await _repositorio.AtualizarAsync(cliente);
    }

    public async Task<Cliente> ObterAsync(ObterClienteRequest request)
    {
        Cliente? cliente = await _repositorio.ObterPorIdAsync(request.Id);

        if (cliente == null)
            throw new ErroDeNegocio("Cliente não encontrado", TipoErroDeNegocio.NaoEncontrado);

        return cliente;
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
