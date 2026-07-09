namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardViewModel
{
    public DashboardFilterViewModel Filter { get; set; } = new();

    public List<DashboardWidgetViewModel> Widgets { get; set; } = new();
}
