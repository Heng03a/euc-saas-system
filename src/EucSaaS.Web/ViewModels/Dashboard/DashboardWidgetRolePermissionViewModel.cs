namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetRolePermissionViewModel
{
    public Guid AppRoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }
}
