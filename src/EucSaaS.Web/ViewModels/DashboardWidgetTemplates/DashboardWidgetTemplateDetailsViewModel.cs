namespace EucSaaS.Web.ViewModels.DashboardWidgetTemplates;

public class DashboardWidgetTemplateDetailsViewModel
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string DefaultWidgetType { get; set; } = string.Empty;

    public string DefaultSqlQuery { get; set; } = string.Empty;

    public string? DefaultIcon { get; set; }

    public string? DefaultColor { get; set; }

    public int DefaultGridWidth { get; set; }

    public int DefaultGridHeight { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool CanEdit =>
        !IsSystem &&
        TenantId.HasValue;

    public bool CanDelete =>
        !IsSystem &&
        TenantId.HasValue;
}
