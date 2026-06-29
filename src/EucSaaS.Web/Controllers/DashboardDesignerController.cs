using EucSaaS.Application.Services;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardDesignerController : Controller
{
    private readonly AppDbContext _context;
    private readonly DashboardQueryService _dashboardQueryService;

    public DashboardDesignerController(
        AppDbContext context,
        DashboardQueryService dashboardQueryService)
    {
        _context = context;
        _dashboardQueryService = dashboardQueryService;
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
        ValidateWidgetModel(model, isCreate: true);

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

    [HttpPost("/DashboardDesigner/Edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid? id, DashboardWidgetDefinitionViewModel model)
    {
        if (model.Id == Guid.Empty && id.HasValue)
            model.Id = id.Value;

        ValidateWidgetModel(model, isCreate: false);

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

[HttpPost("/DashboardDesigner/Clone/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Clone(Guid id)
{
    var sourceWidget = await _context.DashboardWidgetDefinitions
        .FirstOrDefaultAsync(x => x.Id == id);

    if (sourceWidget == null)
        return NotFound();

    var baseCode = sourceWidget.WidgetCode + "_COPY";
    var newCode = baseCode;
    var counter = 1;

    while (await _context.DashboardWidgetDefinitions
        .AnyAsync(x => x.WidgetCode == newCode))
    {
        counter++;
        newCode = $"{baseCode}_{counter}";
    }

    var maxOrder = await _context.DashboardWidgetDefinitions
        .MaxAsync(x => (int?)x.DisplayOrder) ?? 0;

    var clonedWidget = new DashboardWidgetDefinition
    {
        Id = Guid.NewGuid(),
        WidgetCode = newCode,
        WidgetTitle = sourceWidget.WidgetTitle + " Copy",
        WidgetType = sourceWidget.WidgetType,
        SqlQuery = sourceWidget.SqlQuery,
        DisplayOrder = maxOrder + 1,
        WidgetWidth = sourceWidget.WidgetWidth,
        Icon = sourceWidget.Icon,
        Color = sourceWidget.Color,
        IsActive = false
    };

    _context.DashboardWidgetDefinitions.Add(clonedWidget);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Dashboard widget cloned successfully. Please review and activate it.";

    return RedirectToAction(nameof(Edit), new { id = clonedWidget.Id });
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

    [HttpPost("/DashboardDesigner/TestSql")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestSql(string sqlQuery)
    {
        var preview = new DashboardSqlPreviewViewModel();

        try
        {
            var dataSource = await _context.DataSources
                .FirstOrDefaultAsync(x => x.IsActive);

            if (dataSource == null)
            {
                preview.IsSuccess = false;
                preview.Message = "No active data source was found.";
                return PartialView("_SqlPreview", preview);
            }

            var table = await _dashboardQueryService.TestSqlAsync(
                dataSource,
                sqlQuery,
                20);

            preview.IsSuccess = true;
            preview.Message = $"SQL executed successfully. Showing {table.Rows.Count} row(s).";

            foreach (DataColumn column in table.Columns)
                preview.Columns.Add(column.ColumnName);

            foreach (DataRow dataRow in table.Rows)
            {
                var row = new Dictionary<string, string>();

                foreach (DataColumn column in table.Columns)
                {
                    row[column.ColumnName] =
                        dataRow[column] == DBNull.Value
                            ? ""
                            : dataRow[column]?.ToString() ?? "";
                }

                preview.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            preview.IsSuccess = false;
            preview.Message = ex.Message;
        }

        return PartialView("_SqlPreview", preview);
    }

    private void ValidateWidgetModel(
        DashboardWidgetDefinitionViewModel model,
        bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(model.WidgetCode))
            ModelState.AddModelError(nameof(model.WidgetCode), "Widget Code is required.");

        if (string.IsNullOrWhiteSpace(model.WidgetName))
            ModelState.AddModelError(nameof(model.WidgetName), "Widget Title is required.");

        if (string.IsNullOrWhiteSpace(model.SqlQuery))
            ModelState.AddModelError(nameof(model.SqlQuery), "SQL Query is required.");

        if (isCreate)
        {
            var code = model.WidgetCode?.Trim().ToUpper() ?? "";

            var exists = _context.DashboardWidgetDefinitions
                .Any(x => x.WidgetCode == code);

            if (exists)
                ModelState.AddModelError(nameof(model.WidgetCode), "Widget Code already exists.");
        }
    }
}
