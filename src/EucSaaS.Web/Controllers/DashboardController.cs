using EucSaaS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardAsync();

        return View(model);
    }
}
