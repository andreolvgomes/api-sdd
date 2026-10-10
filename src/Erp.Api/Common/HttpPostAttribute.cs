namespace Erp.Api.Common;

public class HttpPostAttribute : RotaHttpAttribute
{
    public HttpPostAttribute(string rota) : base(HttpMetodo.Post, rota)
    {
    }
}
