namespace EucSaaS.Web.ViewModels.DashboardLayoutItems;

public class DashboardLayoutItemListItemViewModel
{
    public Guid Id { get; set; }

    public Guid DashboardLayoutId { get; set; }

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

    // ------------------------------------------------------------
    // Display helpers
    // ------------------------------------------------------------
    public string GridPosition =>
        $"Row {GridRow}, Column {GridColumn}";

    public string GridSize =>
        $"{GridWidth} × {GridHeight}";

    public string VisibilityLabel =>
        IsVisible
            ? "Visible"
            : "Hidden";

    public bool HasSettings =>
        !string.IsNullOrWhiteSpace(
            SettingsJson);
}
