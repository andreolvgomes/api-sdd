using Erp.Application.Abstractions.Common.AppResults;

namespace Erp.Application.Abstractions.Common;

public class AppResult<T>
{
    public bool Sucess { get; private set; }
    public T? Data { get; private set; }
    public IApplicationError? Error { get; private set; }

    private AppResult() { }

    public static implicit operator AppResult<T>(T valor)
        => new AppResult<T> { Sucess = true, Data = valor };

    public static implicit operator AppResult<T>(ApplicationNotfound erro)
        => new AppResult<T> { Error = erro };

    public static implicit operator AppResult<T>(ApplicationConflito erro)
        => new AppResult<T> { Error = erro };

    public static implicit operator AppResult<T>(ApplicationValidation erro)
        => new AppResult<T> { Error = erro };
}
