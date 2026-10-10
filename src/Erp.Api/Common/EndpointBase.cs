using System.Reflection;
using Erp.Application.Abstractions.Common;
using FastEndpoints;

namespace Erp.Api.Common;

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

    protected T ExecutarCasoDeUso<T>(Func<T> casoDeUso)
    {
        try
        {
            return casoDeUso();
        }
        catch (ErroDeNegocio erro)
        {
            ThrowError(erro.Message, StatusHttp(erro.Tipo));
            return default!;
        }
    }

    protected void ExecutarCasoDeUsoSemRetorno(Action casoDeUso)
    {
        try
        {
            casoDeUso();
        }
        catch (ErroDeNegocio erro)
        {
            ThrowError(erro.Message, StatusHttp(erro.Tipo));
        }
    }

    private static int StatusHttp(TipoErroDeNegocio tipo)
    {
        if (tipo == TipoErroDeNegocio.NaoEncontrado)
            return 404;

        if (tipo == TipoErroDeNegocio.Conflito)
            return 409;

        return 400;
    }
}
