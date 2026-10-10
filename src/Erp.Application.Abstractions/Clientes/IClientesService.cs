using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Clientes;

public interface IClientesService
{
    Task<ClienteResponse> IncluirAsync(IncluirClienteRequest request);

    Task<ClienteResponse> AlterarAsync(AlterarClienteRequest request);

    Task ExcluirAsync(ExcluirClienteRequest request);

    Task<Cliente> ObterAsync(ObterClienteRequest request);

    Task<List<Cliente>> ListarAsync(ListarClientesRequest request);
}
