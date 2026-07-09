using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services;

public class DashboardFilterService
{
    private readonly AppDbContext _context;

    public DashboardFilterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardFilterViewModel> GetFiltersAsync(
        string? department,
        string? status)
    {
        var filter = new DashboardFilterViewModel
        {
            Department = department,
            Status = status
        };

        filter.Departments = await _context.Employees
            .Where(x => !string.IsNullOrWhiteSpace(x.Department))
            .Select(x => x.Department)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        filter.Statuses = await _context.Employees
            .Where(x => !string.IsNullOrWhiteSpace(x.Status))
            .Select(x => x.Status)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return filter;
    }
}
