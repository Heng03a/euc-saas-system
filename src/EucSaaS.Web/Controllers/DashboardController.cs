using EucSaaS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

[HttpGet("/Dashboard")]
public async Task<IActionResult> Index(string? department, string? status)
{
    Guid? appRoleId = null;

    var appRoleIdValue = User.FindFirst("AppRoleId")?.Value;

    if (Guid.TryParse(appRoleIdValue, out var parsedAppRoleId))
    {
        appRoleId = parsedAppRoleId;
    }

    var model = await _dashboardService.GetDashboardAsync(
        appRoleId,
        department,
        status
    );

    return View(model);
}
}
