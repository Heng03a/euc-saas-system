namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetDesignerViewModel
{
    public Guid Id { get; set; }

    public Guid? DashboardTemplateDefinitionId { get; set; }

    public string DashboardTemplateName { get; set; } = string.Empty;

    public string WidgetCode { get; set; } = string.Empty;

    public string WidgetName { get; set; } = string.Empty;

    public string WidgetType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int WidgetWidth { get; set; }

    public int RowPosition { get; set; }

    public int ColumnPosition { get; set; }

    public int Height { get; set; }

    public int GridRow { get; set; }

    public int GridColumn { get; set; }

    public int GridWidth { get; set; }

    public int GridHeight { get; set; }

    public string Icon { get; set; } = string.Empty;

    public string Color { get; set; } = "primary";

    public bool IsActive { get; set; }
}
