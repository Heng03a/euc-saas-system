namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardDesignerFilterViewModel
{
    // Search and filtering
    public string? Search { get; set; }

    public string? WidgetType { get; set; }

    public bool? IsActive { get; set; }

    public Guid? DashboardTemplateId { get; set; }

    // Sorting
    public string SortBy { get; set; } = "displayOrder";

    public string SortDirection { get; set; } = "asc";

    // Pagination
    public int Page { get; set; } = 1;

    /// <summary>
    /// PageSize = 0 means show all records.
    /// </summary>
    public int PageSize { get; set; } = 12;
}
