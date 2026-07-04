namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardWidgetDefinitionViewModel
{
    public Guid Id { get; set; }

    public string WidgetCode { get; set; } = string.Empty;

    public string WidgetName { get; set; } = string.Empty;

    public string WidgetType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SqlQuery { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int WidgetWidth { get; set; } = 6;

public int RowPosition { get; set; } = 1;

public int ColumnPosition { get; set; } = 1;

public int Height { get; set; } = 300;

public Guid? DashboardTemplateDefinitionId { get; set; }

public string? DashboardTemplateName { get; set; }

    public string Icon { get; set; } = string.Empty;

    public string Color { get; set; } = "primary";

    public bool IsActive { get; set; }

    public bool IsSelected { get; set; }
}
