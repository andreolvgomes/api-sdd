using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Application.Abstractions.Common;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.CaixasBancos;

public interface ICaixasBancosService
{
    Task<AppResult<CaixaBancoResponse>> IncluirAsync(IncluirCaixaBancoRequest request);

    Task<AppResult<CaixaBancoResponse>> AlterarAsync(AlterarCaixaBancoRequest request);

    Task<AppResult<CaixaBanco>> ObterAsync(ObterCaixaBancoRequest request);

    Task<AppResult<List<CaixaBanco>>> ListarAsync(ListarCaixasBancosRequest request);

    Task<AppResult<bool>> ExcluirAsync(ExcluirCaixaBancoRequest request);
}
