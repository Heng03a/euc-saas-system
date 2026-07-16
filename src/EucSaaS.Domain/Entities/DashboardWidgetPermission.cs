namespace EucSaaS.Domain.Entities;

public class DashboardWidgetPermission
{
    public Guid Id { get; set; }

    public Guid DashboardWidgetDefinitionId { get; set; }

    public Guid AppRoleId { get; set; }

    public bool CanView { get; set; } = true;

    // Navigation
    public DashboardWidgetDefinition? DashboardWidgetDefinition { get; set; }

    public AppRole? AppRole { get; set; }
}
