using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services;

public class DashboardDesignerQueryService
{
    private readonly AppDbContext _context;

    public DashboardDesignerQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDesignerIndexViewModel> GetIndexAsync(
        DashboardDesignerFilterViewModel? filter)
    {
        filter ??= new DashboardDesignerFilterViewModel();

        var query = _context.DashboardWidgetDefinitions
            .AsNoTracking()
            .Include(x => x.DashboardTemplateDefinition)
            .AsQueryable();

if (!string.IsNullOrWhiteSpace(filter.Search))
{
    var search = filter.Search.Trim();
    var pattern = $"%{search}%";

    query = query.Where(x =>
        EF.Functions.ILike(
            x.WidgetCode,
            pattern) ||
        EF.Functions.ILike(
            x.WidgetTitle,
            pattern) ||
        EF.Functions.ILike(
            x.WidgetType,
            pattern) ||
        (
            x.DashboardTemplateDefinition != null &&
            EF.Functions.ILike(
                x.DashboardTemplateDefinition.TemplateName,
                pattern)
        ));
}
        if (!string.IsNullOrWhiteSpace(filter.WidgetType))
        {
            query = query.Where(x =>
                x.WidgetType == filter.WidgetType);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == filter.IsActive.Value);
        }

        if (filter.DashboardTemplateId.HasValue)
        {
            query = query.Where(x =>
                x.DashboardTemplateDefinitionId ==
                filter.DashboardTemplateId.Value);
        }

        var widgets = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.WidgetTitle)
            .Select(x => new DashboardWidgetDesignerViewModel
            {
                Id = x.Id,

                DashboardTemplateDefinitionId =
                    x.DashboardTemplateDefinitionId,

                DashboardTemplateName =
                    x.DashboardTemplateDefinition == null
                        ? string.Empty
                        : x.DashboardTemplateDefinition.TemplateName,

                WidgetCode = x.WidgetCode,

                WidgetName = x.WidgetTitle,

                WidgetType = x.WidgetType,

                Description = x.SqlQuery,

                DisplayOrder = x.DisplayOrder,

                WidgetWidth = x.WidgetWidth,

                RowPosition = x.RowPosition,

                ColumnPosition = x.ColumnPosition,

                Height = x.Height,

                GridRow = x.GridRow,

                GridColumn = x.GridColumn,

                GridWidth = x.GridWidth,

                GridHeight = x.GridHeight,

                Icon = x.Icon ?? string.Empty,

                Color = x.Color ?? "primary",

                IsActive = x.IsActive
            })
            .ToListAsync();

        var widgetTypes = await _context
            .DashboardWidgetDefinitions
            .AsNoTracking()
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.WidgetType))
            .Select(x => x.WidgetType)
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            })
            .ToListAsync();

        var dashboardTemplates = await _context
            .DashboardTemplateDefinitions
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new SelectListItem
            {
                Text = x.TemplateName,
                Value = x.Id.ToString()
            })
            .ToListAsync();

        return new DashboardDesignerIndexViewModel
        {
            Filter = filter,
            Widgets = widgets,
            WidgetTypes = widgetTypes,
            DashboardTemplates = dashboardTemplates
        };
    }
}
