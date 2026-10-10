using Erp.Api.Common;
using Erp.Application.Abstractions.Produtos;

namespace Erp.Api.Features.Produtos;

[HttpPost("api/produtos")]
public class IncluirProdutoEndpoint : EndpointBase<IncluirProdutoRequest, ProdutoResponse>
{
    private readonly IProdutosService _produtosService;

    public IncluirProdutoEndpoint(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    public override async Task HandleAsync(IncluirProdutoRequest req, CancellationToken ct)
    {
        ProdutoResponse produto = ExecutarCasoDeUso(() => _produtosService.Incluir(req));

        await Send.ResponseAsync(produto, 201, ct);
    }
}
