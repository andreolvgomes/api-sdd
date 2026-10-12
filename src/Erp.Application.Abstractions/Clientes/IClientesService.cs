using Erp.Core.Entities;
using Erp.Application.Abstractions.Clientes.Dtos;

namespace Erp.Application.Abstractions.Clientes;

public interface IClientesService
{
    Task<ClienteResponse> IncluirAsync(IncluirClienteRequest request);

    Task<ClienteResponse> AlterarAsync(AlterarClienteRequest request);

    Task ExcluirAsync(ExcluirClienteRequest request);

    Task DesativarAsync(DesativarClienteRequest request);

    Task AtivarAsync(AtivarClienteRequest request);

    Task<ClienteDetalheResponse> ObterAsync(ObterClienteRequest request);

    Task<List<Cliente>> ListarAsync(ListarClientesRequest request);

    Task<EnderecoAdicionalResponse> IncluirEnderecoAdicionalAsync(IncluirEnderecoAdicionalRequest request);

    Task<EnderecoAdicionalResponse> AlterarEnderecoAdicionalAsync(AlterarEnderecoAdicionalRequest request);

    Task ExcluirEnderecoAdicionalAsync(ExcluirEnderecoAdicionalRequest request);
}
