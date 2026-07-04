namespace EucSaaS.Domain.Entities;

public class RoleDashboardTemplateAssignment
{
    public Guid Id { get; set; }

    public Guid AppRoleId { get; set; }
    public AppRole AppRole { get; set; } = null!;

    public Guid DashboardTemplateDefinitionId { get; set; }
    public DashboardTemplateDefinition DashboardTemplateDefinition { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
