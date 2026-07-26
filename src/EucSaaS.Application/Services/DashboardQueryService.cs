using EucSaaS.Domain.Entities;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace EucSaaS.Application.Services;

public class DashboardQueryService
{
public async Task<DataTable> TestSqlAsync(
    DataSource dataSource,
    string sqlQuery,
    Guid tenantId,
    int maxRows = 20)
    {
        if (dataSource == null)
        {
            throw new ArgumentNullException(
                nameof(dataSource));
        }

        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            throw new InvalidOperationException(
                "SQL query is required.");
        }

        if (maxRows < 1)
        {
            maxRows = 20;
        }

        var trimmedSql =
            sqlQuery.Trim();

        ValidateSql(trimmedSql);

if (!trimmedSql.Contains(
        "@TenantId",
        StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Every dashboard SQL query must contain the mandatory @TenantId parameter.");
}

        var connectionString =
            BuildConnectionString(dataSource);

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        var previewSql = $"""
            SELECT *
            FROM
            (
                {trimmedSql.TrimEnd(';')}
            ) AS preview_result
            LIMIT @maxRows;
            """;

        await using var command =
            new NpgsqlCommand(
                previewSql,
                connection);

AddPreviewParameters(
    command,
    trimmedSql,
    tenantId);

        command.Parameters.AddWithValue(
            "maxRows",
            NpgsqlDbType.Integer,
            maxRows);

        await using var reader =
            await command.ExecuteReaderAsync();

        var table =
            new DataTable();

        table.Load(reader);

        return table;
    }

    // ============================================================
    // Dashboard preview parameters
    // ============================================================
private static void AddPreviewParameters(
    NpgsqlCommand command,
    string sql,
    Guid tenantId)
{
    if (tenantId == Guid.Empty)
    {
        throw new InvalidOperationException(
            "A valid tenant ID is required to test dashboard SQL.");
    }

    if (ContainsParameter(
            sql,
            "@TenantId"))
    {
        command.Parameters.AddWithValue(
            "TenantId",
            NpgsqlDbType.Uuid,
            tenantId);
    }

    if (ContainsParameter(
            sql,
            "@Department"))
    {
        command.Parameters.Add(
            new NpgsqlParameter(
                "Department",
                NpgsqlDbType.Text)
            {
                Value = DBNull.Value
            });
    }

    if (ContainsParameter(
            sql,
                "@Status"))
    {
        command.Parameters.Add(
            new NpgsqlParameter(
                "Status",
                NpgsqlDbType.Text)
            {
                Value = DBNull.Value
            });
    }
}

    private static bool ContainsParameter(
        string sql,
        string parameterName)
    {
        return sql.Contains(
            parameterName,
            StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // SQL security validation
    // ============================================================
    private static void ValidateSql(
        string sql)
    {
        if (!sql.StartsWith(
                "SELECT",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only SELECT queries are allowed.");
        }

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
            if (ContainsSqlKeyword(
                    sql,
                    keyword))
            {
                throw new InvalidOperationException(
                    $"Keyword '{keyword}' is not allowed.");
            }
        }
    }

    private static bool ContainsSqlKeyword(
        string sql,
        string keyword)
    {
        var separators = new[]
        {
            ' ',
            '\t',
            '\r',
            '\n',
            '(',
            ')',
            ';',
            ','
        };

        return sql
            .Split(
                separators,
                StringSplitOptions.RemoveEmptyEntries)
            .Any(token =>
                string.Equals(
                    token,
                    keyword,
                    StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Connection
    // ============================================================
    private static string BuildConnectionString(
        DataSource dataSource)
    {
        var builder =
            new NpgsqlConnectionStringBuilder
            {
                Host =
                    dataSource.HostName,

                Port =
                    dataSource.PortNumber,

                Database =
                    dataSource.DatabaseName,

                Username =
                    dataSource.ReadOnlyUserName,

                Password =
                    dataSource.EncryptedPassword
            };

        return builder.ConnectionString;
    }
}
