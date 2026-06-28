public class DashboardWidgetItemViewModel
{
    public Guid Id { get; set; }

    public Guid WidgetDefinitionId { get; set; }

    public string WidgetName { get; set; } = "";

    public int Row { get; set; }

    public int Column { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool IsVisible { get; set; }
}
