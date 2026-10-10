namespace Erp.Application.Produtos;

public static class ProdutoValidacao
{
    public const string StatusAtivo = "ativo";
    public const string StatusInativo = "inativo";

    public static string? Validar(string? codigo, string? descricao, decimal? precoVenda, string? status)
    {
        string? erro = ValidarCodigo(codigo);

        if (erro == null)
            erro = ValidarDescricao(descricao);

        if (erro == null)
            erro = ValidarPreco(precoVenda);

        if (erro == null)
            erro = ValidarStatus(status);

        return erro;
    }

    public static string? ValidarAlteracao(string? descricao, decimal? precoVenda, string? status)
    {
        string? erro = ValidarDescricao(descricao);

        if (erro == null)
            erro = ValidarPreco(precoVenda);

        if (erro == null)
            erro = ValidarStatus(status);

        return erro;
    }

    public static string? ValidarCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return "Código é obrigatório";

        if (codigo.Length > 20)
            return "Código deve ter no máximo 20 caracteres";

        foreach (char letra in codigo)
        {
            if (!EhLetraOuNumero(letra))
                return "Código aceita apenas letras e números";
        }

        return null;
    }

    public static string? ValidarDescricao(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return "Descrição é obrigatória";

        if (descricao.Length > 100)
            return "Descrição deve ter no máximo 100 caracteres";

        return null;
    }

    public static string? ValidarPreco(decimal? precoVenda)
    {
        if (precoVenda == null)
            return "Preço de venda é obrigatório";

        if (precoVenda < 0)
            return "Preço não pode ser negativo";

        if (Math.Round(precoVenda.Value, 2) != precoVenda.Value)
            return "Preço aceita no máximo 2 casas decimais";

        return null;
    }

    public static string? ValidarStatus(string? status)
    {
        if (status != StatusAtivo && status != StatusInativo)
            return "Status deve ser ativo ou inativo";

        return null;
    }

    private static bool EhLetraOuNumero(char letra)
    {
        return (letra >= 'a' && letra <= 'z')
            || (letra >= 'A' && letra <= 'Z')
            || (letra >= '0' && letra <= '9');
    }
}
