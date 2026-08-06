using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.DashboardLayouts;

public class DashboardLayoutEditViewModel
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    [Display(Name = "Role")]
    public Guid? AppRoleId { get; set; }

    [Display(Name = "Department")]
    public Guid? DepartmentId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Layout code")]
    public string LayoutCode { get; set; } =
        string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Layout name")]
    public string LayoutName { get; set; } =
        string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "System layout")]
    public bool IsSystem { get; set; }

    [Display(Name = "Default layout")]
    public bool IsDefault { get; set; }

    [Display(Name = "Shared layout")]
    public bool IsShared { get; set; } = true;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int ItemCount { get; set; }

    public bool IsEditMode =>
        Id != Guid.Empty;

    public IReadOnlyList<SelectListItem>
        RoleOptions { get; set; } =
            Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem>
        DepartmentOptions { get; set; } =
            Array.Empty<SelectListItem>();
}
