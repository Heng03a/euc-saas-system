namespace EucSaaS.Web.ViewModels.Dashboard;

public class DashboardFilterViewModel
{
    public string? Department { get; set; }
    public string? Status { get; set; }

    public List<string> Departments { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
}
