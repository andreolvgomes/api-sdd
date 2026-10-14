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
            Ende_id = endereco.Ende_id,
            Ende_rotulo = endereco.Ende_rotulo,
            Ende_endereco = endereco.Ende_endereco
        };
    }
}
