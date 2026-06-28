namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardDesignerViewModel
{
    public Guid DashboardId { get; set; }

    public string DashboardName { get; set; } = string.Empty;

    public List<DashboardWidgetItemViewModel> Widgets { get; set; } = new();

    public List<DashboardWidgetDefinitionViewModel> AvailableWidgets { get; set; } = new();
}
