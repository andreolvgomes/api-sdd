using Erp.Application.Abstractions.Clientes;
using Erp.Application.Abstractions.Clientes.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Clientes;

public static class EnderecoAdicionalMapeamento
{
    public static EnderecoAdicionalResponse ParaResposta(EnderecoAdicional endereco)
    {
        return new EnderecoAdicionalResponse
        {
            Id = endereco.Id,
            Rotulo = endereco.Rotulo,
            Endereco = endereco.Endereco
        };
    }
}
