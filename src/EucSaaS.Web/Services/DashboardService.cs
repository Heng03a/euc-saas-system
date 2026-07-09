using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace EucSaaS.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _context;
    private readonly DashboardSqlBuilder _sqlBuilder;
    private readonly DashboardFilterService _filterService;

    public DashboardService(
        AppDbContext context,
        DashboardSqlBuilder sqlBuilder,
        DashboardFilterService filterService)
    {
        _context = context;
        _sqlBuilder = sqlBuilder;
        _filterService = filterService;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        Guid? appRoleId,
        string? department,
        string? status)
    {
        var model = new DashboardViewModel();

        model.Filter = await _filterService.GetFiltersAsync(
            department,
            status);

        var dashboardTemplateId = appRoleId.HasValue
            ? await _context.RoleDashboardTemplateAssignments
                .Where(x => x.AppRoleId == appRoleId.Value && x.IsActive)
                .Select(x => (Guid?)x.DashboardTemplateDefinitionId)
                .FirstOrDefaultAsync()
            : null;

        var widgetsQuery = _context.DashboardWidgetDefinitions
            .Where(x => x.IsActive);

        if (dashboardTemplateId.HasValue)
        {
            widgetsQuery = widgetsQuery
                .Where(x => x.DashboardTemplateDefinitionId == dashboardTemplateId.Value);
        }

        var widgets = await widgetsQuery
            .OrderBy(x => x.RowPosition)
            .ThenBy(x => x.ColumnPosition)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        foreach (var widget in widgets)
        {
            var vm = new DashboardWidgetViewModel
            {
                WidgetCode = widget.WidgetCode,
                WidgetTitle = widget.WidgetTitle,
                WidgetType = widget.WidgetType,
                DisplayOrder = widget.DisplayOrder,
                WidgetWidth = widget.WidgetWidth,
                RowPosition = widget.RowPosition,
                ColumnPosition = widget.ColumnPosition,
                Height = widget.Height,
                Icon = widget.Icon,
                Color = widget.Color
            };

            if (widget.WidgetType.Equals("Table", StringComparison.OrdinalIgnoreCase)
                || widget.WidgetType.Equals("Bar", StringComparison.OrdinalIgnoreCase)
                || widget.WidgetType.Equals("Pie", StringComparison.OrdinalIgnoreCase)
                || widget.WidgetType.Equals("Chart", StringComparison.OrdinalIgnoreCase)
                || widget.WidgetType.Equals("Line", StringComparison.OrdinalIgnoreCase))
            {
                var tableResult = await ExecuteTableAsync(
                    widget.SqlQuery,
                    department,
                    status);

                vm.Columns = tableResult.Columns;
                vm.Rows = tableResult.Rows;
            }
            else
            {
                vm.Value = await ExecuteScalarAsync(
                    widget.SqlQuery,
                    department,
                    status);
            }

            model.Widgets.Add(vm);
        }

        return model;
    }

    private async Task<string> ExecuteScalarAsync(
        string sql,
        string? department,
        string? status)
    {
        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        _sqlBuilder.AddFilterParameters(command, sql, department, status);

        var result = await command.ExecuteScalarAsync();

        return result?.ToString() ?? "0";
    }

    private async Task<(List<string> Columns, List<Dictionary<string, string>> Rows)> ExecuteTableAsync(
        string sql,
        string? department,
        string? status)
    {
        var columns = new List<string>();
        var rows = new List<Dictionary<string, string>>();

        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        _sqlBuilder.AddFilterParameters(command, sql, department, status);

        await using var reader = await command.ExecuteReaderAsync();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
        }

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, string>();

            foreach (var column in columns)
            {
                var value = reader[column];
                row[column] = value == DBNull.Value ? "" : value.ToString() ?? "";
            }

            rows.Add(row);
        }

        return (columns, rows);
    }
}
