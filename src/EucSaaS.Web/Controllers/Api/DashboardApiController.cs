using EucSaaS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EucSaaS.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "AuthenticatedOnly")]
[Produces("application/json")]
public class DashboardApiController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardApiController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // ------------------------------------------------------------
    // GET: /api/dashboard
    // GET: /api/dashboard?department=IT&status=Active
    // ------------------------------------------------------------
    /// <summary>
    /// Returns the current user's permitted dashboard widgets.
    /// </summary>
    /// <param name="department">
    /// Optional department filter.
    /// </param>
    /// <param name="status">
    /// Optional employee status filter.
    /// </param>
    /// <returns>
    /// The dashboard filter options and permitted dashboard widgets.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] string? department,
        [FromQuery] string? status)
    {
        var appRoleId = GetCurrentAppRoleId();

        if (!appRoleId.HasValue)
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user does not have a valid AppRoleId claim."
            });
        }

        var model = await _dashboardService.GetDashboardAsync(
            appRoleId,
            department,
            status);

        return Ok(model);
    }

    // ------------------------------------------------------------
    // GET: /api/dashboard/widget/TOTAL_EMPLOYEES
    // GET: /api/dashboard/widget/TOTAL_EMPLOYEES
    //      ?department=IT&status=Active
    // ------------------------------------------------------------
    /// <summary>
    /// Returns one permitted dashboard widget.
    /// </summary>
    /// <param name="widgetCode">
    /// The unique dashboard widget code.
    /// </param>
    /// <param name="department">
    /// Optional department filter.
    /// </param>
    /// <param name="status">
    /// Optional employee status filter.
    /// </param>
    /// <returns>
    /// The requested dashboard widget.
    /// </returns>
    [HttpGet("widget/{widgetCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWidget(
        [FromRoute] string widgetCode,
        [FromQuery] string? department,
        [FromQuery] string? status)
    {
        if (string.IsNullOrWhiteSpace(widgetCode))
        {
            return BadRequest(new
            {
                message = "Widget code is required."
            });
        }

        var appRoleId = GetCurrentAppRoleId();

        if (!appRoleId.HasValue)
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user does not have a valid AppRoleId claim."
            });
        }

        var model = await _dashboardService.GetDashboardAsync(
            appRoleId,
            department,
            status);

        var widget = model.Widgets.FirstOrDefault(x =>
            string.Equals(
                x.WidgetCode,
                widgetCode,
                StringComparison.OrdinalIgnoreCase));

        if (widget == null)
        {
            return NotFound(new
            {
                message =
                    $"Dashboard widget '{widgetCode}' was not found " +
                    "or is not permitted for the current user."
            });
        }

        return Ok(widget);
    }

    // ------------------------------------------------------------
    // Read the current user's AppRoleId claim
    // ------------------------------------------------------------
    private Guid? GetCurrentAppRoleId()
    {
        var roleClaim = User.FindFirst("AppRoleId");

        if (roleClaim == null)
        {
            return null;
        }

        if (!Guid.TryParse(roleClaim.Value, out var appRoleId))
        {
            return null;
        }

        return appRoleId;
    }
}
