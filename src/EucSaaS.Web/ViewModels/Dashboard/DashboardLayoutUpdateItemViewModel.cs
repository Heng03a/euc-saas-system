namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardLayoutUpdateItemViewModel
{
    public Guid WidgetId { get; set; }

    public int RowPosition { get; set; }

    public int ColumnPosition { get; set; }

    public int WidgetWidth { get; set; }

    public int Height { get; set; }

    public int DisplayOrder { get; set; }
}
