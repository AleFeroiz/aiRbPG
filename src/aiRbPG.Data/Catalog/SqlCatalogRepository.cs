using aiRbPG.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Data.Catalog
{
    public sealed class SqlCatalogRepository : ICatalogRepository
    {
        private readonly SqlConnectionFactory connections;
        public SqlCatalogRepository(SqlConnectionFactory connections) { this.connections = connections; }

        public async Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT ClienteId, Nome, Telefone, Email, Ativo FROM dbo.Clientes WHERE Ativo = 1 ORDER BY Nome;";
            var clients = new List<Client>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    while (await reader.ReadAsync(cancellationToken))
                        clients.Add(new Client(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetBoolean(4)));
            }
            return clients;
        }

        public Task SaveClientAsync(Client client, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveAsync(client.Id,
                "INSERT dbo.Clientes (Nome, Telefone, Email) VALUES (@Nome, @Telefone, @Email);",
                "UPDATE dbo.Clientes SET Nome = @Nome, Telefone = @Telefone, Email = @Email WHERE ClienteId = @Id AND Ativo = 1;",
                delegate(SqlCommand command)
                {
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = client.Name.Trim();
                    command.Parameters.Add("@Telefone", SqlDbType.NVarChar, 30).Value = (object)OptionalTrim(client.Phone) ?? DBNull.Value;
                    command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = (object)OptionalTrim(client.Email) ?? DBNull.Value;
                }, cancellationToken);
        }

        public Task DeactivateClientAsync(int clientId, CancellationToken cancellationToken = default(CancellationToken))
        { return DeactivateAsync("UPDATE dbo.Clientes SET Ativo = 0 WHERE ClienteId = @Id;", clientId, cancellationToken); }

        public async Task<IReadOnlyList<RpgSpace>> GetSpacesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT EspacoId, Nome, Descricao, Endereco, Capacidade, TarifaHora, TarifaPessoaHora, Ativo FROM dbo.Espacos WHERE Ativo = 1 ORDER BY Nome;";
            var spaces = new List<RpgSpace>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    while (await reader.ReadAsync(cancellationToken))
                        spaces.Add(new RpgSpace(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.GetInt32(4), reader.GetDecimal(5), reader.GetDecimal(6), reader.GetBoolean(7)));
            }
            return spaces;
        }

        public Task SaveSpaceAsync(RpgSpace space, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveAsync(space.Id,
                "INSERT dbo.Espacos (Nome, Descricao, Endereco, Capacidade, TarifaHora, TarifaPessoaHora) VALUES (@Nome, @Descricao, @Endereco, @Capacidade, @TarifaHora, @TarifaPessoaHora);",
                "UPDATE dbo.Espacos SET Nome=@Nome, Descricao=@Descricao, Endereco=@Endereco, Capacidade=@Capacidade, TarifaHora=@TarifaHora, TarifaPessoaHora=@TarifaPessoaHora WHERE EspacoId=@Id AND Ativo=1;",
                delegate(SqlCommand command)
                {
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = space.Name.Trim();
                    command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object)OptionalTrim(space.Description) ?? DBNull.Value;
                    command.Parameters.Add("@Endereco", SqlDbType.NVarChar, 250).Value = (object)OptionalTrim(space.Address) ?? DBNull.Value;
                    command.Parameters.Add("@Capacidade", SqlDbType.Int).Value = space.Capacity;
                    AddDecimal(command, "@TarifaHora", space.HourlyRate);
                    AddDecimal(command, "@TarifaPessoaHora", space.PerPersonHourlyRate);
                }, cancellationToken);
        }

        public Task DeactivateSpaceAsync(int spaceId, CancellationToken cancellationToken = default(CancellationToken))
        { return DeactivateAsync("UPDATE dbo.Espacos SET Ativo = 0 WHERE EspacoId = @Id;", spaceId, cancellationToken); }

        public async Task<IReadOnlyList<RpgSystem>> GetRpgSystemsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT SistemaRpgId, Nome, Descricao, Ativo FROM dbo.SistemasRpg WHERE Ativo = 1 ORDER BY Nome;";
            var systems = new List<RpgSystem>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    while (await reader.ReadAsync(cancellationToken))
                        systems.Add(new RpgSystem(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetBoolean(3)));
            }
            return systems;
        }

        public Task SaveRpgSystemAsync(RpgSystem system, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveAsync(system.Id,
                "INSERT dbo.SistemasRpg (Nome, Descricao) VALUES (@Nome, @Descricao);",
                "UPDATE dbo.SistemasRpg SET Nome=@Nome, Descricao=@Descricao WHERE SistemaRpgId=@Id AND Ativo=1;",
                delegate(SqlCommand command)
                {
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = system.Name.Trim();
                    command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object)OptionalTrim(system.Description) ?? DBNull.Value;
                }, cancellationToken);
        }

        public Task DeactivateRpgSystemAsync(int systemId, CancellationToken cancellationToken = default(CancellationToken))
        { return DeactivateAsync("UPDATE dbo.SistemasRpg SET Ativo = 0 WHERE SistemaRpgId = @Id;", systemId, cancellationToken); }

        public async Task<IReadOnlyList<Accessory>> GetAccessoriesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT AcessorioId, Nome, Descricao, QuantidadeEstoque, TarifaHora, Ativo FROM dbo.Acessorios WHERE Ativo = 1 ORDER BY Nome;";
            var accessories = new List<Accessory>();
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    while (await reader.ReadAsync(cancellationToken))
                        accessories.Add(new Accessory(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetInt32(3), reader.GetDecimal(4), reader.GetBoolean(5)));
            }
            return accessories;
        }

        public Task SaveAccessoryAsync(Accessory accessory, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveAsync(accessory.Id,
                "INSERT dbo.Acessorios (Nome, Descricao, QuantidadeEstoque, TarifaHora) VALUES (@Nome, @Descricao, @QuantidadeEstoque, @TarifaHora);",
                "UPDATE dbo.Acessorios SET Nome=@Nome, Descricao=@Descricao, QuantidadeEstoque=@QuantidadeEstoque, TarifaHora=@TarifaHora WHERE AcessorioId=@Id AND Ativo=1;",
                delegate(SqlCommand command)
                {
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = accessory.Name.Trim();
                    command.Parameters.Add("@Descricao", SqlDbType.NVarChar, 500).Value = (object)OptionalTrim(accessory.Description) ?? DBNull.Value;
                    command.Parameters.Add("@QuantidadeEstoque", SqlDbType.Int).Value = accessory.QuantityInStock;
                    AddDecimal(command, "@TarifaHora", accessory.HourlyRate);
                }, cancellationToken);
        }

        public Task DeactivateAccessoryAsync(int accessoryId, CancellationToken cancellationToken = default(CancellationToken))
        { return DeactivateAsync("UPDATE dbo.Acessorios SET Ativo = 0 WHERE AcessorioId = @Id;", accessoryId, cancellationToken); }

        private async Task SaveAsync(int id, string insertSql, string updateSql, Action<SqlCommand> addParameters, CancellationToken cancellationToken)
        {
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(id == 0 ? insertSql : updateSql, connection))
                {
                    addParameters(command);
                    if (id != 0) command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        private async Task DeactivateAsync(string sql, int id, CancellationToken cancellationToken)
        {
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        private static string OptionalTrim(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        private static void AddDecimal(SqlCommand command, string name, decimal value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 10; parameter.Scale = 2; parameter.Value = value;
        }
    }
}
