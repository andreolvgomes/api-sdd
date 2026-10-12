namespace Erp.Application.Vendedores;

public static class VendedorValidacao
{
    public const string StatusAtivo = "Ativo";
    public const string StatusInativo = "Inativo";

    public static string? ValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return "Nome é obrigatório.";

        if (nome.Trim().Length < 3 || nome.Trim().Length > 100)
            return "Nome deve ter entre 3 e 100 caracteres.";

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
