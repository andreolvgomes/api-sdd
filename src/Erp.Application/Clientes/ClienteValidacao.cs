namespace Erp.Application.Clientes;

public static class ClienteValidacao
{
    public const int LimiteEnderecosAdicionais = 10;

    public static string? Validar(string? nome, string? endereco, string? telefone)
    {
        string? erro = ValidarNome(nome);

        if (erro == null)
            erro = ValidarEndereco(endereco);

        if (erro == null)
            erro = ValidarTelefone(telefone);

        return erro;
    }

    public static string? ValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return "Nome completo é obrigatório";

        if (nome.Length > 100)
            return "Nome completo deve ter no máximo 100 caracteres";

        return null;
    }

    public static string? ValidarEndereco(string? endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            return "Endereço é obrigatório";

        if (endereco.Length > 200)
            return "Endereço deve ter no máximo 200 caracteres";

        return null;
    }

    public static string? ValidarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return "Telefone é obrigatório";

        if (telefone.Length > 20)
            return "Telefone deve ter no máximo 20 caracteres";

        return null;
    }

    public static string? ValidarRotulo(string? rotulo)
    {
        if (string.IsNullOrWhiteSpace(rotulo))
            return "Rótulo é obrigatório";

        if (rotulo.Length > 30)
            return "Rótulo deve ter no máximo 30 caracteres";

        return null;
    }

    public static string? ValidarStatus(string? status)
    {
        if (status != "ativo" && status != "inativo")
            return "Status deve ser ativo ou inativo";

        return null;
    }

    public static string? ValidarBloqueio(string? bloqueio)
    {
        if (bloqueio != "liberado" && bloqueio != "bloqueado")
            return "Nível de bloqueio deve ser liberado ou bloqueado";

        return null;
    }

    public static string? ValidarFiltroStatus(string? status)
    {
        if (status != "ativo" && status != "inativo" && status != "todos")
            return "Status deve ser ativo, inativo ou todos";

        return null;
    }
}
