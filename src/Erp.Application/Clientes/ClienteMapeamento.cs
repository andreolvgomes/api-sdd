using Erp.Application.Abstractions.Clientes;
using Erp.Core.Entities;

namespace Erp.Application.Clientes;

public static class ClienteMapeamento
{
    public static ClienteDetalheResponse ParaDetalhe(Cliente cliente, List<EnderecoAdicional> enderecos)
    {
        ClienteDetalheResponse resposta = new ClienteDetalheResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Endereco = cliente.Endereco,
            Telefone = cliente.Telefone,
            Status = cliente.Status
        };

        foreach (EnderecoAdicional endereco in enderecos)
            resposta.EnderecosAdicionais.Add(EnderecoAdicionalMapeamento.ParaResposta(endereco));

        return resposta;
    }

    public static ClienteResponse ParaResposta(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Endereco = cliente.Endereco,
            Telefone = cliente.Telefone,
            Status = cliente.Status
        };
    }
}
