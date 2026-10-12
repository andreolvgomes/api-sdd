using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class FormaPagamentoRepository : IFormaPagamentoRepository
{
    private readonly IDbConnection _conexao;

    public FormaPagamentoRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<bool> ExisteDescricaoAsync(string descricao, int idIgnorado)
    {
        int quantidade = await _conexao.ExecuteScalarAsync<int>(
            "select count(1) from forma_pagamento where descricao = @descricao collate nocase and id <> @idIgnorado",
            new { descricao, idIgnorado });

        return quantidade > 0;
    }

    public async Task<int> InserirAsync(FormaPagamento formaPagamento)
    {
        return await _conexao.ExecuteScalarAsync<int>(
            "insert into forma_pagamento (descricao, tipo, status) values (@Descricao, @Tipo, @Status) returning id",
            formaPagamento);
    }

    public async Task<FormaPagamento?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<FormaPagamento>(
            "select id, descricao, tipo, status from forma_pagamento where id = @id", new { id });
    }

    public async Task<List<FormaPagamento>> ListarAsync(string? status, int? tipo)
    {
        string sql = "select id, descricao, tipo, status from forma_pagamento";

        if (status != null && tipo != null)
            sql = sql + " where status = @status and tipo = @tipo";
        else if (status != null)
            sql = sql + " where status = @status";
        else if (tipo != null)
            sql = sql + " where tipo = @tipo";

        sql = sql + " order by id";

        IEnumerable<FormaPagamento> formasPagamento = await _conexao.QueryAsync<FormaPagamento>(sql, new { status, tipo });

        return formasPagamento.ToList();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from forma_pagamento where id = @id", new { id });

        return linhas > 0;
    }

    public async Task<bool> AtualizarAsync(FormaPagamento formaPagamento)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update forma_pagamento set descricao = @Descricao, tipo = @Tipo, status = @Status where id = @Id",
            formaPagamento);

        return linhas > 0;
    }
}
