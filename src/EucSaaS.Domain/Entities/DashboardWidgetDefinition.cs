namespace EucSaaS.Domain.Entities;

public class DashboardWidgetDefinition
{
    public Guid Id { get; set; }

    public string WidgetCode { get; set; } = "";
    public string WidgetTitle { get; set; } = "";

    public string WidgetType { get; set; } = "Card";

    public string SqlQuery { get; set; } = "";

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
