namespace Erp.Api.Common;

public class HttpPutAttribute : RotaHttpAttribute
{
    public HttpPutAttribute(string rota) : base(HttpMetodo.Put, rota)
    {
    }
}
