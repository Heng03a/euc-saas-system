using Npgsql;
using NpgsqlTypes;

namespace EucSaaS.Web.Services;

public class DashboardSqlBuilder
{
    public void AddFilterParameters(
        NpgsqlCommand command,
        string sql,
        string? department,
        string? status)
    {
        if (sql.Contains("@Department", StringComparison.OrdinalIgnoreCase))
        {
            command.Parameters.Add(
                "Department",
                NpgsqlDbType.Text
            ).Value = string.IsNullOrWhiteSpace(department)
                ? DBNull.Value
                : department;
        }

        if (sql.Contains("@Status", StringComparison.OrdinalIgnoreCase))
        {
            command.Parameters.Add(
                "Status",
                NpgsqlDbType.Text
            ).Value = string.IsNullOrWhiteSpace(status)
                ? DBNull.Value
                : status;
        }
    }
}
