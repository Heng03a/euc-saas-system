namespace EucSaaS.Web.ViewModels.DashboardLayouts;

public class DashboardLayoutItemViewModel
{
    public Guid Id { get; set; }

    public Guid DashboardWidgetDefinitionId { get; set; }

    public string WidgetCode { get; set; } =
        string.Empty;

    public string WidgetName { get; set; } =
        string.Empty;

    public string WidgetType { get; set; } =
        string.Empty;

    public int GridRow { get; set; }

    public int GridColumn { get; set; }

    public int GridWidth { get; set; }

    public int GridHeight { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; }

    public string? SettingsJson { get; set; }
}
