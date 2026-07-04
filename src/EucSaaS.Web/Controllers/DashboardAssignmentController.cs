using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardAssignmentController : Controller
{
    private readonly AppDbContext _context;

    public DashboardAssignmentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/DashboardAssignment")]
    public async Task<IActionResult> Index()
    {
        var model = await _context.RoleDashboardTemplateAssignments
            .Include(x => x.AppRole)
            .Include(x => x.DashboardTemplateDefinition)
            .OrderBy(x => x.AppRole.Name)
            .Select(x => new RoleDashboardTemplateAssignmentViewModel
            {
                Id = x.Id,
                AppRoleId = x.AppRoleId,
                AppRoleName = x.AppRole.Name,
                DashboardTemplateDefinitionId = x.DashboardTemplateDefinitionId,
                DashboardTemplateName = x.DashboardTemplateDefinition.TemplateName,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet("/DashboardAssignment/Create")]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();

        return View(new RoleDashboardTemplateAssignmentViewModel
        {
            IsActive = true
        });
    }

    [HttpPost("/DashboardAssignment/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleDashboardTemplateAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var exists = await _context.RoleDashboardTemplateAssignments
            .AnyAsync(x =>
                x.AppRoleId == model.AppRoleId &&
                x.DashboardTemplateDefinitionId == model.DashboardTemplateDefinitionId);

        if (exists)
        {
            ModelState.AddModelError("", "This role is already assigned to this dashboard template.");
            await LoadDropdownsAsync();
            return View(model);
        }

        var entity = new RoleDashboardTemplateAssignment
        {
            Id = Guid.NewGuid(),
            AppRoleId = model.AppRoleId,
            DashboardTemplateDefinitionId = model.DashboardTemplateDefinitionId,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoleDashboardTemplateAssignments.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard assignment created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/DashboardAssignment/Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        await LoadDropdownsAsync();

        var entity = await _context.RoleDashboardTemplateAssignments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        var model = new RoleDashboardTemplateAssignmentViewModel
        {
            Id = entity.Id,
            AppRoleId = entity.AppRoleId,
            DashboardTemplateDefinitionId = entity.DashboardTemplateDefinitionId,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost("/DashboardAssignment/Edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid? id, RoleDashboardTemplateAssignmentViewModel model)
    {
        if (model.Id == Guid.Empty && id.HasValue)
            model.Id = id.Value;

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var entity = await _context.RoleDashboardTemplateAssignments
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (entity == null)
            return NotFound();

        var duplicate = await _context.RoleDashboardTemplateAssignments
            .AnyAsync(x =>
                x.Id != model.Id &&
                x.AppRoleId == model.AppRoleId &&
                x.DashboardTemplateDefinitionId == model.DashboardTemplateDefinitionId);

        if (duplicate)
        {
            ModelState.AddModelError("", "This role is already assigned to this dashboard template.");
            await LoadDropdownsAsync();
            return View(model);
        }

        entity.AppRoleId = model.AppRoleId;
        entity.DashboardTemplateDefinitionId = model.DashboardTemplateDefinitionId;
        entity.IsActive = model.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard assignment updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/DashboardAssignment/Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.RoleDashboardTemplateAssignments
            .Include(x => x.AppRole)
            .Include(x => x.DashboardTemplateDefinition)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        var model = new RoleDashboardTemplateAssignmentViewModel
        {
            Id = entity.Id,
            AppRoleId = entity.AppRoleId,
            AppRoleName = entity.AppRole.Name,
            DashboardTemplateDefinitionId = entity.DashboardTemplateDefinitionId,
            DashboardTemplateName = entity.DashboardTemplateDefinition.TemplateName,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost("/DashboardAssignment/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _context.RoleDashboardTemplateAssignments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        _context.RoleDashboardTemplateAssignments.Remove(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard assignment deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.AppRoles = await _context.AppRoles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        ViewBag.DashboardTemplates = await _context.DashboardTemplateDefinitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.TemplateName
            })
            .ToListAsync();
    }
}
