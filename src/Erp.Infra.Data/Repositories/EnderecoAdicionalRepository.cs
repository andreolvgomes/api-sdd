using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class EnderecoAdicionalRepository : IEnderecoAdicionalRepository
{
    private const string Colunas = "id as Id, cliente_id as ClienteId, rotulo as Rotulo, endereco as Endereco";

    private readonly IDbConnection _conexao;

    public EnderecoAdicionalRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(EnderecoAdicional endereco)
    {
        return await _conexao.QuerySingleAsync<int>(
            @"insert into endereco_adicional (cliente_id, id, rotulo, endereco)
              select @ClienteId, coalesce(max(id), 0) + 1, @Rotulo, @Endereco
              from endereco_adicional where cliente_id = @ClienteId
              returning id",
            endereco);
    }

    public async Task<List<EnderecoAdicional>> ListarPorClienteAsync(int clienteId)
    {
        IEnumerable<EnderecoAdicional> enderecos = await _conexao.QueryAsync<EnderecoAdicional>(
            $"select {Colunas} from endereco_adicional where cliente_id = @clienteId order by id", new { clienteId });

        return enderecos.ToList();
    }

    public async Task<EnderecoAdicional?> ObterAsync(int clienteId, int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<EnderecoAdicional>(
            $"select {Colunas} from endereco_adicional where cliente_id = @clienteId and id = @id", new { clienteId, id });
    }

    public async Task<bool> AtualizarAsync(EnderecoAdicional endereco)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update endereco_adicional set rotulo = @Rotulo, endereco = @Endereco where cliente_id = @ClienteId and id = @Id",
            endereco);

        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int clienteId, int id)
    {
        int linhas = await _conexao.ExecuteAsync(
            "delete from endereco_adicional where cliente_id = @clienteId and id = @id", new { clienteId, id });

        return linhas > 0;
    }
}
