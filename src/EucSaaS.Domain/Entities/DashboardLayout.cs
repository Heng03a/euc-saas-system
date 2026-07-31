namespace EucSaaS.Domain.Entities;

public class DashboardLayout
{
    public Guid Id { get; set; }

    /// <summary>
    /// Null represents a system-level layout.
    /// A value represents a tenant-owned layout.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Optional role-specific assignment.
    /// Null means the layout is not restricted to one role.
    /// </summary>
    public Guid? AppRoleId { get; set; }

    /// <summary>
    /// Optional department-specific assignment.
    /// Null means the layout is not restricted to one department.
    /// </summary>
    public Guid? DepartmentId { get; set; }

    public string LayoutCode { get; set; } = string.Empty;

    public string LayoutName { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// System layouts are protected from tenant deletion.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Identifies the default layout within its assignment scope.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Shared layouts may be selected by authorised users.
    /// </summary>
    public bool IsShared { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<DashboardLayoutItem> Items { get; set; }
        = new List<DashboardLayoutItem>();
}