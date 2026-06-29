using EucSaaS.Domain.Entities;
using Npgsql;
using System.Data;

namespace EucSaaS.Application.Services;

public class DashboardQueryService
{
    public async Task<DataTable> TestSqlAsync(
        DataSource dataSource,
        string sqlQuery,
        int maxRows = 20)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
            throw new InvalidOperationException("SQL query is required.");

        var trimmedSql = sqlQuery.Trim();

        ValidateSql(trimmedSql);

        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var previewSql = $@"
            select *
            from (
                {trimmedSql.TrimEnd(';')}
            ) as preview_result
            limit @maxRows";

        await using var command = new NpgsqlCommand(previewSql, connection);
        command.Parameters.AddWithValue("maxRows", maxRows);

        await using var reader = await command.ExecuteReaderAsync();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }

    private static void ValidateSql(string sql)
    {
        if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only SELECT queries are allowed.");

        var blockedKeywords = new[]
        {
            "INSERT",
            "UPDATE",
            "DELETE",
            "DROP",
            "ALTER",
            "TRUNCATE",
            "CREATE",
            "GRANT",
            "REVOKE",
            "MERGE",
            "CALL",
            "EXEC"
        };

        foreach (var keyword in blockedKeywords)
        {
            if (sql.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Keyword '{keyword}' is not allowed.");
        }
    }

    private static string BuildConnectionString(DataSource dataSource)
    {
        return
            $"Host={dataSource.HostName};" +
            $"Port={dataSource.PortNumber};" +
            $"Database={dataSource.DatabaseName};" +
            $"Username={dataSource.ReadOnlyUserName};" +
            $"Password={dataSource.EncryptedPassword}";
    }
}
