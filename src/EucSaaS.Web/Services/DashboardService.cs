using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Services.Security;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Security;

namespace EucSaaS.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _context;
    private readonly DashboardSqlBuilder _sqlBuilder;
    private readonly DashboardFilterService _filterService;
    private readonly IDataAccessScopeResolver _scopeResolver;

    public DashboardService(
        AppDbContext context,
        DashboardSqlBuilder sqlBuilder,
        DashboardFilterService filterService,
        IDataAccessScopeResolver scopeResolver)
    {
        _context = context;
        _sqlBuilder = sqlBuilder;
        _filterService = filterService;
        _scopeResolver = scopeResolver;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        Guid? appRoleId,
        string? department,
        string? status)
    {
        // -------------------------------------------------
        // Resolve security information from authenticated
        // user claims. Browser values are not trusted.
        // -------------------------------------------------
        var accessScope = _scopeResolver.Resolve();

        // -------------------------------------------------
        // Security: the role supplied by the controller must
        // match the authenticated user's AppRoleId claim.
        // -------------------------------------------------
        var effectiveRoleId = ResolveRoleId(
            appRoleId,
            accessScope);

        // -------------------------------------------------
        // Admin:
        // May select a department within the same tenant.
        //
        // Other roles:
        // Department is forced to the authenticated user's
        // own department regardless of query-string values.
        // -------------------------------------------------
        var effectiveDepartment = ResolveDepartment(
            department,
            accessScope);

        var effectiveStatus = NormalizeOptionalValue(status);

        var model = new DashboardViewModel
        {
            Filter = await _filterService.GetFiltersAsync(
                effectiveDepartment,
                effectiveStatus)
        };

        // Ensure the UI reflects the enforced security scope.
        model.Filter.Department = effectiveDepartment;
        model.Filter.Status = effectiveStatus;

        // -------------------------------------------------
        // Find the active dashboard template assigned
        // to the authenticated user's role.
        // -------------------------------------------------
        var dashboardTemplateId =
            await _context.RoleDashboardTemplateAssignments
                .AsNoTracking()
                .Where(x =>
                    x.AppRoleId == effectiveRoleId &&
                    x.IsActive)
                .Select(x =>
                    (Guid?)x.DashboardTemplateDefinitionId)
                .FirstOrDefaultAsync();

        // -------------------------------------------------
        // Find widget IDs that the authenticated role
        // has permission to view.
        // -------------------------------------------------
        var permittedWidgetIds =
            _context.DashboardWidgetPermissions
                .AsNoTracking()
                .Where(permission =>
                    permission.AppRoleId == effectiveRoleId &&
                    permission.CanView)
                .Select(permission =>
                    permission.DashboardWidgetDefinitionId);

        // -------------------------------------------------
        // Load only:
        // 1. Active widgets
        // 2. Widgets permitted for the current role
        // 3. Widgets belonging to the assigned template,
        //    when a template assignment exists
        // -------------------------------------------------
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
        // using mandatory server-side security parameters.
        // -------------------------------------------------
        foreach (var widget in widgets)
        {
            ValidateWidgetSqlSecurity(
                widget.WidgetCode,
                widget.SqlQuery,
                accessScope);

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
                    accessScope.TenantId,
                    effectiveDepartment,
                    effectiveStatus);

                vm.Columns = tableResult.Columns;
                vm.Rows = tableResult.Rows;
            }
            else
            {
                vm.Value = await ExecuteScalarAsync(
                    widget.SqlQuery,
                    accessScope.TenantId,
                    effectiveDepartment,
                    effectiveStatus);
            }

            model.Widgets.Add(vm);
        }

        return model;
    }

    private static Guid ResolveRoleId(
        Guid? requestedRoleId,
        DataAccessScope accessScope)
    {
        if (!accessScope.AppRoleId.HasValue ||
            accessScope.AppRoleId.Value == Guid.Empty)
        {
            throw new SecurityException(
                "The authenticated user does not have a valid AppRoleId claim.");
        }

        var authenticatedRoleId =
            accessScope.AppRoleId.Value;

        if (requestedRoleId.HasValue &&
            requestedRoleId.Value != Guid.Empty &&
            requestedRoleId.Value != authenticatedRoleId)
        {
            throw new SecurityException(
                "The requested application role does not match the authenticated user's role.");
        }

        return authenticatedRoleId;
    }

    private static string? ResolveDepartment(
        string? requestedDepartment,
        DataAccessScope accessScope)
    {
        if (!accessScope.CanAccessAllDepartments)
        {
            if (string.IsNullOrWhiteSpace(
                    accessScope.Department))
            {
                throw new SecurityException(
                    "The authenticated user does not have a valid department claim.");
            }

            return NormalizeDepartment(
                accessScope.Department);
        }

        return NormalizeDepartment(
            requestedDepartment);
    }

    private static string? NormalizeDepartment(
        string? department)
    {
        return string.IsNullOrWhiteSpace(department)
            ? null
            : department.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static void ValidateWidgetSqlSecurity(
        string? widgetCode,
        string? sql,
        DataAccessScope accessScope)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return;
        }

        // Every dashboard SQL statement must be tenant-aware.
        if (!ContainsParameter(sql, "@TenantId"))
        {
            throw new SecurityException(
                $"Dashboard widget '{widgetCode}' does not contain the mandatory @TenantId security parameter.");
        }

        // Department-restricted users must never execute SQL
        // that ignores their department.
        if (accessScope.IsDepartmentRestricted &&
            !ContainsParameter(sql, "@Department"))
        {
            throw new SecurityException(
                $"Dashboard widget '{widgetCode}' does not contain the mandatory @Department security parameter.");
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

    private static bool IsTableOrChartWidget(
        string? widgetType)
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
        Guid tenantId,
        string? department,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return "0";
        }

        var connection =
            (NpgsqlConnection)_context.Database
                .GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command =
            new NpgsqlCommand(sql, connection);

        AddQueryParameters(
            command,
            sql,
            tenantId,
            department,
            status);

        var result =
            await command.ExecuteScalarAsync();

        return result?.ToString() ?? "0";
    }

    private async Task<(
        List<string> Columns,
        List<Dictionary<string, string>> Rows)>
        ExecuteTableAsync(
            string sql,
            Guid tenantId,
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
            (NpgsqlConnection)_context.Database
                .GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command =
            new NpgsqlCommand(sql, connection);

        AddQueryParameters(
            command,
            sql,
            tenantId,
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
                        : value.ToString() ??
                          string.Empty;
            }

            rows.Add(row);
        }

        return (columns, rows);
    }

    private void AddQueryParameters(
        NpgsqlCommand command,
        string sql,
        Guid tenantId,
        string? department,
        string? status)
    {
        // Existing Department and Status parameters.
        _sqlBuilder.AddFilterParameters(
            command,
            sql,
            department,
            status);

        // Mandatory TenantId parameter.
        if (ContainsParameter(sql, "@TenantId") &&
            !command.Parameters.Contains("@TenantId"))
        {
            command.Parameters.AddWithValue(
                "@TenantId",
                tenantId);
        }
    }
}
