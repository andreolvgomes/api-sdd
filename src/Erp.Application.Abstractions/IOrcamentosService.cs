using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Orcamentos.Dtos;

namespace Erp.Application.Abstractions.Orcamentos;

public interface IOrcamentosService
{
    Task<AppResult<OrcamentoResponse>> IncluirAsync(IncluirOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> IncluirItemAsync(IncluirItemOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> AlterarAsync(AlterarOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> AlterarItemAsync(AlterarItemOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> CancelarItemAsync(CancelarItemOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> ReativarItemAsync(ReativarItemOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> CancelarAsync(CancelarOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> FecharAsync(FecharOrcamentoRequest request);

    Task<AppResult<OrcamentoResponse>> LancarPagamentoAsync(LancarPagamentoRequest request);

    Task<AppResult<OrcamentoResponse>> CancelarPagamentoAsync(CancelarPagamentoRequest request);

    Task<AppResult<OrcamentoResponse>> ObterAsync(ObterOrcamentoRequest request);

    Task<AppResult<List<OrcamentoResumoResponse>>> ListarAsync(ListarOrcamentosRequest request);
}
