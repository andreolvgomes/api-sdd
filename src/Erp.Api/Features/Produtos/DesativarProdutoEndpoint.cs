using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpPost("api/produtos/{Codigo}/desativar")]
[AllowAnonymous()]
[Tags("Produtos")]
public class DesativarProdutoEndpoint : EndpointBase<DesativarProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public DesativarProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(DesativarProdutoRequest req, CancellationToken ct)
    {
        await _produtosService.DesativarAsync(req);

        return await Ok(ct);
    }
}
