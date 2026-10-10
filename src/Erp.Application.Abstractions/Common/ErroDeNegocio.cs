namespace Erp.Application.Abstractions.Common;

public class ErroDeNegocio : Exception
{
    public ErroDeNegocio(string mensagem, TipoErroDeNegocio tipo) : base(mensagem)
    {
        Tipo = tipo;
    }

    public TipoErroDeNegocio Tipo { get; }
}
