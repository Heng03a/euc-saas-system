namespace EucSaaS.Web.ViewModels.DashboardLayoutItems;

public class DashboardLayoutItemIndexViewModel
{
    // ------------------------------------------------------------
    // Layout identity
    // ------------------------------------------------------------
    public Guid DashboardLayoutId { get; set; }

    public Guid? TenantId { get; set; }

    public string LayoutCode { get; set; } =
        string.Empty;

    public string LayoutName { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    // ------------------------------------------------------------
    // Layout assignment
    // ------------------------------------------------------------
    public string? RoleName { get; set; }

    public string? DepartmentName { get; set; }

    // ------------------------------------------------------------
    // Layout properties
    // ------------------------------------------------------------
    public bool IsSystem { get; set; }

    public bool IsDefault { get; set; }

    public bool IsShared { get; set; }

    public bool IsActive { get; set; }

    // ------------------------------------------------------------
    // Items
    // ------------------------------------------------------------
    public IReadOnlyList<
        DashboardLayoutItemListItemViewModel>
        Items { get; set; } =
            Array.Empty<
                DashboardLayoutItemListItemViewModel>();

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    public int ItemCount =>
        Items.Count;

    public int VisibleItemCount =>
        Items.Count(
            x =>
                x.IsVisible);

    public int HiddenItemCount =>
        Items.Count(
            x =>
                !x.IsVisible);

    public bool HasItems =>
        Items.Count > 0;

    public bool CanManage =>
        !IsSystem;

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

    public string RoleLabel =>
        string.IsNullOrWhiteSpace(
            RoleName)
            ? "All roles"
            : RoleName;

    public string DepartmentLabel =>
        string.IsNullOrWhiteSpace(
            DepartmentName)
            ? "All departments"
            : DepartmentName;
}
