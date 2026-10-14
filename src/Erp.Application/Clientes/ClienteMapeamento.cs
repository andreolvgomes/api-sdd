using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Clientes;

public static class ClienteMapeamento
{
    public static ClienteDetalheResponse ParaDetalhe(Cliente cliente, List<EnderecoAdicional> enderecos)
    {
        ClienteDetalheResponse resposta = new ClienteDetalheResponse
        {
            Clie_id = cliente.Clie_id,
            Clie_nome = cliente.Clie_nome,
            Clie_endereco = cliente.Clie_endereco,
            Clie_telefone = cliente.Clie_telefone,
            Clie_status = cliente.Clie_status
        };

        foreach (EnderecoAdicional endereco in enderecos)
            resposta.EnderecosAdicionais.Add(EnderecoAdicionalMapeamento.ParaResposta(endereco));

        return resposta;
    }

    public static ClienteResponse ParaResposta(Cliente cliente)
    {
        return new ClienteResponse
        {
            Clie_id = cliente.Clie_id,
            Clie_nome = cliente.Clie_nome,
            Clie_endereco = cliente.Clie_endereco,
            Clie_telefone = cliente.Clie_telefone,
            Clie_status = cliente.Clie_status
        };
    }
}
