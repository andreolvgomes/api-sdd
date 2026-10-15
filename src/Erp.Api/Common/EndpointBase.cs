using Erp.Application.Abstractions.Common;
using Erp.Application.Abstractions.Common.AppResults;
using FastEndpoints;

namespace Erp.Api.Common;

public abstract class EndpointBase : EndpointWithoutRequest<object?>
{
    public async Task<object?> Ok(object data, CancellationToken ct = default)
        => await this.OkAsync(data, ct);

    public async Task<object?> Ok(CancellationToken ct = default)
        => await this.OkAsync(ct);

    public async Task<object?> Created(object data, CancellationToken ct = default)
        => await this.CreatedAsync(data, ct);

    public async Task<object?> BadRequest(string message, CancellationToken ct = default)
        => await this.BadRequestAsync(message, ct);

    public async Task<object?> Conflict(string message = "", CancellationToken ct = default)
        => await this.ConflictAsync(message, ct);

    public async Task<object?> NotFound(string message = "", CancellationToken ct = default)
        => await this.NotFoundAsync(message, ct);

    public async Task<object?> NoContent(string message = "", CancellationToken ct = default)
        => await this.NoContentAsync(message, ct);

    public async Task<object?> Forbidden(string message = "", CancellationToken ct = default)
        => await this.ForbiddenAsync(message, ct);

    public async Task<object?> Unauthorized(string message = "", CancellationToken ct = default)
        => await this.UnauthorizedAsync(message, ct);

    protected async Task<object?> Response<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (result.Sucess)
            return await Ok(result.Data!, ct);

        if (result.Error is ApplicationNotfound)
            return await NotFound(result.Error.Mensagem, ct);

        if (result.Error is AppConflito)
            return await Conflict(result.Error.Mensagem, ct);

        return await BadRequest(result.Error!.Mensagem, ct);
    }

    protected async Task<object?> NoContent<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (!result.Sucess)
            return await Response(result, ct);
        return await NoContent(ct: ct);
    }

    protected async Task<object?> Created<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (!result.Sucess)
            return await Response(result, ct);
        return await Created(result.Data!, ct);
    }
}

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

    public async Task<object?> Conflict(string message = "", CancellationToken ct = default)
        => await this.ConflictAsync(message, ct);

    public async Task<object?> NoContent(string message = "", CancellationToken ct = default)
        => await this.NoContentAsync(message, ct);

    public async Task<object?> Forbidden(string message = "", CancellationToken ct = default)
        => await this.ForbiddenAsync(message, ct);

    public async Task<object?> Unauthorized(string message = "", CancellationToken ct = default)
        => await this.UnauthorizedAsync(message, ct);

    protected async Task<object?> Response<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (result.Sucess)
            return await Ok(result.Data!, ct);

        if (result.Error is ApplicationNotfound)
            return await NotFound(result.Error.Mensagem, ct);

        if (result.Error is AppConflito)
            return await Conflict(result.Error.Mensagem, ct);

        return await BadRequest(result.Error!.Mensagem, ct);
    }

    protected async Task<object?> NoContent<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (!result.Sucess)
            return await Response(result, ct);
        return await NoContent(ct: ct);
    }

    protected async Task<object?> Created<T>(AppResult<T> result, CancellationToken ct = default)
    {
        if (!result.Sucess)
            return await Response(result, ct);
        return await Created(result.Data!, ct);
    }
}
