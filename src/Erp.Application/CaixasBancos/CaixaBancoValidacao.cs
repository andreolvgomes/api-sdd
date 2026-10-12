namespace Erp.Application.CaixasBancos;

public static class CaixaBancoValidacao
{
    public const string StatusAtivo = "Ativo";
    public const string StatusInativo = "Inativo";

    public static string? ValidarDescricao(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return "Descrição é obrigatória.";

        if (descricao.Trim().Length < 3 || descricao.Trim().Length > 100)
            return "Descrição deve ter entre 3 e 100 caracteres.";

        return null;
    }

    public static string? ValidarStatus(string? status)
    {
        if (NormalizarStatus(status) == "")
            return "Status inválido.";

        return null;
    }

    public static string NormalizarStatus(string? status)
    {
        if (string.Equals(status, StatusAtivo, StringComparison.OrdinalIgnoreCase))
            return StatusAtivo;

        if (string.Equals(status, StatusInativo, StringComparison.OrdinalIgnoreCase))
            return StatusInativo;

        return "";
    }
}
