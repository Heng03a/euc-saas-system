namespace EucSaaS.Domain.Entities;

public class DashboardWidgetDefinition
{
    public Guid Id { get; set; }

public int WidgetWidth { get; set; } = 6;

public string? Icon { get; set; }

public string? Color { get; set; } = "primary";

public int RowPosition { get; set; } = 1;
public int ColumnPosition { get; set; } = 1;
public int Height { get; set; } = 300;

    public string WidgetCode { get; set; } = "";
    public string WidgetTitle { get; set; } = "";

    public string WidgetType { get; set; } = "Card";

    public string SqlQuery { get; set; } = "";

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
