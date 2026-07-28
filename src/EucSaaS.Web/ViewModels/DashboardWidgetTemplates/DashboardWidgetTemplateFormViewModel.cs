using System.ComponentModel.DataAnnotations;

namespace EucSaaS.Web.ViewModels.DashboardWidgetTemplates;

public class DashboardWidgetTemplateFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Template Code")]
    [RegularExpression(
        @"^[A-Z0-9_]+$",
        ErrorMessage =
            "Template Code can contain only uppercase letters, numbers and underscores.")]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Template Name")]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Default Widget Type")]
    public string DefaultWidgetType { get; set; } = "Card";

    [Required]
    [Display(Name = "Default SQL Query")]
    public string DefaultSqlQuery { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Default Icon")]
    public string? DefaultIcon { get; set; }

    [StringLength(50)]
    [Display(Name = "Default Colour")]
    public string? DefaultColor { get; set; }

    [Range(1, 12)]
    [Display(Name = "Default Grid Width")]
    public int DefaultGridWidth { get; set; } = 4;

    [Range(1, 12)]
    [Display(Name = "Default Grid Height")]
    public int DefaultGridHeight { get; set; } = 2;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public bool IsSystem { get; set; }

    public Guid? TenantId { get; set; }

    public List<string> WidgetTypes { get; set; } =
    [
        "Card",
        "Table",
        "Pie",
        "Doughnut",
        "Bar",
        "Line"
    ];
}
