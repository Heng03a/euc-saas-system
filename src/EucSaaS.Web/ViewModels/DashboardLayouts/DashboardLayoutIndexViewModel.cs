using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.DashboardLayouts;

public class DashboardLayoutIndexViewModel
{
    public IReadOnlyList<DashboardLayoutListItemViewModel>
        Layouts { get; set; } =
            Array.Empty<DashboardLayoutListItemViewModel>();

    // ------------------------------------------------------------
    // Search and filters
    // ------------------------------------------------------------
    public string? SearchTerm { get; set; }

    public string? Ownership { get; set; }

    public string? Status { get; set; }

    public Guid? AppRoleId { get; set; }

    public Guid? DepartmentId { get; set; }

    // ------------------------------------------------------------
    // Sorting
    // ------------------------------------------------------------
    public string SortBy { get; set; } =
        "layoutname";

    public string SortDirection { get; set; } =
        "asc";

    // ------------------------------------------------------------
    // Paging
    // ------------------------------------------------------------
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages =>
        PageSize <= 0
            ? 1
            : (int)Math.Ceiling(
                TotalItems / (double)PageSize);

    public int StartItem =>
        TotalItems == 0
            ? 0
            : PageSize <= 0
                ? 1
                : ((Page - 1) * PageSize) + 1;

    public int EndItem =>
        TotalItems == 0
            ? 0
            : PageSize <= 0
                ? TotalItems
                : Math.Min(
                    Page * PageSize,
                    TotalItems);

    public bool HasPreviousPage =>
        Page > 1;

    public bool HasNextPage =>
        PageSize > 0 &&
        Page < TotalPages;

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(Ownership) ||
        !string.IsNullOrWhiteSpace(Status) ||
        AppRoleId.HasValue ||
        DepartmentId.HasValue;

    // ------------------------------------------------------------
    // Filter options
    // ------------------------------------------------------------
    public IReadOnlyList<SelectListItem>
        RoleOptions { get; set; } =
            Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem>
        DepartmentOptions { get; set; } =
            Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem>
        OwnershipOptions { get; set; } =
            new List<SelectListItem>
            {
                new()
                {
                    Value = string.Empty,
                    Text = "All ownership"
                },
                new()
                {
                    Value = "system",
                    Text = "System"
                },
                new()
                {
                    Value = "tenant",
                    Text = "Tenant"
                }
            };

    public IReadOnlyList<SelectListItem>
        StatusOptions { get; set; } =
            new List<SelectListItem>
            {
                new()
                {
                    Value = string.Empty,
                    Text = "All statuses"
                },
                new()
                {
                    Value = "active",
                    Text = "Active"
                },
                new()
                {
                    Value = "inactive",
                    Text = "Inactive"
                }
            };

    public IReadOnlyList<SelectListItem>
        SortOptions { get; set; } =
            new List<SelectListItem>
            {
                new()
                {
                    Value = "layoutname",
                    Text = "Layout name"
                },
                new()
                {
                    Value = "layoutcode",
                    Text = "Layout code"
                },
                new()
                {
                    Value = "displayorder",
                    Text = "Default first"
                },
                new()
                {
                    Value = "itemcount",
                    Text = "Widget count"
                },
                new()
                {
                    Value = "lastchanged",
                    Text = "Last changed"
                }
            };

    public IReadOnlyList<SelectListItem>
        SortDirectionOptions { get; set; } =
            new List<SelectListItem>
            {
                new()
                {
                    Value = "asc",
                    Text = "Ascending"
                },
                new()
                {
                    Value = "desc",
                    Text = "Descending"
                }
            };

    public IReadOnlyList<SelectListItem>
        PageSizeOptions { get; set; } =
            new List<SelectListItem>
            {
                new()
                {
                    Value = "6",
                    Text = "6 per page"
                },
                new()
                {
                    Value = "10",
                    Text = "10 per page"
                },
                new()
                {
                    Value = "20",
                    Text = "20 per page"
                },
                new()
                {
                    Value = "50",
                    Text = "50 per page"
                },
                new()
                {
                    Value = "0",
                    Text = "Show all"
                }
            };
}
