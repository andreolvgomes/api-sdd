using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Application.Abstractions.Produtos.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace Erp.Api.Features.Produtos;

[HttpPut("api/produtos/{Prod_codigo}")]
[AllowAnonymous()]
[Tags("Produtos")]
public class AlterarProdutoEndpoint : EndpointBase<AlterarProdutoRequest>
{
    private readonly IProdutosService _produtosService;

    public AlterarProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task<object?> ExecuteAsync(AlterarProdutoRequest req, CancellationToken ct)
    {
        return await Response(await _produtosService.AlterarAsync(req), ct);
    }
}
