using FastEndpoints;
using System.Text.Json;

namespace Erp.Api.Common;

public static class EndpointExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static async Task<object?> OkAsync(this IEndpoint ep, object data, CancellationToken ct = default)
    {
        var jsonString = JsonSerializer.Serialize(data, JsonOptions);
        var jsonObject = JsonDocument.Parse(jsonString);

        ep.HttpContext.Response.ContentType = "application/json";
        return await ep.HttpContext.Response.SendAsync(jsonObject, statusCode: 200, cancellation: ct);
    }

    public static async Task<object?> OkAsync(this IEndpoint ep, CancellationToken ct = default)
    {
        return await ep.HttpContext.Response.SendOkAsync(cancellation: ct);
    }

    public static async Task<object?> CreatedAsync(this IEndpoint ep, object data, CancellationToken ct = default)
    {
        return await ep.HttpContext.Response.SendAsync(data, statusCode: 201, cancellation: ct);
    }

    public static async Task<object?> BadRequestAsync(this IEndpoint ep, string message, CancellationToken ct = default)
    {
        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 400,
            cancellation: ct);
    }

    public static async Task<object?> ConflictAsync(this IEndpoint ep, string message, CancellationToken ct = default)
    {
        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 409,
            cancellation: ct);
    }

    public static async Task<object?> NotFoundAsync(this IEndpoint ep, string message = "", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(message))
            return await ep.HttpContext.Response.SendNotFoundAsync(cancellation: ct);

        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 404,
            cancellation: ct);
    }

    public static async Task<object?> NoContentAsync(this IEndpoint ep, string message = "", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(message))
            return await ep.HttpContext.Response.SendNoContentAsync(cancellation: ct);

        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 204,
            cancellation: ct);
    }

    public static async Task<object?> UnauthorizedAsync(this IEndpoint ep, string message = "", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(message))
            return await ep.HttpContext.Response.SendUnauthorizedAsync(cancellation: ct);

        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 401,
            cancellation: ct);
    }

    public static async Task<object?> ForbiddenAsync(this IEndpoint ep, string message = "", CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(message))
            return await ep.HttpContext.Response.SendAsync((object?)null, statusCode: 403, cancellation: ct);

        return await ep.HttpContext.Response.SendAsync(
            new { messages = new List<string> { message } },
            statusCode: 403,
            cancellation: ct);
    }
}
