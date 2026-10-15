using Erp.Application.Abstractions.Common.AppResults;

namespace Erp.Application.Abstractions.Common;

public class AppResult<T>
{
    public bool Sucess { get; private set; }
    public T? Data { get; private set; }
    public IApplicationError? Error { get; private set; }

    private AppResult() { }

    public static implicit operator AppResult<T>(T valor)
    {
        if (valor == null)
            return new AppResult<T> { Error = new ApplicationNotfound("Registro não encontrado.") };

        return new AppResult<T> { Sucess = true, Data = valor };
    }

    public static implicit operator AppResult<T>(ApplicationNotfound erro)
        => new AppResult<T> { Error = erro };

    public static implicit operator AppResult<T>(AppConflito erro)
        => new AppResult<T> { Error = erro };

    public static implicit operator AppResult<T>(AppValidation erro)
        => new AppResult<T> { Error = erro };

    public AppResult<TOutro> Propagar<TOutro>()
        => new AppResult<TOutro> { Error = Error };
}
