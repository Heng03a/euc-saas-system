namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardSqlPreviewViewModel
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = "";

    public List<string> Columns { get; set; } = new();

    public List<Dictionary<string, string>> Rows { get; set; } = new();
}
