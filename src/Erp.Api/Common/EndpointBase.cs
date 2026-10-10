using System.Reflection;
using FastEndpoints;

namespace Erp.Api.Common;

public abstract class EndpointBase : EndpointWithoutRequest
{
    public override void Configure()
    {
        RotaHttpAttribute rota = GetType().GetCustomAttribute<RotaHttpAttribute>()
            ?? throw new InvalidOperationException($"{GetType().Name} precisa de um atributo de rota HTTP");

        if (rota.Metodo == HttpMetodo.Post)
            Post(rota.Rota);

        if (rota.Metodo == HttpMetodo.Get)
            Get(rota.Rota);

        if (rota.Metodo == HttpMetodo.Put)
            Put(rota.Rota);

        if (rota.Metodo == HttpMetodo.Delete)
            Delete(rota.Rota);

        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await this.ExecutarCasoDeUsoAsync(() => ExecutarAsync(ct), ct);
    }

    public abstract Task<object?> ExecutarAsync(CancellationToken ct);

    protected Task<object?> Ok(object dados, CancellationToken ct = default) => this.OkAsync(dados, ct);

    protected Task<object?> Created(object dados, CancellationToken ct = default) => this.CreatedAsync(dados, ct);

    protected Task<object?> NoContent(CancellationToken ct = default) => this.NoContentAsync(ct);

    protected Task<object?> BadRequest(string mensagem, CancellationToken ct = default) => this.BadRequestAsync(mensagem, ct);

    protected Task<object?> NotFound(string mensagem, CancellationToken ct = default) => this.NotFoundAsync(mensagem, ct);

    protected Task<object?> Conflict(string mensagem, CancellationToken ct = default) => this.ConflictAsync(mensagem, ct);

    protected Task<object?> Unauthorized(string mensagem, CancellationToken ct = default) => this.UnauthorizedAsync(mensagem, ct);

    protected Task<object?> Forbidden(string mensagem, CancellationToken ct = default) => this.ForbiddenAsync(mensagem, ct);
}

public abstract class EndpointBase<TReq> : Endpoint<TReq>
    where TReq : notnull
{
    public override void Configure()
    {
        RotaHttpAttribute rota = GetType().GetCustomAttribute<RotaHttpAttribute>()
            ?? throw new InvalidOperationException($"{GetType().Name} precisa de um atributo de rota HTTP");

        if (rota.Metodo == HttpMetodo.Post)
            Post(rota.Rota);

        if (rota.Metodo == HttpMetodo.Get)
            Get(rota.Rota);

        if (rota.Metodo == HttpMetodo.Put)
            Put(rota.Rota);

        if (rota.Metodo == HttpMetodo.Delete)
            Delete(rota.Rota);

        AllowAnonymous();
    }

    public override async Task HandleAsync(TReq req, CancellationToken ct)
    {
        await this.ExecutarCasoDeUsoAsync(() => ExecutarAsync(req, ct), ct);
    }

    public abstract Task<object?> ExecutarAsync(TReq req, CancellationToken ct);

    protected Task<object?> Ok(object dados, CancellationToken ct = default) => this.OkAsync(dados, ct);

    protected Task<object?> Created(object dados, CancellationToken ct = default) => this.CreatedAsync(dados, ct);

    protected Task<object?> NoContent(CancellationToken ct = default) => this.NoContentAsync(ct);

    protected Task<object?> BadRequest(string mensagem, CancellationToken ct = default) => this.BadRequestAsync(mensagem, ct);

    protected Task<object?> NotFound(string mensagem, CancellationToken ct = default) => this.NotFoundAsync(mensagem, ct);

    protected Task<object?> Conflict(string mensagem, CancellationToken ct = default) => this.ConflictAsync(mensagem, ct);

    protected Task<object?> Unauthorized(string mensagem, CancellationToken ct = default) => this.UnauthorizedAsync(mensagem, ct);

    protected Task<object?> Forbidden(string mensagem, CancellationToken ct = default) => this.ForbiddenAsync(mensagem, ct);
}

public abstract class EndpointBase<TReq, TRes> : Endpoint<TReq, TRes>
    where TReq : notnull
{
    public override void Configure()
    {
        RotaHttpAttribute rota = GetType().GetCustomAttribute<RotaHttpAttribute>()
            ?? throw new InvalidOperationException($"{GetType().Name} precisa de um atributo de rota HTTP");

        if (rota.Metodo == HttpMetodo.Post)
            Post(rota.Rota);

        if (rota.Metodo == HttpMetodo.Get)
            Get(rota.Rota);

        if (rota.Metodo == HttpMetodo.Put)
            Put(rota.Rota);

        if (rota.Metodo == HttpMetodo.Delete)
            Delete(rota.Rota);

        AllowAnonymous();
    }

    public override async Task HandleAsync(TReq req, CancellationToken ct)
    {
        await this.ExecutarCasoDeUsoAsync(() => ExecutarAsync(req, ct), ct);
    }

    public abstract Task<object?> ExecutarAsync(TReq req, CancellationToken ct);

    protected Task<object?> Ok(object dados, CancellationToken ct = default) => this.OkAsync(dados, ct);

    protected Task<object?> Created(object dados, CancellationToken ct = default) => this.CreatedAsync(dados, ct);

    protected Task<object?> NoContent(CancellationToken ct = default) => this.NoContentAsync(ct);

    protected Task<object?> BadRequest(string mensagem, CancellationToken ct = default) => this.BadRequestAsync(mensagem, ct);

    protected Task<object?> NotFound(string mensagem, CancellationToken ct = default) => this.NotFoundAsync(mensagem, ct);

    protected Task<object?> Conflict(string mensagem, CancellationToken ct = default) => this.ConflictAsync(mensagem, ct);

    protected Task<object?> Unauthorized(string mensagem, CancellationToken ct = default) => this.UnauthorizedAsync(mensagem, ct);

    protected Task<object?> Forbidden(string mensagem, CancellationToken ct = default) => this.ForbiddenAsync(mensagem, ct);
}
