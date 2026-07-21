using EucSaaS.Domain.Entities;
using Npgsql;
using System.Data;
using EucSaaS.Application.Interfaces;

namespace EucSaaS.Application.Services;

public class DynamicDataService
{
    private readonly ICurrentUserService _currentUserService;

    public DynamicDataService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

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
        var oldRow = await GetRecordAsync(
            dataSource,
            schemaName,
            tableName,
            primaryKeyColumn,
            id);

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

        if (setClauses.Count == 0)
            return;

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

        if (oldRow != null)
        {
            foreach (var item in values)
            {
                if (item.Key.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (item.Key.Equals("TenantId", StringComparison.OrdinalIgnoreCase))
                    continue;

                var oldValue = oldRow.ContainsKey(item.Key)
                    ? oldRow[item.Key]?.ToString() ?? ""
                    : "";

                var newValue = item.Value ?? "";

                if (oldValue != newValue)
                {
                    await InsertAuditLogAsync(
                        dataSource,
                        tableName,
                        id.ToString(),
                        "UPDATE",
                        item.Key,
                        oldValue,
                        newValue);
                }
            }
        }
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

        var newId = Guid.NewGuid();

        var columns = new List<string>();
        var parameterNames = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        var index = 0;

        columns.Add(@"""Id""");
        parameterNames.Add("@id");
        parameters.Add(new NpgsqlParameter("id", newId));

        columns.Add(@"""CreatedDate""");
        parameterNames.Add("@createdDate");
        parameters.Add(new NpgsqlParameter("createdDate", DateTime.UtcNow));

        if (IsTenantIsolatedTable(tableName))
        {
            columns.Add(@"""TenantId""");
            parameterNames.Add("@tenantId");

            if (_currentUserService.TenantId == Guid.Empty)
                throw new InvalidOperationException("TenantId was not found for current user.");

            parameters.Add(new NpgsqlParameter("tenantId", _currentUserService.TenantId));
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

        foreach (var item in values)
        {
            if (item.Key.Equals("TenantId", StringComparison.OrdinalIgnoreCase))
                continue;

            await InsertAuditLogAsync(
                dataSource,
                tableName,
                newId.ToString(),
                "CREATE",
                item.Key,
                "",
                item.Value ?? "");
        }
    }

    public async Task DeleteRecordAsync(
        DataSource dataSource,
        string schemaName,
        string tableName,
        string primaryKeyColumn,
        Guid id)
    {
        var oldRow = await GetRecordAsync(
            dataSource,
            schemaName,
            tableName,
            primaryKeyColumn,
            id);

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

        if (oldRow != null)
        {
            foreach (var item in oldRow)
            {
                await InsertAuditLogAsync(
                    dataSource,
                    tableName,
                    id.ToString(),
                    "DELETE",
                    item.Key,
                    item.Value?.ToString() ?? "",
                    "");
            }
        }
    }

private async Task InsertAuditLogAsync(
    DataSource dataSource,
    string resourceCode,
    string recordId,
    string actionType,
    string fieldName,
    string oldValue,
    string newValue)
{
    if (_currentUserService.TenantId == Guid.Empty)
    {
        throw new InvalidOperationException(
            "TenantId was not found for the current user.");
    }

    var changedBy =
        !string.IsNullOrWhiteSpace(_currentUserService.Username)
            ? _currentUserService.Username
            : _currentUserService.UserId.ToString();

    var connectionString = BuildConnectionString(dataSource);

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    const string sql = """
        insert into "AuditLogs"
        (
            "Id",
            "TenantId",
            "ScreenCode",
            "RecordId",
            "ActionType",
            "FieldName",
            "OldValue",
            "NewValue",
            "ChangedBy",
            "ChangedAt"
        )
        values
        (
            @id,
            @tenantId,
            @screenCode,
            @recordId,
            @actionType,
            @fieldName,
            @oldValue,
            @newValue,
            @changedBy,
            @changedAt
        )
        """;

    await using var command =
        new NpgsqlCommand(sql, connection);

    command.Parameters.AddWithValue(
        "id",
        Guid.NewGuid());

    command.Parameters.AddWithValue(
        "tenantId",
        _currentUserService.TenantId);

    command.Parameters.AddWithValue(
        "screenCode",
        resourceCode);

    command.Parameters.AddWithValue(
        "recordId",
        recordId);

    command.Parameters.AddWithValue(
        "actionType",
        actionType);

    command.Parameters.AddWithValue(
        "fieldName",
        fieldName);

    command.Parameters.AddWithValue(
        "oldValue",
        oldValue);

    command.Parameters.AddWithValue(
        "newValue",
        newValue);

    command.Parameters.AddWithValue(
        "changedBy",
        changedBy);

    command.Parameters.AddWithValue(
        "changedAt",
        DateTime.UtcNow);

    await command.ExecuteNonQueryAsync();
}

    private void AddTenantFilter(
        string tableName,
        List<string> whereClauses,
        List<NpgsqlParameter> parameters)
    {
        if (!IsTenantIsolatedTable(tableName))
            return;

        if (_currentUserService.TenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId was not found for current user.");

        whereClauses.Add(@"""TenantId"" = @tenantId");

        if (!parameters.Any(x => x.ParameterName == "tenantId"))
        {
            parameters.Add(new NpgsqlParameter("tenantId", _currentUserService.TenantId));
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
