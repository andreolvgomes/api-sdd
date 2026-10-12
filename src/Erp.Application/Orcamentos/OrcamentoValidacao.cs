namespace Erp.Application.Orcamentos;

public static class OrcamentoValidacao
{
    public static string? ValidarItem(string? codigoProduto, decimal? quantidade)
    {
        if (string.IsNullOrWhiteSpace(codigoProduto))
            return "Produto é obrigatório.";

        return ValidarQuantidade(quantidade);
    }

    public static string? ValidarQuantidade(decimal? quantidade)
    {
        if (quantidade == null)
            return "Quantidade é obrigatória.";

        if (quantidade <= 0)
            return "Quantidade deve ser maior que zero.";

        if (Math.Round(quantidade.Value, 2) != quantidade.Value)
            return "Quantidade deve ter até 2 casas decimais.";

        return null;
    }

    public static string? ValidarDesconto(decimal desconto)
    {
        if (desconto < 0)
            return "Desconto não pode ser negativo.";

        if (Math.Round(desconto, 2) != desconto)
            return "Desconto deve ter até 2 casas decimais.";

        return null;
    }

    public static bool EhAtivo(string status)
    {
        return string.Equals(status, "ativo", StringComparison.OrdinalIgnoreCase);
    }

    public static decimal Arredondar(decimal valor)
    {
        return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
}
