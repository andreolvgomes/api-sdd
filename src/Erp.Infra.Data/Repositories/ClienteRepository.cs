using System.Data;
using Dapper;
using Erp.Core.Entities;
using Erp.Core.Repositories;

namespace Erp.Infra.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private const string Colunas = "id, nome, endereco, telefone, status";

    private readonly IDbConnection _conexao;

    public ClienteRepository(IDbConnection conexao)
    {
        _conexao = conexao;
    }

    public async Task<int> InserirAsync(Cliente cliente)
    {
        return await _conexao.QuerySingleAsync<int>(
            "insert into cliente (nome, endereco, telefone, status) values (@Nome, @Endereco, @Telefone, @Status) returning id",
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
            "update cliente set nome = @Nome, endereco = @Endereco, telefone = @Telefone, status = @Status where id = @Id",
            cliente);

        return linhas > 0;
    }
}
