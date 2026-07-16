using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class DashboardPermissionController : Controller
{
    private readonly AppDbContext _context;

    public DashboardPermissionController(AppDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------
    // GET: /DashboardPermission
    // -------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var widgets = await _context.DashboardWidgetDefinitions
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.WidgetTitle)
            .ToListAsync();

        var roles = await _context.AppRoles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        var existingPermissions = await _context
            .DashboardWidgetPermissions
            .AsNoTracking()
            .ToListAsync();

        var model = widgets
            .Select(widget => new DashboardWidgetPermissionViewModel
            {
                DashboardWidgetDefinitionId = widget.Id,
                WidgetCode = widget.WidgetCode,
                WidgetTitle = widget.WidgetTitle,
                WidgetType = widget.WidgetType,
                IsActive = widget.IsActive,

                Roles = roles
                    .Select(role =>
                        new DashboardWidgetRolePermissionViewModel
                        {
                            AppRoleId = role.Id,
                            RoleName = role.Name,

                            IsAllowed = existingPermissions.Any(
                                permission =>
                                    permission.DashboardWidgetDefinitionId
                                        == widget.Id
                                    &&
                                    permission.AppRoleId == role.Id
                                    &&
                                    permission.CanView)
                        })
                    .ToList()
            })
            .ToList();

        return View(model);
    }

    // -------------------------------------------------
    // POST: /DashboardPermission
    // -------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        List<DashboardWidgetPermissionViewModel> model)
    {
        if (model == null || model.Count == 0)
        {
            TempData["ErrorMessage"] =
                "No dashboard widget permissions were submitted.";

            return RedirectToAction(nameof(Index));
        }

        var widgetIds = model
            .Select(x => x.DashboardWidgetDefinitionId)
            .Distinct()
            .ToList();

        var validWidgetIds = await _context
            .DashboardWidgetDefinitions
            .Where(x => widgetIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        var submittedRoleIds = model
            .SelectMany(x => x.Roles ?? new())
            .Select(x => x.AppRoleId)
            .Distinct()
            .ToList();

        var validRoleIds = await _context.AppRoles
            .Where(x => submittedRoleIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var existingPermissions = await _context
                .DashboardWidgetPermissions
                .Where(x =>
                    validWidgetIds.Contains(
                        x.DashboardWidgetDefinitionId))
                .ToListAsync();

            _context.DashboardWidgetPermissions.RemoveRange(
                existingPermissions);

            var newPermissions =
                new List<DashboardWidgetPermission>();

            foreach (var widget in model)
            {
                if (!validWidgetIds.Contains(
                        widget.DashboardWidgetDefinitionId))
                {
                    continue;
                }

                foreach (var role in widget.Roles ?? new())
                {
                    if (!role.IsAllowed)
                    {
                        continue;
                    }

                    if (!validRoleIds.Contains(role.AppRoleId))
                    {
                        continue;
                    }

                    newPermissions.Add(
                        new DashboardWidgetPermission
                        {
                            Id = Guid.NewGuid(),

                            DashboardWidgetDefinitionId =
                                widget.DashboardWidgetDefinitionId,

                            AppRoleId = role.AppRoleId,

                            CanView = true
                        });
                }
            }

            if (newPermissions.Count > 0)
            {
                await _context.DashboardWidgetPermissions
                    .AddRangeAsync(newPermissions);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Dashboard widget permissions saved successfully.";
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                "Unable to save dashboard widget permissions.";

            throw;
        }

        return RedirectToAction(nameof(Index));
    }
}
