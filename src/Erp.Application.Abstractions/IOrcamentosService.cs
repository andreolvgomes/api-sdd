using Erp.Application.Abstractions.Orcamentos.Dtos;

namespace Erp.Application.Abstractions.Orcamentos;

public interface IOrcamentosService
{
    Task<OrcamentoResponse> IncluirAsync(IncluirOrcamentoRequest request);

    Task<OrcamentoResponse> IncluirItemAsync(IncluirItemOrcamentoRequest request);

    Task<OrcamentoResponse> AlterarAsync(AlterarOrcamentoRequest request);

    Task<OrcamentoResponse> AlterarItemAsync(AlterarItemOrcamentoRequest request);

    Task<OrcamentoResponse> CancelarItemAsync(CancelarItemOrcamentoRequest request);

    Task<OrcamentoResponse> ReativarItemAsync(ReativarItemOrcamentoRequest request);

    Task<OrcamentoResponse> CancelarAsync(CancelarOrcamentoRequest request);

    Task<OrcamentoResponse> FecharAsync(FecharOrcamentoRequest request);

    Task<OrcamentoResponse> LancarPagamentoAsync(LancarPagamentoRequest request);

    Task<OrcamentoResponse> CancelarPagamentoAsync(CancelarPagamentoRequest request);

    Task<OrcamentoResponse> ObterAsync(ObterOrcamentoRequest request);

    Task<List<OrcamentoResumoResponse>> ListarAsync(ListarOrcamentosRequest request);
}
