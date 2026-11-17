using System;
using System.Data.SqlClient;

namespace aiRbPG.Data
{
public sealed class SqlConnectionFactory
{
    public const string ConnectionEnvironmentVariable = "AIRBPG_SQL_CONNECTION";

    public SqlConnection Create()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Defina a variável de ambiente {ConnectionEnvironmentVariable} com a conexão do SQL Server.");
        }

        return new SqlConnection(connectionString);
    }
}
}
