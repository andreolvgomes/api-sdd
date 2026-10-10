using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private const string Colunas = "id, nome, endereco, telefone";

    private readonly IDbConnection _conexao;

    public ClienteRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(Cliente cliente)
    {
        return await _conexao.QuerySingleAsync<int>(
            "insert into cliente (nome, endereco, telefone) values (@Nome, @Endereco, @Telefone) returning id",
            cliente);
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _conexao.QueryFirstOrDefaultAsync<Cliente>(
            $"select {Colunas} from cliente where id = @id", new { id });
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        IEnumerable<Cliente> clientes = await _conexao.QueryAsync<Cliente>(
            $"select {Colunas} from cliente order by id");

        return clientes.ToList();
    }

    public async Task<bool> AtualizarAsync(Cliente cliente)
    {
        int linhas = await _conexao.ExecuteAsync(
            "update cliente set nome = @Nome, endereco = @Endereco, telefone = @Telefone where id = @Id",
            cliente);

        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        int linhas = await _conexao.ExecuteAsync("delete from cliente where id = @id", new { id });

        return linhas > 0;
    }
}
