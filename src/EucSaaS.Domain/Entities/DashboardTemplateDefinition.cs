namespace EucSaaS.Domain.Entities;

public class DashboardTemplateDefinition
{
    public Guid Id { get; set; }

    public string TemplateCode { get; set; } = "";
    public string TemplateName { get; set; } = "";
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
