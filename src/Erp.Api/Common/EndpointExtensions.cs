using Erp.Application.Abstractions.Common;
using FastEndpoints;

namespace Erp.Api.Common;

public static class EndpointExtensions
{
    public static async Task<object?> OkAsync(this IEndpoint ep, object dados, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(dados, 200, cancellation: ct);
        return null;
    }

    public static async Task<object?> CreatedAsync(this IEndpoint ep, object dados, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(dados, 201, cancellation: ct);
        return null;
    }

    public static async Task<object?> NoContentAsync(this IEndpoint ep, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendNoContentAsync(cancellation: ct);
        return null;
    }

    public static async Task<object?> BadRequestAsync(this IEndpoint ep, string mensagem, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(MensagemDeErro(mensagem), 400, cancellation: ct);
        return null;
    }

    public static async Task<object?> NotFoundAsync(this IEndpoint ep, string mensagem, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(MensagemDeErro(mensagem), 404, cancellation: ct);
        return null;
    }

    public static async Task<object?> ConflictAsync(this IEndpoint ep, string mensagem, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(MensagemDeErro(mensagem), 409, cancellation: ct);
        return null;
    }

    public static async Task<object?> UnauthorizedAsync(this IEndpoint ep, string mensagem, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(MensagemDeErro(mensagem), 401, cancellation: ct);
        return null;
    }

    public static async Task<object?> ForbiddenAsync(this IEndpoint ep, string mensagem, CancellationToken ct = default)
    {
        await ep.HttpContext.Response.SendAsync(MensagemDeErro(mensagem), 403, cancellation: ct);
        return null;
    }

    public static async Task<object?> ErroDeNegocioAsync(this IEndpoint ep, ErroDeNegocio erro, CancellationToken ct = default)
    {
        if (erro.Tipo == TipoErroDeNegocio.NaoEncontrado)
            return await ep.NotFoundAsync(erro.Message, ct);

        if (erro.Tipo == TipoErroDeNegocio.Conflito)
            return await ep.ConflictAsync(erro.Message, ct);

        return await ep.BadRequestAsync(erro.Message, ct);
    }

    public static async Task<object?> ExecutarCasoDeUsoAsync(this IEndpoint ep, Func<Task<object?>> casoDeUso, CancellationToken ct = default)
    {
        try
        {
            return await casoDeUso();
        }
        catch (ErroDeNegocio erro)
        {
            return await ep.ErroDeNegocioAsync(erro, ct);
        }
    }

    private static object MensagemDeErro(string mensagem)
    {
        return new { messages = new List<string> { mensagem } };
    }
}
