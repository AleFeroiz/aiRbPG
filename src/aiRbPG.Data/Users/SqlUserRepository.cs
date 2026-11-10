using aiRbPG.Domain.Users;
using Microsoft.Data.SqlClient;
using System.Data;

namespace aiRbPG.Data.Users;

public sealed class SqlUserRepository(SqlConnectionFactory connections) : IUserRepository
{
    public async Task<bool> HasAnyUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT TOP (1) 1 FROM dbo.Usuarios;", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task<UserAccount?> FindByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT UsuarioId, Nome, Login, Perfil, SenhaSalt, SenhaHash, IteracoesHash, Ativo
            FROM dbo.Usuarios
            WHERE Login = @Login;
            """;

        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Login", SqlDbType.NVarChar, 60).Value = login;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserAccount(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            ParseRole(reader.GetString(3)),
            (byte[])reader[4],
            (byte[])reader[5],
            reader.GetInt32(6),
            reader.GetBoolean(7));
    }

    public async Task CreateFirstAdministratorAsync(
        string fullName,
        string login,
        byte[] passwordSalt,
        byte[] passwordHash,
        int hashIterations,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT dbo.Usuarios (Nome, Login, Perfil, SenhaSalt, SenhaHash, IteracoesHash)
            VALUES (@Nome, @Login, N'Administrador', @SenhaSalt, @SenhaHash, @IteracoesHash);
            """;

        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            await using (var countCommand = new SqlCommand(
                "SELECT COUNT_BIG(1) FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK);",
                connection,
                transaction))
            {
                var existingUsers = (long)(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0L);
                if (existingUsers > 0)
                {
                    throw new InvalidOperationException("Já existe um usuário. O Administrador inicial não pode ser recriado.");
                }
            }

            await using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = fullName;
                command.Parameters.Add("@Login", SqlDbType.NVarChar, 60).Value = login;
                command.Parameters.Add("@SenhaSalt", SqlDbType.VarBinary, 16).Value = passwordSalt;
                command.Parameters.Add("@SenhaHash", SqlDbType.VarBinary, 32).Value = passwordHash;
                command.Parameters.Add("@IteracoesHash", SqlDbType.Int).Value = hashIterations;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static UserRole ParseRole(string role) => role switch
    {
        "Administrador" => UserRole.Administrator,
        "Atendente" => UserRole.Attendant,
        _ => throw new InvalidOperationException($"Perfil de usuário desconhecido: {role}.")
    };
}
