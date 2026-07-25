using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardDesignerIndexViewModel
{
    public DashboardDesignerFilterViewModel Filter { get; set; } = new();

    public List<DashboardWidgetDesignerViewModel> Widgets { get; set; } = [];

    public List<SelectListItem> WidgetTypes { get; set; } = [];

    public List<SelectListItem> DashboardTemplates { get; set; } = [];
}
