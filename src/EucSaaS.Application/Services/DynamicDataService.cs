using EucSaaS.Domain.Entities;
using Npgsql;
using System.Data;

namespace EucSaaS.Application.Services;

public class DynamicDataService
{
public async Task<DataTable> GetTableDataAsync(
    DataSource dataSource,
    string schemaName,
    string tableName,
    Dictionary<string, string>? filters = null,
    string? defaultSortColumn = null,
    string? defaultSortDirection = null)
{
    var connectionString = BuildConnectionString(dataSource);

    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    var whereClauses = new List<string>();
    var parameters = new List<NpgsqlParameter>();

    var index = 0;

    if (filters != null)
    {
        foreach (var filter in filters)
        {
            var parameterName = $"p{index}";

            whereClauses.Add($@"""{filter.Key}""::text ilike @{parameterName}");
            parameters.Add(new NpgsqlParameter(parameterName, $"%{filter.Value}%"));

            index++;
        }
    }

    var whereSql = whereClauses.Count > 0
        ? " where " + string.Join(" and ", whereClauses)
        : "";

var orderBySql = "";

if (!string.IsNullOrWhiteSpace(defaultSortColumn))
{
    var direction =
        string.Equals(defaultSortDirection, "DESC",
            StringComparison.OrdinalIgnoreCase)
        ? "DESC"
        : "ASC";

    orderBySql =
        $@" order by ""{defaultSortColumn}"" {direction}";
}

var sql = $@"
    select *
    from ""{schemaName}"".""{tableName}""
    {whereSql}
    {orderBySql}
    limit 100";

    await using var command = new NpgsqlCommand(sql, connection);

    foreach (var parameter in parameters)
    {
        command.Parameters.Add(parameter);
    }

    await using var reader = await command.ExecuteReaderAsync();

    var table = new DataTable();
    table.Load(reader);

    return table;
}

    public async Task<Dictionary<string, object?>?> GetRecordAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        Guid id)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = $@"
            select *
            from ""{schemaName}"".""{tableName}""
            where ""{primaryKeyColumn}"" = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var row = new Dictionary<string, object?>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] =
                reader.IsDBNull(i)
                    ? null
                    : reader.GetValue(i);
        }

        return row;
    }

    public async Task UpdateRecordAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        Guid id,
        Dictionary<string, string?> values)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var setClauses = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        var index = 0;

        foreach (var item in values)
        {
            if (item.Key.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parameterName = $"p{index}";

            setClauses.Add($@"""{item.Key}"" = @{parameterName}");
            parameters.Add(new NpgsqlParameter(parameterName, item.Value ?? ""));

            index++;
        }

        var sql = $@"
            update ""{schemaName}"".""{tableName}""
            set {string.Join(", ", setClauses)}
            where ""{primaryKeyColumn}"" = @id";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertRecordAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        Dictionary<string, string?> values)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var columns = new List<string>();
        var parameterNames = new List<string>();
        var parameters = new List<NpgsqlParameter>();

var index = 0;

columns.Add(@"""Id""");
parameterNames.Add("@id");
parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));

columns.Add(@"""CreatedDate""");
parameterNames.Add("@createdDate");
parameters.Add(new NpgsqlParameter("createdDate", DateTime.UtcNow));

        foreach (var item in values)
        {
            columns.Add($@"""{item.Key}""");

            var parameterName = $"p{index}";
            parameterNames.Add($"@{parameterName}");

            parameters.Add(new NpgsqlParameter(parameterName, item.Value ?? ""));

            index++;
        }

        var sql = $@"
            insert into ""{schemaName}"".""{tableName}""
            ({string.Join(", ", columns)})
            values
            ({string.Join(", ", parameterNames)})";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();
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
