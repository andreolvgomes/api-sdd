namespace Erp.Api.Common;

public class HttpDeleteAttribute : RotaHttpAttribute
{
    public HttpDeleteAttribute(string rota) : base(HttpMetodo.Delete, rota)
    {
    }
}
