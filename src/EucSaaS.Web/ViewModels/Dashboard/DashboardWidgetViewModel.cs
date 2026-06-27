namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetViewModel
{
public int WidgetWidth { get; set; } = 6;

public string? Icon { get; set; }

public string? Color { get; set; } = "primary";

    public string WidgetCode { get; set; } = "";
    public string WidgetTitle { get; set; } = "";
    public string WidgetType { get; set; } = "Card";

    public string Value { get; set; } = "0";

    public int DisplayOrder { get; set; }

    public List<string> Columns { get; set; } = new();

    public List<Dictionary<string, string>> Rows { get; set; } = new();
}
