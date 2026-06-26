using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EucSaaS.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var model = new DashboardViewModel();

        var widgets = await _context.DashboardWidgetDefinitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        foreach (var widget in widgets)
        {
            var value = await ExecuteScalarAsync(widget.SqlQuery);

            model.Widgets.Add(new DashboardWidgetViewModel
            {
                WidgetCode = widget.WidgetCode,
                WidgetTitle = widget.WidgetTitle,
                WidgetType = widget.WidgetType,
                Value = value,
                DisplayOrder = widget.DisplayOrder
            });
        }

        return model;
    }

    private async Task<string> ExecuteScalarAsync(string sql)
    {
        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();

        return result?.ToString() ?? "0";
    }
}
