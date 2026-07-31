namespace EucSaaS.Domain.Entities;

public class DashboardLayoutItem
{
    public Guid Id { get; set; }

    public Guid DashboardLayoutId { get; set; }

    public Guid DashboardWidgetDefinitionId { get; set; }

    public int GridRow { get; set; } = 1;

    public int GridColumn { get; set; } = 1;

    public int GridWidth { get; set; } = 4;

    public int GridHeight { get; set; } = 2;

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Reserved for future layout-specific widget configuration.
    /// For example: filters, titles or display options.
    /// </summary>
    public string? SettingsJson { get; set; }

    public DashboardLayout DashboardLayout { get; set; } = null!;

    public DashboardWidgetDefinition DashboardWidgetDefinition { get; set; }
        = null!;
}
