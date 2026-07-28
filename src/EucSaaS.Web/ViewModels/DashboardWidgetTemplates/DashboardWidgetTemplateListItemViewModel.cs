namespace EucSaaS.Web.ViewModels.DashboardWidgetTemplates;

public class DashboardWidgetTemplateListItemViewModel
{
    public Guid Id { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string DefaultWidgetType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime CreatedAt { get; set; }

public DateTime? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public bool CanEdit =>
        !IsSystem &&
        TenantId.HasValue;

    public bool CanDelete =>
        !IsSystem &&
        TenantId.HasValue;
}
