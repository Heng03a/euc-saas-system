namespace EucSaaS.Web.ViewModels.Dashboard;

public class RoleDashboardTemplateAssignmentViewModel
{
    public Guid Id { get; set; }

    public Guid AppRoleId { get; set; }

    public string AppRoleName { get; set; } = "";

    public Guid DashboardTemplateDefinitionId { get; set; }

    public string DashboardTemplateName { get; set; } = "";

    public bool IsActive { get; set; } = true;
}
