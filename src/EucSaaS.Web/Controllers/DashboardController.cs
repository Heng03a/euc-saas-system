using EucSaaS.Web.Services;
using EucSaaS.Web.Services.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;
    private readonly IExcelExportService _excelExportService;

    public DashboardController(
        DashboardService dashboardService,
        IExcelExportService excelExportService)
    {
        _dashboardService = dashboardService;
        _excelExportService = excelExportService;
    }

    /// <summary>
    /// Loads the complete Dashboard page.
    /// </summary>
    [HttpGet("/Dashboard")]
    public async Task<IActionResult> Index(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var model =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        return View(model);
    }

    /// <summary>
    /// Reloads only the dashboard widget area through AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Refresh(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var model =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        return PartialView(
            "_DashboardContent",
            model);
    }

    /// <summary>
    /// Reloads one dashboard widget through AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshWidget(
        Guid id,
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var dashboard =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        var widget =
            dashboard.Widgets?
                .FirstOrDefault(
                    x => x.Id == id);

        if (widget == null)
        {
            return NotFound(
                "The requested dashboard widget could not be found.");
        }

        return PartialView(
            "_DashboardWidget",
            widget);
    }

    /// <summary>
    /// Exports the currently filtered dashboard to Excel.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportExcel(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var dashboard =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        var exportedBy =
            User.Identity?.Name
            ?? User.FindFirstValue(
                ClaimTypes.Name)
            ?? "Unknown User";

        var excelBytes =
            _excelExportService.ExportDashboard(
                dashboard,
                department,
                status,
                exportedBy);

        var fileName =
            $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private Guid? GetCurrentAppRoleId()
    {
        var appRoleIdValue =
            User.FindFirstValue(
                "AppRoleId");

        if (Guid.TryParse(
                appRoleIdValue,
                out var appRoleId))
        {
            return appRoleId;
        }

        return null;
    }
}
