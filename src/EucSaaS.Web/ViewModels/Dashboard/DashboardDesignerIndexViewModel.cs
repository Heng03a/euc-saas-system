using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardDesignerIndexViewModel
{
    public DashboardDesignerFilterViewModel Filter { get; set; } = new();

    public List<DashboardWidgetDesignerViewModel> Widgets { get; set; } = [];

    public List<SelectListItem> WidgetTypes { get; set; } = [];

    public List<SelectListItem> DashboardTemplates { get; set; } = [];

    public List<SelectListItem> SortOptions { get; set; } = [];

    public List<SelectListItem> PageSizeOptions { get; set; } = [];

    // Pagination summary
    public int TotalItems { get; set; }

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 12;

    public int TotalPages { get; set; }

    public int StartItem { get; set; }

    public int EndItem { get; set; }

    public bool HasPreviousPage =>
        CurrentPage > 1;

    public bool HasNextPage =>
        CurrentPage < TotalPages;

    public bool IsShowingAll =>
        PageSize == 0;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Filter.Search) ||
        !string.IsNullOrWhiteSpace(Filter.WidgetType) ||
        Filter.IsActive.HasValue ||
        Filter.DashboardTemplateId.HasValue;

    /// <summary>
    /// Drag-and-drop layout editing is safe only when:
    /// 1. No filters are active.
    /// 2. All widgets are displayed on the page.
    /// </summary>
    public bool CanEditLayout =>
        !HasActiveFilters &&
        IsShowingAll &&
        TotalItems > 0;
}
