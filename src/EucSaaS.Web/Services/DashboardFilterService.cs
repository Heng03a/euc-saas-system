using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Services.Security;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services;

public class DashboardFilterService
{
    private readonly AppDbContext _context;
    private readonly IDataAccessScopeResolver _scopeResolver;

    public DashboardFilterService(
        AppDbContext context,
        IDataAccessScopeResolver scopeResolver)
    {
        _context = context;
        _scopeResolver = scopeResolver;
    }

    public async Task<DashboardFilterViewModel> GetFiltersAsync(
        string? department,
        string? status)
    {
        // -------------------------------------------------
        // Obtain the authenticated user's mandatory
        // tenant and department security scope.
        // -------------------------------------------------
        var accessScope = _scopeResolver.Resolve();

        var effectiveDepartment =
            ResolveDepartment(
                department,
                accessScope);

        var effectiveStatus =
            NormalizeOptionalValue(status);

        var filter = new DashboardFilterViewModel
        {
            Department = effectiveDepartment,
            Status = effectiveStatus
        };

        // -------------------------------------------------
        // All filter data must remain inside the
        // authenticated user's tenant.
        // -------------------------------------------------
        var employeesQuery =
            _context.Employees
                .AsNoTracking()
                .Where(employee =>
                    employee.TenantId ==
                    accessScope.TenantId);

        // -------------------------------------------------
        // Admin:
        // Show every department in the current tenant.
        //
        // Manager/User/ReadOnly:
        // Show only the authenticated user's department.
        // -------------------------------------------------
        if (accessScope.CanAccessAllDepartments)
        {
            filter.Departments =
                await employeesQuery
                    .Where(employee =>
                        !string.IsNullOrWhiteSpace(
                            employee.Department))
                    .Select(employee =>
                        employee.Department)
                    .Distinct()
                    .OrderBy(departmentName =>
                        departmentName)
                    .ToListAsync();
        }
        else
        {
            filter.Departments =
                string.IsNullOrWhiteSpace(
                    effectiveDepartment)
                    ? new List<string>()
                    : new List<string>
                    {
                        effectiveDepartment
                    };
        }

        // -------------------------------------------------
        // Status options are also tenant-aware.
        //
        // When a department is selected or enforced,
        // only statuses found in that department are shown.
        // -------------------------------------------------
        if (!string.IsNullOrWhiteSpace(
                effectiveDepartment))
        {
            employeesQuery =
                employeesQuery.Where(employee =>
                    employee.Department ==
                    effectiveDepartment);
        }

        filter.Statuses =
            await employeesQuery
                .Where(employee =>
                    !string.IsNullOrWhiteSpace(
                        employee.Status))
                .Select(employee =>
                    employee.Status)
                .Distinct()
                .OrderBy(statusName =>
                    statusName)
                .ToListAsync();

        return filter;
    }

    private static string? ResolveDepartment(
        string? requestedDepartment,
        DataAccessScope accessScope)
    {
        // Restricted users cannot override their department
        // through URL parameters or browser form values.
        if (!accessScope.CanAccessAllDepartments)
        {
            return NormalizeDepartment(
                accessScope.Department);
        }

        // Admin may select any department, but the employee
        // query is still restricted to the current tenant.
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
}
