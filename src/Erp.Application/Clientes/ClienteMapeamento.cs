using Erp.Application.Abstractions.Clientes;
using Erp.Core.Entities;

namespace Erp.Application.Clientes;

public static class ClienteMapeamento
{
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
