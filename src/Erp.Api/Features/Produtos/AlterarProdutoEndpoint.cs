using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpPut("api/produtos/{Codigo}")]
[AllowAnonymous()]
public class AlterarProdutoEndpoint : EndpointBase<AlterarProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public AlterarProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(AlterarProdutoRequest req, CancellationToken ct)
    {
        return await Ok(await _produtosService.AlterarAsync(req), ct);
    }
}
