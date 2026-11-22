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
        private readonly int userId;
        public SqlCatalogRepository(SqlConnectionFactory connections, aiRbPG.Domain.Users.UserAccount user)
        { this.connections = connections; if (user == null) throw new ArgumentNullException("user"); userId = user.Id; }

        private static void Name(string value, int max)
        { if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > max) throw new ArgumentException("Nome obrigatório, limitado a " + max + " caracteres."); }
        private static void Length(string value, int max)
        { if (value != null && value.Length > max) throw new ArgumentException("Campo excede o limite de " + max + " caracteres."); }

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
            Name(client.Name,120); Length(client.Phone,30); Length(client.Email,254);
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
            Name(space.Name,100); Length(space.Description,500); Length(space.Address,250);
            if (space.Capacity <= 0 || space.HourlyRate < 0 || space.PerPersonHourlyRate < 0) throw new ArgumentException("Capacidade e tarifas inválidas.");
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
            Name(system.Name,100); Length(system.Description,500);
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
            Name(accessory.Name,120); Length(accessory.Description,500);
            if (accessory.QuantityInStock < 0 || accessory.HourlyRate < 0) throw new ArgumentException("Estoque e tarifa não podem ser negativos.");
            return SaveAsync(accessory.Id,
                "INSERT dbo.Acessorios (Nome, Descricao, QuantidadeEstoque, TarifaHora) VALUES (@Nome, @Descricao, @QuantidadeEstoque, @TarifaHora);",
                "IF @QuantidadeEstoque < (SELECT COALESCE(SUM(i.QuantidadeReservada-i.QuantidadeDevolvida-i.QuantidadeDanificada-i.QuantidadePerdida),0) FROM dbo.ItensReserva i JOIN dbo.Reservas r ON r.ReservaId=i.ReservaId WHERE i.AcessorioId=@Id AND r.Status NOT IN(N'Cancelada',N'Finalizada')) THROW 50002,N'Estoque inferior às quantidades comprometidas em reservas abertas.',1; UPDATE dbo.Acessorios SET Nome=@Nome, Descricao=@Descricao, QuantidadeEstoque=@QuantidadeEstoque, TarifaHora=@TarifaHora WHERE AcessorioId=@Id AND Ativo=1;",
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
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                using (var command = new SqlCommand(Permission(insertSql) + (id == 0 ? insertSql : updateSql), connection, transaction))
                {
                    command.Parameters.AddWithValue("@User",userId);
                    addParameters(command);
                    if (id != 0) command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    if (await command.ExecuteNonQueryAsync(cancellationToken) != 1) throw new InvalidOperationException("Registro inativo ou inexistente.");
                    transaction.Commit();
                }
            }
        }

        private async Task DeactivateAsync(string sql, int id, CancellationToken cancellationToken)
        {
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(Permission(sql) + sql, connection))
                {
                    command.Parameters.AddWithValue("@User",userId);
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        private static string Permission(string sql)
        {
            var role = sql.Contains("dbo.Clientes") ? "IN(N'Administrador',N'Atendente')" : "=N'Administrador'";
            return "IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE UsuarioId=@User AND Ativo=1 AND Perfil " + role + ") THROW 50001,N'Usuário sem permissão para este cadastro.',1; ";
        }

        private static string OptionalTrim(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        private static void AddDecimal(SqlCommand command, string name, decimal value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 10; parameter.Scale = 2; parameter.Value = value;
        }
    }
}
