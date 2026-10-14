using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Abstractions.Produtos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpPost("api/produtos/{Prod_codigo}/ativar")]
[AllowAnonymous()]
[Tags("Produtos")]
public class AtivarProdutoEndpoint : EndpointBase<AtivarProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public AtivarProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(AtivarProdutoRequest req, CancellationToken ct)
    {
        await _produtosService.AtivarAsync(req);

        return await Ok(ct);
    }
}
