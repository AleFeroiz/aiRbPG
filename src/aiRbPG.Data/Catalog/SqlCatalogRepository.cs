using aiRbPG.Domain.Catalog;
using Microsoft.Data.SqlClient;
using System.Data;

namespace aiRbPG.Data.Catalog;

public sealed class SqlCatalogRepository(SqlConnectionFactory connections) : ICatalogRepository
{
    public async Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT ClienteId, Nome, Telefone, Email, Ativo FROM dbo.Clientes WHERE Ativo = 1 ORDER BY Nome;";
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var clients = new List<Client>();
        while (await reader.ReadAsync(cancellationToken))
        {
            clients.Add(new Client(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetBoolean(4)));
        }

        return clients;
    }

    public Task SaveClientAsync(Client client, CancellationToken cancellationToken = default) =>
        SaveAsync(client.Id, """
            INSERT dbo.Clientes (Nome, Telefone, Email) VALUES (@Nome, @Telefone, @Email);
            """, """
            UPDATE dbo.Clientes SET Nome = @Nome, Telefone = @Telefone, Email = @Email WHERE ClienteId = @Id AND Ativo = 1;
            """, command =>
        {
            command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = client.Name.Trim();
            command.Parameters.Add("@Telefone", SqlDbType.NVarChar, 30).Value = (object?)client.Phone?.Trim() ?? DBNull.Value;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = (object?)client.Email?.Trim() ?? DBNull.Value;
        }, cancellationToken);

    public Task DeactivateClientAsync(int clientId, CancellationToken cancellationToken = default) =>
        DeactivateAsync("UPDATE dbo.Clientes SET Ativo = 0 WHERE ClienteId = @Id;", clientId, cancellationToken);

    public async Task<IReadOnlyList<RpgSpace>> GetSpacesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EspacoId, Nome, Descricao, Endereco, Capacidade, TarifaHora, TarifaPessoaHora, Ativo
            FROM dbo.Espacos WHERE Ativo = 1 ORDER BY Nome;
            """;
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var spaces = new List<RpgSpace>();
        while (await reader.ReadAsync(cancellationToken))
        {
            spaces.Add(new RpgSpace(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetInt32(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetBoolean(7)));
        }

        return spaces;
    }

    public Task SaveSpaceAsync(RpgSpace space, CancellationToken cancellationToken = default) =>
        SaveAsync(space.Id, """
            INSERT dbo.Espacos (Nome, Descricao, Endereco, Capacidade, TarifaHora, TarifaPessoaHora)
            VALUES (@Nome, @Descricao, @Endereco, @Capacidade, @TarifaHora, @TarifaPessoaHora);
            """, """
            UPDATE dbo.Espacos
            SET Nome = @Nome, Descricao = @Descricao, Endereco = @Endereco, Capacidade = @Capacidade,
                TarifaHora = @TarifaHora, TarifaPessoaHora = @TarifaPessoaHora
            WHERE EspacoId = @Id AND Ativo = 1;
            """, command =>
        {
            command.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = space.Name.Trim();
            command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object?)space.Description?.Trim() ?? DBNull.Value;
            command.Parameters.Add("@Endereco", SqlDbType.NVarChar, 250).Value = (object?)space.Address?.Trim() ?? DBNull.Value;
            command.Parameters.Add("@Capacidade", SqlDbType.Int).Value = space.Capacity;
            command.Parameters.Add("@TarifaHora", SqlDbType.Decimal).Value = space.HourlyRate;
            command.Parameters.Add("@TarifaPessoaHora", SqlDbType.Decimal).Value = space.PerPersonHourlyRate;
        }, cancellationToken);

    public Task DeactivateSpaceAsync(int spaceId, CancellationToken cancellationToken = default) =>
        DeactivateAsync("UPDATE dbo.Espacos SET Ativo = 0 WHERE EspacoId = @Id;", spaceId, cancellationToken);

    public async Task<IReadOnlyList<RpgSystem>> GetRpgSystemsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT SistemaRpgId, Nome, Descricao, Ativo FROM dbo.SistemasRpg WHERE Ativo = 1 ORDER BY Nome;";
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var systems = new List<RpgSystem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            systems.Add(new RpgSystem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetBoolean(3)));
        }

        return systems;
    }

    public Task SaveRpgSystemAsync(RpgSystem system, CancellationToken cancellationToken = default) =>
        SaveAsync(system.Id, "INSERT dbo.SistemasRpg (Nome, Descricao) VALUES (@Nome, @Descricao);",
            "UPDATE dbo.SistemasRpg SET Nome = @Nome, Descricao = @Descricao WHERE SistemaRpgId = @Id AND Ativo = 1;",
            command =>
            {
                command.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = system.Name.Trim();
                command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object?)system.Description?.Trim() ?? DBNull.Value;
            }, cancellationToken);

    public Task DeactivateRpgSystemAsync(int systemId, CancellationToken cancellationToken = default) =>
        DeactivateAsync("UPDATE dbo.SistemasRpg SET Ativo = 0 WHERE SistemaRpgId = @Id;", systemId, cancellationToken);

    public async Task<IReadOnlyList<Accessory>> GetAccessoriesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT AcessorioId, Nome, Descricao, QuantidadeEstoque, TarifaHora, Ativo
            FROM dbo.Acessorios WHERE Ativo = 1 ORDER BY Nome;
            """;
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var accessories = new List<Accessory>();
        while (await reader.ReadAsync(cancellationToken))
        {
            accessories.Add(new Accessory(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetInt32(3),
                reader.GetDecimal(4),
                reader.GetBoolean(5)));
        }

        return accessories;
    }

    public Task SaveAccessoryAsync(Accessory accessory, CancellationToken cancellationToken = default) =>
        SaveAsync(accessory.Id, """
            INSERT dbo.Acessorios (Nome, Descricao, QuantidadeEstoque, TarifaHora)
            VALUES (@Nome, @Descricao, @QuantidadeEstoque, @TarifaHora);
            """, """
            UPDATE dbo.Acessorios
            SET Nome = @Nome, Descricao = @Descricao, QuantidadeEstoque = @QuantidadeEstoque, TarifaHora = @TarifaHora
            WHERE AcessorioId = @Id AND Ativo = 1;
            """, command =>
        {
            command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = accessory.Name.Trim();
            command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object?)accessory.Description?.Trim() ?? DBNull.Value;
            command.Parameters.Add("@QuantidadeEstoque", SqlDbType.Int).Value = accessory.QuantityInStock;
            command.Parameters.Add("@TarifaHora", SqlDbType.Decimal).Value = accessory.HourlyRate;
        }, cancellationToken);

    public Task DeactivateAccessoryAsync(int accessoryId, CancellationToken cancellationToken = default) =>
        DeactivateAsync("UPDATE dbo.Acessorios SET Ativo = 0 WHERE AcessorioId = @Id;", accessoryId, cancellationToken);

    private async Task SaveAsync(
        int id,
        string insertSql,
        string updateSql,
        Action<SqlCommand> addParameters,
        CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(id == 0 ? insertSql : updateSql, connection);
        addParameters(command);
        if (id != 0)
        {
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task DeactivateAsync(string sql, int id, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
