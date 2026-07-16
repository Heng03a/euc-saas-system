namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetPermissionViewModel
{
    public Guid DashboardWidgetDefinitionId { get; set; }

    public string WidgetCode { get; set; } = string.Empty;

    public string WidgetTitle { get; set; } = string.Empty;

    public string WidgetType { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public List<DashboardWidgetRolePermissionViewModel> Roles { get; set; }
        = new();
}
