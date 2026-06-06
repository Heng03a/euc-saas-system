using EucSaaS.Domain.Entities;
using Npgsql;
using System.Data;

namespace EucSaaS.Application.Services;

public class DynamicDataService
{
 public async Task<DataTable> GetTableDataAsync(
    DataSource dataSource,
    string schemaName,
    string tableName)
    {
        var connectionString =
            $"Host={dataSource.HostName};" +
            $"Port={dataSource.PortNumber};" +
            $"Database={dataSource.DatabaseName};" +
            $"Username={dataSource.ReadOnlyUserName};" +
            $"Password={dataSource.EncryptedPassword}";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

var sql = $"""
    select *
    from "{schemaName}"."{tableName}"
    limit 100
    """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }
}
