using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DashboardDesignerController : Controller
{
    private readonly AppDbContext _context;

    public DashboardDesignerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/DashboardDesigner")]
    public async Task<IActionResult> Index()
    {
        var model = await _context.DashboardWidgetDefinitions
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new DashboardWidgetDefinitionViewModel
            {
                Id = x.Id,
                WidgetCode = x.WidgetCode,
                WidgetName = x.WidgetTitle,
                WidgetType = x.WidgetType,
                Description = x.SqlQuery,
                SqlQuery = x.SqlQuery,
                DisplayOrder = x.DisplayOrder,
                WidgetWidth = x.WidgetWidth,
                Icon = x.Icon ?? "",
                Color = x.Color ?? "primary",
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet("/DashboardDesigner/Create")]
    public IActionResult Create()
    {
        var model = new DashboardWidgetDefinitionViewModel
        {
            Id = Guid.NewGuid(),
            WidgetType = "Card",
            WidgetWidth = 4,
            DisplayOrder = 99,
            Color = "primary",
            Icon = "bi bi-grid",
            IsActive = true
        };

        return View(model);
    }

    [HttpPost("/DashboardDesigner/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DashboardWidgetDefinitionViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.WidgetCode))
            ModelState.AddModelError(nameof(model.WidgetCode), "Widget Code is required.");

        if (string.IsNullOrWhiteSpace(model.WidgetName))
            ModelState.AddModelError(nameof(model.WidgetName), "Widget Title is required.");

        if (string.IsNullOrWhiteSpace(model.SqlQuery))
            ModelState.AddModelError(nameof(model.SqlQuery), "SQL Query is required.");

        var exists = await _context.DashboardWidgetDefinitions
            .AnyAsync(x => x.WidgetCode == model.WidgetCode);

        if (exists)
            ModelState.AddModelError(nameof(model.WidgetCode), "Widget Code already exists.");

        if (!ModelState.IsValid)
            return View(model);

        var widget = new DashboardWidgetDefinition
        {
            Id = Guid.NewGuid(),
            WidgetCode = model.WidgetCode.Trim().ToUpper(),
            WidgetTitle = model.WidgetName.Trim(),
            WidgetType = model.WidgetType,
            SqlQuery = model.SqlQuery.Trim(),
            DisplayOrder = model.DisplayOrder,
            WidgetWidth = model.WidgetWidth,
            Icon = model.Icon,
            Color = model.Color,
            IsActive = model.IsActive
        };

        _context.DashboardWidgetDefinitions.Add(widget);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard widget created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/DashboardDesigner/Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var widget = await _context.DashboardWidgetDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (widget == null)
            return NotFound();

        var model = new DashboardWidgetDefinitionViewModel
        {
            Id = widget.Id,
            WidgetCode = widget.WidgetCode,
            WidgetName = widget.WidgetTitle,
            WidgetType = widget.WidgetType,
            Description = widget.SqlQuery,
            SqlQuery = widget.SqlQuery,
            DisplayOrder = widget.DisplayOrder,
            WidgetWidth = widget.WidgetWidth,
            Icon = widget.Icon ?? "",
            Color = widget.Color ?? "primary",
            IsActive = widget.IsActive
        };

        return View(model);
    }

    [HttpPost("/DashboardDesigner/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DashboardWidgetDefinitionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var widget = await _context.DashboardWidgetDefinitions
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (widget == null)
            return NotFound();

        widget.WidgetTitle = model.WidgetName.Trim();
        widget.WidgetType = model.WidgetType;
        widget.SqlQuery = model.SqlQuery.Trim();
        widget.DisplayOrder = model.DisplayOrder;
        widget.WidgetWidth = model.WidgetWidth;
        widget.Icon = model.Icon;
        widget.Color = model.Color;
        widget.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard widget updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/DashboardDesigner/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var widget = await _context.DashboardWidgetDefinitions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (widget == null)
            return NotFound();

        _context.DashboardWidgetDefinitions.Remove(widget);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Dashboard widget deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
