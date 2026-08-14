using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.DashboardLayoutItems;

public class DashboardLayoutItemEditViewModel
{
    public Guid Id { get; set; }

    // ------------------------------------------------------------
    // Parent layout
    // ------------------------------------------------------------
    [Required]
    public Guid DashboardLayoutId { get; set; }

    public string LayoutCode { get; set; } =
        string.Empty;

    public string LayoutName { get; set; } =
        string.Empty;

    public bool IsSystemLayout { get; set; }

    // ------------------------------------------------------------
    // Widget
    // ------------------------------------------------------------
    [Required(
        ErrorMessage =
            "Dashboard Widget is required.")]
    [Display(Name = "Dashboard Widget")]
    public Guid DashboardWidgetDefinitionId { get; set; }

    public string WidgetCode { get; set; } =
        string.Empty;

    public string WidgetName { get; set; } =
        string.Empty;

    public string WidgetType { get; set; } =
        string.Empty;

    // ------------------------------------------------------------
    // Grid
    // ------------------------------------------------------------
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Grid Row must be greater than or equal to 1.")]
    [Display(Name = "Grid Row")]
    public int GridRow { get; set; } = 1;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Grid Column must be greater than or equal to 1.")]
    [Display(Name = "Grid Column")]
    public int GridColumn { get; set; } = 1;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Grid Width must be greater than or equal to 1.")]
    [Display(Name = "Grid Width")]
    public int GridWidth { get; set; } = 4;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Grid Height must be greater than or equal to 1.")]
    [Display(Name = "Grid Height")]
    public int GridHeight { get; set; } = 2;

    // ------------------------------------------------------------
    // Display
    // ------------------------------------------------------------
    [Range(
        0,
        int.MaxValue,
        ErrorMessage =
            "Display Order cannot be negative.")]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Visible")]
    public bool IsVisible { get; set; } = true;

    // ------------------------------------------------------------
    // Future widget configuration
    // ------------------------------------------------------------
    [Display(Name = "Settings JSON")]
    public string? SettingsJson { get; set; }

    // ------------------------------------------------------------
    // Dropdown
    // ------------------------------------------------------------
    public IReadOnlyList<SelectListItem>
        WidgetOptions { get; set; } =
            Array.Empty<SelectListItem>();

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    public bool IsCreate =>
        Id == Guid.Empty;

    public string PageTitle =>
        IsCreate
            ? "Add Dashboard Widget"
            : "Edit Dashboard Widget";
}
