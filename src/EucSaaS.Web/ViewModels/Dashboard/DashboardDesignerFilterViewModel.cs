namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardDesignerFilterViewModel
{
    public string? Search { get; set; }

    public string? WidgetType { get; set; }

    public bool? IsActive { get; set; }

    public Guid? DashboardTemplateId { get; set; }
}
