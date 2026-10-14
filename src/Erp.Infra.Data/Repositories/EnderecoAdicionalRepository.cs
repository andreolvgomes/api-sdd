using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class EnderecoAdicionalRepository : IEnderecoAdicionalRepository
{
    private const string Colunas = "clie_id, ende_id, ende_rotulo, ende_endereco";

    private readonly IDbConnection _conexao;

    public EnderecoAdicionalRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(EnderecoAdicional endereco)
    {
        return await _conexao.QuerySingleAsync<int>(
            @"insert into endereco_adicional (clie_id, ende_id, ende_rotulo, ende_endereco)
              select @Clie_id, coalesce(max(ende_id), 0) + 1, @Ende_rotulo, @Ende_endereco
              from endereco_adicional where clie_id = @Clie_id
              returning ende_id",
            endereco);
    }

    public async Task<List<EnderecoAdicional>> ListarPorClienteAsync(int clienteId)
    {
        IEnumerable<EnderecoAdicional> enderecos = await _conexao.QueryAsync<EnderecoAdicional>(
            $"select {Colunas} from endereco_adicional where clie_id = @clienteId order by ende_id", new { clienteId });

        return enderecos.ToList();
    }

    public async Task<EnderecoAdicional?> ObterAsync(int clienteId, int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<EnderecoAdicional>(
            $"select {Colunas} from endereco_adicional where clie_id = @clienteId and ende_id = @id", new { clienteId, id });
    }

    public async Task<bool> AtualizarAsync(EnderecoAdicional endereco)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update endereco_adicional set ende_rotulo = @Ende_rotulo, ende_endereco = @Ende_endereco where clie_id = @Clie_id and ende_id = @Ende_id",
            endereco);

        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int clienteId, int id)
    {
        int linhas = await _conexao.ExecuteAsync(
            "delete from endereco_adicional where clie_id = @clienteId and ende_id = @id", new { clienteId, id });

        return linhas > 0;
    }
}