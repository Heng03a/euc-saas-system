namespace EucSaaS.Domain.Entities;

public class DashboardWidgetTemplate
{
    public Guid Id { get; set; }

    /// <summary>
    /// Optional tenant ownership.
    ///
    /// Null means the template is a system template that can be used
    /// by every tenant.
    ///
    /// A populated TenantId means the template belongs only to that tenant.
    /// </summary>
    public Guid? TenantId { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Default widget display type:
    /// Card, Table, Pie, Bar, Line or Doughnut.
    /// </summary>
    public string DefaultWidgetType { get; set; } = "Card";

    public string DefaultSqlQuery { get; set; } = string.Empty;

    public string? DefaultIcon { get; set; }

    public string? DefaultColor { get; set; }

    public int DefaultGridWidth { get; set; } = 4;

    public int DefaultGridHeight { get; set; } = 2;

    /// <summary>
    /// System templates are supplied by the platform and should not
    /// normally be deleted by tenant administrators.
    /// </summary>
    public bool IsSystem { get; set; }

    public bool IsActive { get; set; } = true;

    public string CreatedBy { get; set; } = "SYSTEM";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
