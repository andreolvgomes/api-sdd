//using Erp.Application.Abstractions.Common;
//using Erp.Application.Abstractions.Common.Resultados;
//using System.Diagnostics.CodeAnalysis;

//namespace Erp.Api.Common;

//public class teste2 { }
//public class Teste : EndpointBaseResult<teste2>
//{
//    protected async override Task<AppResult<object?>> Executar(teste2 req, CancellationToken ct)
//    {
//        return new NaoEncontrado("");
//    }
//}

//public abstract class EndpointBaseResult<TRequest> : EndpointBaseResult<TRequest, object?>
//    where TRequest : notnull
//{
//}

//public abstract class EndpointBaseResult<TRequest, TResponse> : EndpointBase<TRequest>
//    where TRequest : notnull
//{
//    public override async Task<object?> ExecuteAsync(TRequest req, CancellationToken ct)
//    {
//        return await Response(await Executar(req, ct), ct);
//    }

//    protected abstract Task<AppResult<TResponse>> Executar(TRequest req, CancellationToken ct);

//    protected virtual Task<object?> RespostaDeSucesso(TResponse valor, CancellationToken ct)
//        => Ok(valor!, ct);

//    private async Task<object?> Response(AppResult<TResponse> resultado, CancellationToken ct)
//    {
//        if (resultado.Sucess)
//            return await RespostaDeSucesso(resultado.Data!, ct);

//        if (resultado.Error is NaoEncontrado)
//            return await NotFound(resultado.Error.Mensagem, ct);

//        if (resultado.Error is Conflito)
//            return await Conflict(resultado.Error.Mensagem, ct);

//        return await BadRequest(resultado.Error!.Mensagem, ct);
//    }
//}
