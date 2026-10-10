using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;
using Erp.Core.Entities;

namespace Erp.Api.Features.Produtos;

[HttpGet("api/produtos")]
public class ListarProdutosEndpoint : EndpointBase<ListarProdutosRequest, List<Produto>>
{
    private readonly IProdutosService _produtosService;

    public ListarProdutosEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task HandleAsync(ListarProdutosRequest req, CancellationToken ct)
    {
        List<Produto> produtos = ExecutarCasoDeUso(() => _produtosService.Listar(req));

        await Send.ResponseAsync(produtos, 200, ct);
    }
}
