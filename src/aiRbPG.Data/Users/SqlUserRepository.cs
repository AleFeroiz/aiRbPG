using aiRbPG.Domain.Users;
using aiRbPG.Domain;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace aiRbPG.Data.Users
{
    public sealed class SqlUserRepository : IUserRepository
    {
        private readonly SqlConnectionFactory connections;
        public SqlUserRepository(SqlConnectionFactory connections) { this.connections = connections; }

        public async Task<bool> HasAnyUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand("SELECT TOP (1) 1 FROM dbo.Usuarios;", connection))
                    return await command.ExecuteScalarAsync(cancellationToken) != null;
            }
        }

        public async Task<UserAccount> FindByLoginAsync(string login, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT UsuarioId, Nome, Login, Perfil, SenhaSalt, SenhaHash, IteracoesHash, Ativo FROM dbo.Usuarios WHERE Login = @Login;";
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Login", SqlDbType.NVarChar, 60).Value = login;
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                    {
                        if (!await reader.ReadAsync(cancellationToken)) return null;
                        return new UserAccount(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), ParseRole(reader.GetString(3)), (byte[])reader[4], (byte[])reader[5], reader.GetInt32(6), reader.GetBoolean(7));
                    }
                }
            }
        }

        public async Task<OperationResult<bool>> CreateFirstAdministratorAsync(string fullName, string login, byte[] passwordSalt, byte[] passwordHash, int hashIterations, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "INSERT dbo.Usuarios (Nome, Login, Perfil, SenhaSalt, SenhaHash, IteracoesHash) VALUES (@Nome, @Login, N'Administrador', @SenhaSalt, @SenhaHash, @Iteracoes);";
            using (var connection = connections.Create())
            {
                await connection.OpenAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (var countCommand = new SqlCommand("SELECT COUNT_BIG(*) FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK);", connection, transaction))
                        {
                            var existingUsers = (long)(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0L);
                            if (existingUsers != 0) return OperationResult<bool>.Failure("O Administrador inicial já foi criado.");
                        }

                        using (var command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = fullName;
                            command.Parameters.Add("@Login", SqlDbType.NVarChar, 60).Value = login;
                            command.Parameters.Add("@SenhaSalt", SqlDbType.VarBinary, 16).Value = passwordSalt;
                            command.Parameters.Add("@SenhaHash", SqlDbType.VarBinary, 32).Value = passwordHash;
                            command.Parameters.Add("@Iteracoes", SqlDbType.Int).Value = hashIterations;
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static UserRole ParseRole(string role)
        {
            if (role == "Administrador") return UserRole.Administrator;
            if (role == "Atendente") return UserRole.Attendant;
            throw new InvalidOperationException("Perfil de usuário desconhecido: " + role + ".");
        }
    }
}
