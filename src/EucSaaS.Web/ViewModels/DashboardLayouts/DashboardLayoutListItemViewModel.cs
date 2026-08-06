namespace EucSaaS.Web.ViewModels.DashboardLayouts;

public class DashboardLayoutListItemViewModel
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? AppRoleId { get; set; }

    public Guid? DepartmentId { get; set; }

    public string LayoutCode { get; set; } =
        string.Empty;

    public string LayoutName { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    public string? RoleName { get; set; }

    public string? DepartmentName { get; set; }

    public bool IsSystem { get; set; }

    public bool IsDefault { get; set; }

    public bool IsShared { get; set; }

    public bool IsActive { get; set; }

    public int ItemCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } =
        string.Empty;

    public string? UpdatedBy { get; set; }

    public DateTime LastChangedAt =>
        UpdatedAt ?? CreatedAt;

    public string LastChangedBy =>
        !string.IsNullOrWhiteSpace(UpdatedBy)
            ? UpdatedBy
            : CreatedBy;

    public string OwnershipLabel =>
        IsSystem
            ? "System"
            : "Tenant";

    public string StatusLabel =>
        IsActive
            ? "Active"
            : "Inactive";

    public string DefaultLabel =>
        IsDefault
            ? "Default"
            : "Standard";

    public string SharingLabel =>
        IsShared
            ? "Shared"
            : "Restricted";
}
