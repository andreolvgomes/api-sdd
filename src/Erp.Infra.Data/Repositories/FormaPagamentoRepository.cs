using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class FormaPagamentoRepository : IFormaPagamentoRepository
{
    private const string Colunas = "form_id, form_descricao, form_tipo, form_status";

    private readonly IDbConnection _conexao;

    public FormaPagamentoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from forma_pagamento where form_descricao = @descricao collate nocase and form_id <> @idIgnorado",
            new { descricao, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(FormaPagamento formaPagamento)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into forma_pagamento (form_descricao, form_tipo, form_status) values (@Form_descricao, @Form_tipo, @Form_status) returning form_id",
            formaPagamento);
    }

    public async Task<FormaPagamento?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<FormaPagamento>(
            $"select {Colunas} from forma_pagamento where form_id = @id", new { id });
    }

    public async Task<List<FormaPagamento>> ListarAsync(string? status, int? tipo)
    {
        string sql = $"select {Colunas} from forma_pagamento";

        if (status != null && tipo != null)
            sql = sql + " where form_status = @status and form_tipo = @tipo";
        else if (status != null)
            sql = sql + " where form_status = @status";
        else if (tipo != null)
            sql = sql + " where form_tipo = @tipo";

        sql = sql + " order by form_id";

        IEnumerable<FormaPagamento> formasPagamento = await _conexao.QueryAsync<FormaPagamento>(sql, new { status, tipo });

        return formasPagamento.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from forma_pagamento where form_id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(FormaPagamento formaPagamento)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update forma_pagamento set form_descricao = @Form_descricao, form_tipo = @Form_tipo, form_status = @Form_status where form_id = @Form_id",
            formaPagamento);

        return linhas > 0;
    }
}