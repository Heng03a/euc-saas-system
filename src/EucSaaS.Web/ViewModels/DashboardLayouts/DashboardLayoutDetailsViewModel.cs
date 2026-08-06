namespace EucSaaS.Web.ViewModels.DashboardLayouts;

public class DashboardLayoutDetailsViewModel
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string LayoutCode { get; set; } =
        string.Empty;

    public string LayoutName { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    public Guid? AppRoleId { get; set; }

    public string? RoleName { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public bool IsSystem { get; set; }

    public bool IsDefault { get; set; }

    public bool IsShared { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } =
        string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public IReadOnlyList<DashboardLayoutItemViewModel>
        Items { get; set; } =
            Array.Empty<DashboardLayoutItemViewModel>();
}
