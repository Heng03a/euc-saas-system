using EucSaaS.Web.ViewModels.Dashboard;

namespace EucSaaS.Web.Services.Export;

public interface IExcelExportService
{
    byte[] ExportDashboard(
        DashboardViewModel dashboard,
        string? department,
        string? status,
        string exportedBy);
}
