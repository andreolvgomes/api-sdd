using FastEndpoints;

namespace Erp.Api.Common;

public abstract class EndpointBase<TRequest> : Endpoint<TRequest, object?>
    where TRequest : notnull
{
    public async Task<object?> Ok(object data, CancellationToken ct = default)
        => await this.OkAsync(data, ct);

    public async Task<object?> Ok(CancellationToken ct = default)
        => await this.OkAsync(ct);

    public async Task<object?> Created(object data, CancellationToken ct = default)
        => await this.CreatedAsync(data, ct);

    public async Task<object?> BadRequest(string message, CancellationToken ct = default)
        => await this.BadRequestAsync(message, ct);

    public async Task<object?> NotFound(string message = "", CancellationToken ct = default)
        => await this.NotFoundAsync(message, ct);

    public async Task<object?> NoContent(string message = "", CancellationToken ct = default)
        => await this.NoContentAsync(message, ct);

    public async Task<object?> Forbidden(string message = "", CancellationToken ct = default)
        => await this.ForbiddenAsync(message, ct);

    public async Task<object?> Unauthorized(string message = "", CancellationToken ct = default)
        => await this.UnauthorizedAsync(message, ct);
}
