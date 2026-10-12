using Erp.Application.Abstractions.Orcamentos.Dtos;

namespace Erp.Application.Abstractions.Orcamentos;

public interface IOrcamentosService
{
    Task<OrcamentoResponse> IncluirAsync(IncluirOrcamentoRequest request);

    Task<OrcamentoResponse> IncluirItemAsync(IncluirItemOrcamentoRequest request);

    Task<OrcamentoResponse> ObterAsync(ObterOrcamentoRequest request);

    Task<List<OrcamentoResumoResponse>> ListarAsync(ListarOrcamentosRequest request);
}
