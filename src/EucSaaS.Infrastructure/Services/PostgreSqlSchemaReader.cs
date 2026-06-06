using EucSaaS.Application.DTOs;
using EucSaaS.Application.Interfaces;
using EucSaaS.Domain.Entities;
using Npgsql;

namespace EucSaaS.Infrastructure.Services;

public class PostgreSqlSchemaReader : IDataSourceSchemaReader
{
    public async Task<List<DatabaseTableDto>> ReadSchemaAsync(DataSource dataSource)
    {
        if (dataSource.DatabaseType != "PostgreSQL")
        {
            throw new NotSupportedException("Only PostgreSQL schema discovery is supported at this stage.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = dataSource.HostName,
            Port = dataSource.PortNumber,
            Database = dataSource.DatabaseName,
            Username = dataSource.ReadOnlyUserName,
            Password = dataSource.EncryptedPassword
        };

        var tables = new List<DatabaseTableDto>();

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var tableSql = """
            select table_schema, table_name
            from information_schema.tables
            where table_type = 'BASE TABLE'
              and table_schema not in ('pg_catalog', 'information_schema')
            order by table_schema, table_name;
            """;

        await using (var tableCommand = new NpgsqlCommand(tableSql, connection))
        await using (var reader = await tableCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(new DatabaseTableDto
                {
                    SchemaName = reader.GetString(0),
                    TableName = reader.GetString(1)
                });
            }
        }

        foreach (var table in tables)
        {
            var columnSql = """
                select column_name, data_type, is_nullable, ordinal_position
                from information_schema.columns
                where table_schema = @schemaName
                  and table_name = @tableName
                order by ordinal_position;
                """;

            await using var columnCommand = new NpgsqlCommand(columnSql, connection);
            columnCommand.Parameters.AddWithValue("@schemaName", table.SchemaName);
            columnCommand.Parameters.AddWithValue("@tableName", table.TableName);

            await using var columnReader = await columnCommand.ExecuteReaderAsync();

            while (await columnReader.ReadAsync())
            {
                table.Columns.Add(new DatabaseColumnDto
                {
                    ColumnName = columnReader.GetString(0),
                    DataType = columnReader.GetString(1),
                    IsNullable = columnReader.GetString(2) == "YES",
                    OrdinalPosition = columnReader.GetInt32(3)
                });
            }
        }

        return tables;
    }
}
