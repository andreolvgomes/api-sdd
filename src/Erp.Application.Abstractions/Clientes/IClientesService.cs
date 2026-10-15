using Erp.Application.Abstractions.Clientes.Dtos;
using Erp.Application.Abstractions.Common;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.Clientes;

public interface IClientesService
{
    Task<AppResult<ClienteResponse>> IncluirAsync(IncluirClienteRequest request);

    Task<AppResult<ClienteResponse>> AlterarAsync(AlterarClienteRequest request);

    Task<AppResult<bool>> ExcluirAsync(ExcluirClienteRequest request);

    Task<AppResult<bool>> DesativarAsync(DesativarClienteRequest request);

    Task<AppResult<bool>> AtivarAsync(AtivarClienteRequest request);

    Task<AppResult<ClienteResponse>> AlterarBloqueioAsync(AlterarBloqueioClienteRequest request);

    Task<AppResult<ClienteDetalheResponse>> ObterAsync(ObterClienteRequest request);

    Task<AppResult<List<Cliente>>> ListarAsync(ListarClientesRequest request);

    Task<AppResult<EnderecoAdicionalResponse>> IncluirEnderecoAdicionalAsync(IncluirEnderecoAdicionalRequest request);

    Task<AppResult<EnderecoAdicionalResponse>> AlterarEnderecoAdicionalAsync(AlterarEnderecoAdicionalRequest request);

    Task<AppResult<bool>> ExcluirEnderecoAdicionalAsync(ExcluirEnderecoAdicionalRequest request);
}
