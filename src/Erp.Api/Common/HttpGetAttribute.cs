namespace Erp.Api.Common;

public class HttpGetAttribute : RotaHttpAttribute
{
    public HttpGetAttribute(string rota) : base(HttpMetodo.Get, rota)
    {
    }
}
