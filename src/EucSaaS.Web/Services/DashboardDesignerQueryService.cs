using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services;

public class DashboardDesignerQueryService
{
    private readonly AppDbContext _context;

    private static readonly int[] AllowedPageSizes =
    [
        0,
        6,
        12,
        24,
        48
    ];

    private static readonly string[] AllowedSortFields =
    [
        "displayorder",
        "code",
        "title",
        "type",
        "template",
        "status"
    ];

    public DashboardDesignerQueryService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDesignerIndexViewModel> GetIndexAsync(
        DashboardDesignerFilterViewModel? filter)
    {
        filter ??= new DashboardDesignerFilterViewModel();

        NormaliseFilter(filter);

        var query = _context.DashboardWidgetDefinitions
            .AsNoTracking()
            .Include(x => x.DashboardTemplateDefinition)
            .AsQueryable();

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search}%";

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

        // --------------------------------------------------------
        // Widget type filter
        // --------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(filter.WidgetType))
        {
            query = query.Where(x =>
                x.WidgetType == filter.WidgetType);
        }

        // --------------------------------------------------------
        // Status filter
        // --------------------------------------------------------
        if (filter.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == filter.IsActive.Value);
        }

        // --------------------------------------------------------
        // Dashboard template filter
        // --------------------------------------------------------
        if (filter.DashboardTemplateId.HasValue)
        {
            query = query.Where(x =>
                x.DashboardTemplateDefinitionId ==
                filter.DashboardTemplateId.Value);
        }

        // Count records before pagination.
        var totalItems = await query.CountAsync();

        // --------------------------------------------------------
        // Sorting
        // --------------------------------------------------------
        IQueryable<DashboardWidgetDefinition> resultQuery =
            ApplySorting(
                query,
                filter.SortBy,
                filter.SortDirection);

        // --------------------------------------------------------
        // Pagination
        // --------------------------------------------------------
        var totalPages = 1;

        if (filter.PageSize > 0)
        {
            totalPages =
                totalItems == 0
                    ? 1
                    : (int)Math.Ceiling(
                        totalItems /
                        (double)filter.PageSize);

            if (filter.Page > totalPages)
            {
                filter.Page = totalPages;
            }

            resultQuery = resultQuery
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(
                    filter.PageSize);
        }
        else
        {
            // PageSize = 0 means show all.
            filter.Page = 1;
            totalPages = 1;
        }

        var widgets = await resultQuery
            .Select(x => new DashboardWidgetDesignerViewModel
            {
                Id = x.Id,

                DashboardTemplateDefinitionId =
                    x.DashboardTemplateDefinitionId,

                DashboardTemplateName =
                    x.DashboardTemplateDefinition == null
                        ? string.Empty
                        : x.DashboardTemplateDefinition.TemplateName,

                WidgetCode =
                    x.WidgetCode,

                WidgetName =
                    x.WidgetTitle,

                WidgetType =
                    x.WidgetType,

                Description =
                    x.SqlQuery,

                DisplayOrder =
                    x.DisplayOrder,

                WidgetWidth =
                    x.WidgetWidth,

                RowPosition =
                    x.RowPosition,

                ColumnPosition =
                    x.ColumnPosition,

                Height =
                    x.Height,

                GridRow =
                    x.GridRow,

                GridColumn =
                    x.GridColumn,

                GridWidth =
                    x.GridWidth,

                GridHeight =
                    x.GridHeight,

                Icon =
                    x.Icon ?? string.Empty,

                Color =
                    x.Color ?? "primary",

                IsActive =
                    x.IsActive
            })
            .ToListAsync();

        var widgetTypes =
            await GetWidgetTypesAsync();

        var dashboardTemplates =
            await GetDashboardTemplatesAsync();

        var startItem =
            CalculateStartItem(
                totalItems,
                filter.Page,
                filter.PageSize);

        var endItem =
            CalculateEndItem(
                totalItems,
                filter.Page,
                filter.PageSize,
                widgets.Count);

        return new DashboardDesignerIndexViewModel
        {
            Filter =
                filter,

            Widgets =
                widgets,

            WidgetTypes =
                widgetTypes,

            DashboardTemplates =
                dashboardTemplates,

            SortOptions =
                GetSortOptions(),

            PageSizeOptions =
                GetPageSizeOptions(),

            TotalItems =
                totalItems,

            CurrentPage =
                filter.Page,

            PageSize =
                filter.PageSize,

            TotalPages =
                totalPages,

            StartItem =
                startItem,

            EndItem =
                endItem
        };
    }

    // ============================================================
    // Sorting
    // ============================================================
    private static IOrderedQueryable<DashboardWidgetDefinition>
        ApplySorting(
            IQueryable<DashboardWidgetDefinition> query,
            string sortBy,
            string sortDirection)
    {
        var descending =
            string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "code" =>
                descending
                    ? query
                        .OrderByDescending(x => x.WidgetCode)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x => x.WidgetCode)
                        .ThenBy(x => x.Id),

            "title" =>
                descending
                    ? query
                        .OrderByDescending(x => x.WidgetTitle)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id),

            "type" =>
                descending
                    ? query
                        .OrderByDescending(x => x.WidgetType)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x => x.WidgetType)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id),

            "template" =>
                descending
                    ? query
                        .OrderByDescending(x =>
                            x.DashboardTemplateDefinition == null
                                ? string.Empty
                                : x.DashboardTemplateDefinition.TemplateName)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x =>
                            x.DashboardTemplateDefinition == null
                                ? string.Empty
                                : x.DashboardTemplateDefinition.TemplateName)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id),

            "status" =>
                descending
                    ? query
                        .OrderByDescending(x => x.IsActive)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x => x.IsActive)
                        .ThenBy(x => x.WidgetTitle)
                        .ThenBy(x => x.Id),

            _ =>
                descending
                    ? query
                        .OrderByDescending(x => x.DisplayOrder)
                        .ThenByDescending(x => x.RowPosition)
                        .ThenByDescending(x => x.ColumnPosition)
                        .ThenBy(x => x.Id)
                    : query
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.RowPosition)
                        .ThenBy(x => x.ColumnPosition)
                        .ThenBy(x => x.Id)
        };
    }

    // ============================================================
    // Filter normalisation
    // ============================================================
    private static void NormaliseFilter(
        DashboardDesignerFilterViewModel filter)
    {
        filter.Search =
            string.IsNullOrWhiteSpace(filter.Search)
                ? null
                : filter.Search.Trim();

        filter.WidgetType =
            string.IsNullOrWhiteSpace(filter.WidgetType)
                ? null
                : filter.WidgetType.Trim();

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        if (!AllowedPageSizes.Contains(filter.PageSize))
        {
            filter.PageSize = 12;
        }

        filter.SortBy =
            string.IsNullOrWhiteSpace(filter.SortBy)
                ? "displayorder"
                : filter.SortBy.Trim().ToLowerInvariant();

        if (!AllowedSortFields.Contains(filter.SortBy))
        {
            filter.SortBy = "displayorder";
        }

        filter.SortDirection =
            string.Equals(
                filter.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";
    }

    // ============================================================
    // Dropdown values
    // ============================================================
    private async Task<List<SelectListItem>>
        GetWidgetTypesAsync()
    {
        return await _context.DashboardWidgetDefinitions
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
    }

    private async Task<List<SelectListItem>>
        GetDashboardTemplatesAsync()
    {
        return await _context.DashboardTemplateDefinitions
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new SelectListItem
            {
                Text = x.TemplateName,
                Value = x.Id.ToString()
            })
            .ToListAsync();
    }

    private static List<SelectListItem>
        GetSortOptions()
    {
        return
        [
            new SelectListItem
            {
                Text = "Display order",
                Value = "displayorder"
            },

            new SelectListItem
            {
                Text = "Widget code",
                Value = "code"
            },

            new SelectListItem
            {
                Text = "Widget title",
                Value = "title"
            },

            new SelectListItem
            {
                Text = "Widget type",
                Value = "type"
            },

            new SelectListItem
            {
                Text = "Template",
                Value = "template"
            },

            new SelectListItem
            {
                Text = "Status",
                Value = "status"
            }
        ];
    }

    private static List<SelectListItem>
        GetPageSizeOptions()
    {
        return
        [
            new SelectListItem
            {
                Text = "6 per page",
                Value = "6"
            },

            new SelectListItem
            {
                Text = "12 per page",
                Value = "12"
            },

            new SelectListItem
            {
                Text = "24 per page",
                Value = "24"
            },

            new SelectListItem
            {
                Text = "48 per page",
                Value = "48"
            },

            new SelectListItem
            {
                Text = "Show all",
                Value = "0"
            }
        ];
    }

    // ============================================================
    // Pagination calculations
    // ============================================================
    private static int CalculateStartItem(
        int totalItems,
        int currentPage,
        int pageSize)
    {
        if (totalItems == 0)
        {
            return 0;
        }

        if (pageSize == 0)
        {
            return 1;
        }

        return
            ((currentPage - 1) * pageSize) + 1;
    }

    private static int CalculateEndItem(
        int totalItems,
        int currentPage,
        int pageSize,
        int returnedItems)
    {
        if (totalItems == 0)
        {
            return 0;
        }

        if (pageSize == 0)
        {
            return totalItems;
        }

        var calculatedEnd =
            ((currentPage - 1) * pageSize) +
            returnedItems;

        return Math.Min(
            calculatedEnd,
            totalItems);
    }
}
