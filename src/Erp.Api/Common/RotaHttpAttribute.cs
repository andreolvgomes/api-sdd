namespace Erp.Api.Common;

[AttributeUsage(AttributeTargets.Class)]
public abstract class RotaHttpAttribute : Attribute
{
    protected RotaHttpAttribute(HttpMetodo metodo, string rota)
    {
        Metodo = metodo;
        Rota = rota;
    }

    public HttpMetodo Metodo { get; }

    public string Rota { get; }
}
