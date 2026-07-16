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
        var model = new DashboardViewModel
        {
            Filter = await _filterService.GetFiltersAsync(
                department,
                status)
        };

        // -------------------------------------------------
        // Security: a valid application role is required.
        // No role means no dashboard widgets are returned.
        // -------------------------------------------------
        if (!appRoleId.HasValue || appRoleId.Value == Guid.Empty)
        {
            return model;
        }

        // -------------------------------------------------
        // Find the active dashboard template assigned
        // to the current user's role.
        // -------------------------------------------------
        var dashboardTemplateId =
            await _context.RoleDashboardTemplateAssignments
                .AsNoTracking()
                .Where(x =>
                    x.AppRoleId == appRoleId.Value &&
                    x.IsActive)
                .Select(x =>
                    (Guid?)x.DashboardTemplateDefinitionId)
                .FirstOrDefaultAsync();

        // -------------------------------------------------
        // Load only:
        // 1. Active widgets
        // 2. Widgets permitted for the current role
        // 3. Widgets belonging to the assigned template,
        //    when a template assignment exists
        // -------------------------------------------------
var permittedWidgetIds =
    _context.DashboardWidgetPermissions
        .AsNoTracking()
        .Where(permission =>
            permission.AppRoleId == appRoleId.Value &&
            permission.CanView)
        .Select(permission =>
            permission.DashboardWidgetDefinitionId);

var widgetsQuery =
    _context.DashboardWidgetDefinitions
        .AsNoTracking()
        .Where(widget =>
            widget.IsActive &&
            permittedWidgetIds.Contains(widget.Id));

        if (dashboardTemplateId.HasValue)
        {
            widgetsQuery = widgetsQuery.Where(widget =>
                widget.DashboardTemplateDefinitionId ==
                    dashboardTemplateId.Value);
        }

        var widgets = await widgetsQuery
            .OrderBy(x => x.RowPosition)
            .ThenBy(x => x.ColumnPosition)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        // -------------------------------------------------
        // Execute each permitted widget's configured SQL
        // -------------------------------------------------
        foreach (var widget in widgets)
        {
            var vm = new DashboardWidgetViewModel
            {
                Id = widget.Id,
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

            if (IsTableOrChartWidget(widget.WidgetType))
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

    private static bool IsTableOrChartWidget(string? widgetType)
    {
        if (string.IsNullOrWhiteSpace(widgetType))
        {
            return false;
        }

        return widgetType.Equals(
                   "Table",
                   StringComparison.OrdinalIgnoreCase)
               ||
               widgetType.Equals(
                   "Bar",
                   StringComparison.OrdinalIgnoreCase)
               ||
               widgetType.Equals(
                   "Pie",
                   StringComparison.OrdinalIgnoreCase)
               ||
               widgetType.Equals(
                   "Chart",
                   StringComparison.OrdinalIgnoreCase)
               ||
               widgetType.Equals(
                   "Line",
                   StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> ExecuteScalarAsync(
        string sql,
        string? department,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return "0";
        }

        var connection =
            (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command =
            new NpgsqlCommand(sql, connection);

        _sqlBuilder.AddFilterParameters(
            command,
            sql,
            department,
            status);

        var result = await command.ExecuteScalarAsync();

        return result?.ToString() ?? "0";
    }

    private async Task<(
        List<string> Columns,
        List<Dictionary<string, string>> Rows)>
        ExecuteTableAsync(
            string sql,
            string? department,
            string? status)
    {
        var columns = new List<string>();

        var rows =
            new List<Dictionary<string, string>>();

        if (string.IsNullOrWhiteSpace(sql))
        {
            return (columns, rows);
        }

        var connection =
            (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command =
            new NpgsqlCommand(sql, connection);

        _sqlBuilder.AddFilterParameters(
            command,
            sql,
            department,
            status);

        await using var reader =
            await command.ExecuteReaderAsync();

        for (var index = 0;
             index < reader.FieldCount;
             index++)
        {
            columns.Add(reader.GetName(index));
        }

        while (await reader.ReadAsync())
        {
            var row =
                new Dictionary<string, string>();

            foreach (var column in columns)
            {
                var value = reader[column];

                row[column] =
                    value == DBNull.Value
                        ? string.Empty
                        : value.ToString() ?? string.Empty;
            }

            rows.Add(row);
        }

        return (columns, rows);
    }
}
