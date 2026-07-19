using EucSaaS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EucSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "AuthenticatedOnly")]
public class DashboardApiController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardApiController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] string? department,
        [FromQuery] string? status)
    {
        Guid? appRoleId = null;

        var roleClaim = User.FindFirst("AppRoleId");

        if (roleClaim != null &&
            Guid.TryParse(roleClaim.Value, out var roleId))
        {
            appRoleId = roleId;
        }

        var model = await _dashboardService.GetDashboardAsync(
            appRoleId,
            department,
            status);

        return Ok(model);
    }
}
