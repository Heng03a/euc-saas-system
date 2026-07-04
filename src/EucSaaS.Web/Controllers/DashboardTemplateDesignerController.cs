using EucSaaS.Infrastructure.Data;
using EucSaaS.Domain.Entities;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DashboardTemplateDesignerController : Controller
{
    private readonly AppDbContext _context;

    public DashboardTemplateDesignerController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var templates = await _context.DashboardTemplateDefinitions
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.TemplateName)
            .Select(x => new DashboardTemplateDefinitionViewModel
            {
                Id = x.Id,
                TemplateCode = x.TemplateCode,
                TemplateName = x.TemplateName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(templates);
    }

    public IActionResult Create()
    {
        return View(new DashboardTemplateDefinitionViewModel
        {
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DashboardTemplateDefinitionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var exists = await _context.DashboardTemplateDefinitions
            .AnyAsync(x => x.TemplateCode == model.TemplateCode);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.TemplateCode), "Template Code already exists.");
            return View(model);
        }

        var entity = new DashboardTemplateDefinition
        {
            Id = Guid.NewGuid(),
            TemplateCode = model.TemplateCode.Trim(),
            TemplateName = model.TemplateName.Trim(),
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.DashboardTemplateDefinitions.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard template created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _context.DashboardTemplateDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        var model = new DashboardTemplateDefinitionViewModel
        {
            Id = entity.Id,
            TemplateCode = entity.TemplateCode,
            TemplateName = entity.TemplateName,
            Description = entity.Description,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DashboardTemplateDefinitionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var entity = await _context.DashboardTemplateDefinitions
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (entity == null)
            return NotFound();

        var duplicate = await _context.DashboardTemplateDefinitions
            .AnyAsync(x => x.TemplateCode == model.TemplateCode && x.Id != model.Id);

        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.TemplateCode), "Template Code already exists.");
            return View(model);
        }

        entity.TemplateCode = model.TemplateCode.Trim();
        entity.TemplateName = model.TemplateName.Trim();
        entity.Description = model.Description;
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsActive = model.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard template updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.DashboardTemplateDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        var model = new DashboardTemplateDefinitionViewModel
        {
            Id = entity.Id,
            TemplateCode = entity.TemplateCode,
            TemplateName = entity.TemplateName,
            Description = entity.Description,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _context.DashboardTemplateDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        _context.DashboardTemplateDefinitions.Remove(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard template deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
