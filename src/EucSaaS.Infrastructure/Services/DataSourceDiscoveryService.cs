using EucSaaS.Application.Interfaces;
using Npgsql;

namespace EucSaaS.Infrastructure.Services;

public class DataSourceDiscoveryService : IDataSourceDiscoveryService
{
public async Task<List<string>> DiscoverTablesAsync(
    string provider,
    string connectionString)
{
    if (provider != "PostgreSQL" &&
        provider != "Postgres")
    {
        throw new NotSupportedException(
            $"Provider '{provider}' is not supported yet.");
    }

        var tables = new List<string>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            select table_schema || '.' || table_name as table_name
            from information_schema.tables
            where table_type = 'BASE TABLE'
              and table_schema not in ('pg_catalog', 'information_schema')
            order by table_schema, table_name;
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
