using Erp.Application.Abstractions.CaixasBancos.Dtos;
using Erp.Core.Entities;

namespace Erp.Application.Abstractions.CaixasBancos;

public interface ICaixasBancosService
{
    Task<CaixaBancoResponse> IncluirAsync(IncluirCaixaBancoRequest request);

    Task<CaixaBancoResponse> AlterarAsync(AlterarCaixaBancoRequest request);

    Task<CaixaBanco> ObterAsync(ObterCaixaBancoRequest request);

    Task<List<CaixaBanco>> ListarAsync(ListarCaixasBancosRequest request);

    Task ExcluirAsync(ExcluirCaixaBancoRequest request);
}
