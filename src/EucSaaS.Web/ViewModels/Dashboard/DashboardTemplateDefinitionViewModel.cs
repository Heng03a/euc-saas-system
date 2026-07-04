namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardTemplateDefinitionViewModel
{
    public Guid Id { get; set; }

    public string TemplateCode { get; set; } = "";
    public string TemplateName { get; set; } = "";
    public string? Description { get; set; }

public Guid? DashboardTemplateDefinitionId { get; set; }

public string? DashboardTemplateName { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
