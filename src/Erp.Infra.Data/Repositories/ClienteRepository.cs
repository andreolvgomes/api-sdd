using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private const string Colunas = "clie_id, clie_nome, clie_endereco, clie_telefone, clie_status";

    private readonly IDbConnection _conexao;

    public ClienteRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(Cliente cliente)
    {
        return await _conexao.QuerySingleAsync<int>(
            "insert into cliente (clie_nome, clie_endereco, clie_telefone, clie_status) values (@Clie_nome, @Clie_endereco, @Clie_telefone, @Clie_status) returning clie_id",
            cliente);
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Cliente>(
            $"select {Colunas} from cliente where clie_id = @id", new { id });
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        IEnumerable<Cliente> clientes = await _conexao.QueryAsync<Cliente>(
            $"select {Colunas} from cliente order by clie_id");

        return clientes.ToList();
    }

    public async Task<bool> AtualizarAsync(Cliente cliente)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update cliente set clie_nome = @Clie_nome, clie_endereco = @Clie_endereco, clie_telefone = @Clie_telefone, clie_status = @Clie_status where clie_id = @Clie_id",
            cliente);

        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from cliente where clie_id = @id", new { id });

        return linhas > 0;
    }
}