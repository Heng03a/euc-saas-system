namespace EucSaaS.Web.ViewModels.DashboardWidgetTemplates;

public class DashboardWidgetTemplateIndexViewModel
{
    // ------------------------------------------------------------
    // Search and filters
    // ------------------------------------------------------------

    public string? Search { get; set; }

    public string? Category { get; set; }

    public string? WidgetType { get; set; }

    public bool? IsActive { get; set; }

    // Existing service properties retained for compatibility
    public string? Status { get; set; }

    public string? Ownership { get; set; }

    // ------------------------------------------------------------
    // Sorting
    // ------------------------------------------------------------

    public string SortBy { get; set; } = "templatecode";

    public string SortDirection { get; set; } = "asc";

    // ------------------------------------------------------------
    // Paging
    // ------------------------------------------------------------

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    private int _totalRecords;

    public int TotalRecords
    {
        get => _totalRecords;
        set => _totalRecords = value;
    }

    // Alias used by the new Index.cshtml
    public int TotalCount
    {
        get => _totalRecords;
        set => _totalRecords = value;
    }

    public int TotalPages =>
        PageSize <= 0
            ? 1
            : (int)Math.Ceiling(
                TotalCount / (double)PageSize);

    // ------------------------------------------------------------
    // Filter options
    // ------------------------------------------------------------

    public List<string> Categories { get; set; } = [];

    public List<string> WidgetTypes { get; set; } = [];

    // ------------------------------------------------------------
    // Results
    // ------------------------------------------------------------

    private List<DashboardWidgetTemplateListItemViewModel> _templates = [];

    // Existing service property
    public List<DashboardWidgetTemplateListItemViewModel> Templates
    {
        get => _templates;
        set => _templates = value ?? [];
    }

    // Alias used by the new Index.cshtml
    public List<DashboardWidgetTemplateListItemViewModel> Items
    {
        get => _templates;
        set => _templates = value ?? [];
    }
}
