using Erp.Application.Abstractions.Common;

namespace Erp.Api.Common;

public static class StatusHttp
{
    public static int Obter(TipoErroDeNegocio tipo)
    {
        if (tipo == TipoErroDeNegocio.NaoEncontrado)
            return 404;

        if (tipo == TipoErroDeNegocio.Conflito)
            return 409;

        return 400;
    }
}
