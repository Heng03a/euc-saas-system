namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetViewModel
{
    public string WidgetCode { get; set; } = "";
    public string WidgetTitle { get; set; } = "";
    public string WidgetType { get; set; } = "Card";
    public string Value { get; set; } = "0";
    public int DisplayOrder { get; set; }
}
