using EucSaaS.Domain.Entities;
using Npgsql;
using System.Data;

namespace EucSaaS.Application.Services;

public class DynamicDataService
{
    private static readonly Guid CurrentTenantId =
        new("11111111-1111-1111-1111-111111111111");

    public async Task<DataTable> GetTableDataAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        Dictionary<string, string>? filters = null,
        string? defaultSortColumn = null,
        string? defaultSortDirection = null,
        int pageNumber = 1,
        int pageSize = 10,
        bool usePaging = true)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var whereClauses = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        AddTenantFilter(tableName, whereClauses, parameters);

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
                string.Equals(defaultSortDirection, "DESC", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : "ASC";

            orderBySql = $@" order by ""{defaultSortColumn}"" {direction}";
        }

        var pagingSql = "";

        if (usePaging)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var offset = (pageNumber - 1) * pageSize;
            pagingSql = " limit @pageSize offset @offset";

            parameters.Add(new NpgsqlParameter("pageSize", pageSize));
            parameters.Add(new NpgsqlParameter("offset", offset));
        }

        var sql = $@"
            select *
            from ""{schemaName}"".""{tableName}""
            {whereSql}
            {orderBySql}
            {pagingSql}";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }

    public async Task<int> GetTableCountAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        Dictionary<string, string>? filters = null)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var whereClauses = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        AddTenantFilter(tableName, whereClauses, parameters);

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

        var sql = $@"
            select count(*)
            from ""{schemaName}"".""{tableName}""
            {whereSql}";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
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

        var whereClauses = new List<string>
        {
            $@"""{primaryKeyColumn}"" = @id"
        };

        var parameters = new List<NpgsqlParameter>
        {
            new("id", id)
        };

        AddTenantFilter(tableName, whereClauses, parameters);

        var sql = $@"
            select *
            from ""{schemaName}"".""{tableName}""
            where {string.Join(" and ", whereClauses)}";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var row = new Dictionary<string, object?>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] =
                reader.IsDBNull(i) ? null : reader.GetValue(i);
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
                continue;

            if (item.Key.Equals("TenantId", StringComparison.OrdinalIgnoreCase))
                continue;

            var parameterName = $"p{index}";
            setClauses.Add($@"""{item.Key}"" = @{parameterName}");
            parameters.Add(new NpgsqlParameter(parameterName, item.Value ?? ""));
            index++;
        }

        var whereClauses = new List<string>
        {
            $@"""{primaryKeyColumn}"" = @id"
        };

        AddTenantFilter(tableName, whereClauses, parameters);

        var sql = $@"
            update ""{schemaName}"".""{tableName}""
            set {string.Join(", ", setClauses)}
            where {string.Join(" and ", whereClauses)}";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

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

        if (IsTenantIsolatedTable(tableName))
        {
            columns.Add(@"""TenantId""");
            parameterNames.Add("@tenantId");
            parameters.Add(new NpgsqlParameter("tenantId", CurrentTenantId));
        }

        foreach (var item in values)
        {
            if (item.Key.Equals("TenantId", StringComparison.OrdinalIgnoreCase))
                continue;

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
            command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteRecordAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        Guid id)
    {
        var connectionString = BuildConnectionString(dataSource);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var whereClauses = new List<string>
        {
            $@"""{primaryKeyColumn}"" = @id"
        };

        var parameters = new List<NpgsqlParameter>
        {
            new("id", id)
        };

        AddTenantFilter(tableName, whereClauses, parameters);

        var sql = $@"
            delete from ""{schemaName}"".""{tableName}""
            where {string.Join(" and ", whereClauses)}";

        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync();
    }

    private static void AddTenantFilter(
        string tableName,
        List<string> whereClauses,
        List<NpgsqlParameter> parameters)
    {
        if (!IsTenantIsolatedTable(tableName))
            return;

        whereClauses.Add(@"""TenantId"" = @tenantId");

        if (!parameters.Any(x => x.ParameterName == "tenantId"))
        {
            parameters.Add(new NpgsqlParameter("tenantId", CurrentTenantId));
        }
    }

private static bool IsTenantIsolatedTable(string tableName)
{
    var tenantTables = new[]
    {
        "Employees",
        "Departments"
    };

    return tenantTables.Contains(
        tableName,
        StringComparer.OrdinalIgnoreCase);
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
