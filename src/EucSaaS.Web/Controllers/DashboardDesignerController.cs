using EucSaaS.Application.Services;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Services;
using EucSaaS.Web.Services.Security;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardDesignerController : Controller
{
    private readonly AppDbContext _context;

    private readonly DashboardQueryService _dashboardQueryService;

    private readonly DashboardChartResultValidator
        _chartResultValidator;

    private readonly DashboardDesignerQueryService
        _dashboardDesignerQueryService;

    private readonly IDataAccessScopeResolver
        _scopeResolver;

    public DashboardDesignerController(
        AppDbContext context,
        DashboardQueryService dashboardQueryService,
        DashboardChartResultValidator chartResultValidator,
        DashboardDesignerQueryService dashboardDesignerQueryService,
        IDataAccessScopeResolver scopeResolver)
    {
        _context = context;

        _dashboardQueryService = dashboardQueryService;

        _chartResultValidator =
            chartResultValidator;

        _dashboardDesignerQueryService =
            dashboardDesignerQueryService;

        _scopeResolver = scopeResolver;
    }

    // ------------------------------------------------------------
    // Dashboard widget list with search and filters
    // ------------------------------------------------------------
    [HttpGet("/DashboardDesigner")]
    public async Task<IActionResult> Index(
        [FromQuery] DashboardDesignerFilterViewModel filter)
    {
        var model =
            await _dashboardDesignerQueryService
                .GetIndexAsync(filter);

        return View(model);
    }

    // ------------------------------------------------------------
    // Create widget - GET
    // ------------------------------------------------------------
    [HttpGet("/DashboardDesigner/Create")]
    public async Task<IActionResult> Create()
    {
        await LoadDashboardTemplatesAsync();

        var model = new DashboardWidgetDefinitionViewModel
        {
            Id = Guid.NewGuid(),

            WidgetType = "Card",

            WidgetWidth = 4,

            RowPosition = 1,

            ColumnPosition = 1,

            Height = 300,

            DisplayOrder = 99,

            Color = "primary",

            Icon = "bi bi-grid",

            IsActive = true
        };

        return View(model);
    }

// ------------------------------------------------------------
// Create dashboard widget from widget template
// ------------------------------------------------------------
[HttpGet(
    "/DashboardDesigner/CreateFromTemplate/{templateId:guid}")]
public async Task<IActionResult> CreateFromTemplate(
    Guid templateId)
{
    var accessScope =
        _scopeResolver.Resolve();

    if (accessScope.TenantId == Guid.Empty)
    {
        TempData["ErrorMessage"] =
            "The authenticated user does not have a valid tenant.";

        return RedirectToAction(
            "Index",
            "DashboardWidgetTemplates");
    }

    /*
     * A tenant may use:
     *
     * 1. An active global system template.
     * 2. An active template belonging to its own tenant.
     *
     * It must never use another tenant's template.
     */
    var template =
        await _context.DashboardWidgetTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == templateId &&
                    x.IsActive &&
                    (
                        x.TenantId == null ||
                        x.TenantId == accessScope.TenantId
                    ));

    if (template == null)
    {
        TempData["ErrorMessage"] =
            "The selected widget template was not found, " +
            "is inactive, or is not available to this tenant.";

        return RedirectToAction(
            "Index",
            "DashboardWidgetTemplates");
    }

    await LoadDashboardTemplatesAsync();

    var widgetCode =
        await GenerateWidgetCodeFromTemplateAsync(
            template.TemplateCode);

    var maximumDisplayOrder =
        await _context.DashboardWidgetDefinitions
            .MaxAsync(x => (int?)x.DisplayOrder)
        ?? 0;

    var model =
        new DashboardWidgetDefinitionViewModel
        {
            Id =
                Guid.NewGuid(),

            WidgetCode =
                widgetCode,

            WidgetName =
                template.TemplateName,

            WidgetType =
                template.DefaultWidgetType,

            Description =
                template.Description ?? string.Empty,

            SqlQuery =
                template.DefaultSqlQuery,

            DisplayOrder =
                maximumDisplayOrder + 1,

            /*
             * DashboardWidgetDefinition currently uses
             * WidgetWidth and Height in its Create workflow.
             *
             * Template defaults are mapped into those fields.
             */
            WidgetWidth =
                template.DefaultGridWidth <= 0
                    ? 4
                    : template.DefaultGridWidth,

            RowPosition =
                1,

            ColumnPosition =
                1,

            Height =
                ConvertTemplateGridHeightToPixels(
                    template.DefaultGridHeight),

            Icon =
                string.IsNullOrWhiteSpace(
                    template.DefaultIcon)
                    ? "bi bi-grid"
                    : template.DefaultIcon,

            Color =
                string.IsNullOrWhiteSpace(
                    template.DefaultColor)
                    ? "primary"
                    : template.DefaultColor,

            /*
             * Generated widgets start inactive.
             * The administrator should review and test them
             * before activation.
             */
            IsActive =
                false
        };

    TempData["InfoMessage"] =
        $"Widget fields were loaded from template " +
        $"'{template.TemplateName}'. " +
        $"Review and test the widget before saving.";

    return View(
        "Create",
        model);
}

    // ------------------------------------------------------------
    // Create widget - POST
    // ------------------------------------------------------------
    [HttpPost("/DashboardDesigner/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DashboardWidgetDefinitionViewModel model)
    {
        ValidateWidgetModel(
            model,
            isCreate: true);

        if (!ModelState.IsValid)
        {
            await LoadDashboardTemplatesAsync();

            return View(model);
        }

        var widget = new DashboardWidgetDefinition
        {
            Id = Guid.NewGuid(),

            DashboardTemplateDefinitionId =
                model.DashboardTemplateDefinitionId,

            WidgetCode =
                model.WidgetCode
                    .Trim()
                    .ToUpperInvariant(),

            WidgetTitle =
                model.WidgetName.Trim(),

            WidgetType =
                model.WidgetType,

            SqlQuery =
                model.SqlQuery.Trim(),

            DisplayOrder =
                model.DisplayOrder,

            WidgetWidth =
                model.WidgetWidth,

            RowPosition =
                model.RowPosition,

            ColumnPosition =
                model.ColumnPosition,

            Height =
                model.Height,

            Icon =
                model.Icon,

            Color =
                model.Color,

            IsActive =
                model.IsActive
        };

        _context.DashboardWidgetDefinitions.Add(widget);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Dashboard widget created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------
    // Edit widget - GET
    // ------------------------------------------------------------
    [HttpGet("/DashboardDesigner/Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        await LoadDashboardTemplatesAsync();

        var widget =
            await _context.DashboardWidgetDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

        if (widget == null)
        {
            return NotFound();
        }

        var model = new DashboardWidgetDefinitionViewModel
        {
            Id =
                widget.Id,

            DashboardTemplateDefinitionId =
                widget.DashboardTemplateDefinitionId,

            WidgetCode =
                widget.WidgetCode,

            WidgetName =
                widget.WidgetTitle,

            WidgetType =
                widget.WidgetType,

            Description =
                widget.SqlQuery,

            SqlQuery =
                widget.SqlQuery,

            DisplayOrder =
                widget.DisplayOrder,

            WidgetWidth =
                widget.WidgetWidth,

            RowPosition =
                widget.RowPosition,

            ColumnPosition =
                widget.ColumnPosition,

            Height =
                widget.Height,

            Icon =
                widget.Icon ?? string.Empty,

            Color =
                widget.Color ?? "primary",

            IsActive =
                widget.IsActive
        };

        return View(model);
    }

    // ------------------------------------------------------------
    // Edit widget - POST
    // ------------------------------------------------------------
    [HttpPost("/DashboardDesigner/Edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid? id,
        DashboardWidgetDefinitionViewModel model)
    {
        if (model.Id == Guid.Empty && id.HasValue)
        {
            model.Id = id.Value;
        }

        ValidateWidgetModel(
            model,
            isCreate: false);

        if (!ModelState.IsValid)
        {
            await LoadDashboardTemplatesAsync();

            return View(model);
        }

        var widget =
            await _context.DashboardWidgetDefinitions
                .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (widget == null)
        {
            return NotFound();
        }

        widget.DashboardTemplateDefinitionId =
            model.DashboardTemplateDefinitionId;

        widget.WidgetTitle =
            model.WidgetName.Trim();

        widget.WidgetType =
            model.WidgetType;

        widget.SqlQuery =
            model.SqlQuery.Trim();

        widget.DisplayOrder =
            model.DisplayOrder;

        widget.WidgetWidth =
            model.WidgetWidth;

        widget.RowPosition =
            model.RowPosition;

        widget.ColumnPosition =
            model.ColumnPosition;

        widget.Height =
            model.Height;

        widget.Icon =
            model.Icon;

        widget.Color =
            model.Color;

        widget.IsActive =
            model.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Dashboard widget updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------
    // Clone widget
    // ------------------------------------------------------------
    [HttpPost("/DashboardDesigner/Clone/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(Guid id)
    {
        var sourceWidget =
            await _context.DashboardWidgetDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

        if (sourceWidget == null)
        {
            return NotFound();
        }

        var baseCode =
            sourceWidget.WidgetCode + "_COPY";

        var newCode =
            baseCode;

        var counter = 1;

        while (
            await _context.DashboardWidgetDefinitions
                .AnyAsync(x => x.WidgetCode == newCode))
        {
            counter++;

            newCode =
                $"{baseCode}_{counter}";
        }

        var maxOrder =
            await _context.DashboardWidgetDefinitions
                .MaxAsync(x => (int?)x.DisplayOrder)
            ?? 0;

        var clonedWidget = new DashboardWidgetDefinition
        {
            Id =
                Guid.NewGuid(),

            DashboardTemplateDefinitionId =
                sourceWidget.DashboardTemplateDefinitionId,

            WidgetCode =
                newCode,

            WidgetTitle =
                sourceWidget.WidgetTitle + " Copy",

            WidgetType =
                sourceWidget.WidgetType,

            SqlQuery =
                sourceWidget.SqlQuery,

            DisplayOrder =
                maxOrder + 1,

            WidgetWidth =
                sourceWidget.WidgetWidth,

            RowPosition =
                sourceWidget.RowPosition,

            ColumnPosition =
                sourceWidget.ColumnPosition,

            Height =
                sourceWidget.Height,

            GridRow =
                sourceWidget.GridRow,

            GridColumn =
                sourceWidget.GridColumn,

            GridWidth =
                sourceWidget.GridWidth,

            GridHeight =
                sourceWidget.GridHeight,

            Icon =
                sourceWidget.Icon,

            Color =
                sourceWidget.Color,

            IsActive =
                false
        };

        _context.DashboardWidgetDefinitions.Add(clonedWidget);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Dashboard widget cloned successfully. " +
            "Please review and activate it.";

        return RedirectToAction(
            nameof(Edit),
            new
            {
                id = clonedWidget.Id
            });
    }

    // ------------------------------------------------------------
    // Delete widget
    // ------------------------------------------------------------
    [HttpPost("/DashboardDesigner/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var widget =
            await _context.DashboardWidgetDefinitions
                .FirstOrDefaultAsync(x => x.Id == id);

        if (widget == null)
        {
            return NotFound();
        }

        _context.DashboardWidgetDefinitions.Remove(widget);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Dashboard widget deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------
    // Save widget layout
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLayout(
        [FromBody]
        List<DashboardLayoutUpdateItemViewModel> items)
    {
        if (items == null || items.Count == 0)
        {
            return BadRequest(new
            {
                success = false,

                message =
                    "No layout data received."
            });
        }

        var widgetIds =
            items
                .Select(x => x.WidgetId)
                .Distinct()
                .ToList();

        var widgets =
            await _context.DashboardWidgetDefinitions
                .Where(x => widgetIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

        foreach (var item in items)
        {
            if (!widgets.TryGetValue(
                    item.WidgetId,
                    out var widget))
            {
                continue;
            }

            widget.RowPosition =
                item.RowPosition <= 0
                    ? 1
                    : item.RowPosition;

            widget.ColumnPosition =
                item.ColumnPosition <= 0
                    ? 1
                    : item.ColumnPosition;

            widget.WidgetWidth =
                item.WidgetWidth <= 0
                    ? 6
                    : item.WidgetWidth;

            widget.Height =
                item.Height <= 0
                    ? 300
                    : item.Height;

            widget.DisplayOrder =
                item.DisplayOrder <= 0
                    ? 1
                    : item.DisplayOrder;
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,

            message =
                "Dashboard layout saved successfully."
        });
    }

    // ------------------------------------------------------------
    // Test widget SQL
    // ------------------------------------------------------------
    [HttpPost("/DashboardDesigner/TestSql")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestSql(
        string sqlQuery,
        string widgetType)
    {
        var preview =
            new DashboardSqlPreviewViewModel();

        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            preview.IsSuccess = false;

            preview.Message =
                "SQL Query is required.";

            return PartialView(
                "_SqlPreview",
                preview);
        }

        try
        {
            var dataSource =
                await _context.DataSources
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IsActive);

            if (dataSource == null)
            {
                preview.IsSuccess = false;

                preview.Message =
                    "No active data source was found.";

                return PartialView(
                    "_SqlPreview",
                    preview);
            }

            var accessScope =
                _scopeResolver.Resolve();

            if (accessScope.TenantId == Guid.Empty)
            {
                preview.IsSuccess = false;

                preview.Message =
                    "The authenticated user does not have a valid tenant ID.";

                return PartialView(
                    "_SqlPreview",
                    preview);
            }

            var table =
                await _dashboardQueryService.TestSqlAsync(
                    dataSource,
                    sqlQuery,
                    accessScope.TenantId,
                    20);

            // Chart widgets must return:
            // 1. A "Label" column
            // 2. A numeric "Value" column
            //
            // ValidateAndConvert throws a clear validation
            // exception when the result structure is invalid.
            if (DashboardChartType.IsChartWidget(widgetType))
            {
                _chartResultValidator.ValidateAndConvert(
                    table);
            }

            preview.IsSuccess = true;

            preview.Message =
                $"SQL executed successfully. " +
                $"Showing {table.Rows.Count} row(s).";

            foreach (DataColumn column in table.Columns)
            {
                preview.Columns.Add(
                    column.ColumnName);
            }

            foreach (DataRow dataRow in table.Rows)
            {
                var row =
                    new Dictionary<string, string>();

                foreach (DataColumn column in table.Columns)
                {
                    row[column.ColumnName] =
                        dataRow[column] == DBNull.Value
                            ? string.Empty
                            : dataRow[column]?.ToString()
                              ?? string.Empty;
                }

                preview.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            preview.IsSuccess = false;

            preview.Message =
                ex.Message;
        }

        return PartialView(
            "_SqlPreview",
            preview);
    }

    // ------------------------------------------------------------
    // Load dashboard templates
    // ------------------------------------------------------------
    private async Task LoadDashboardTemplatesAsync()
    {
        ViewBag.DashboardTemplates =
            await _context.DashboardTemplateDefinitions
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.TemplateName)
                .Select(x => new SelectListItem
                {
                    Value =
                        x.Id.ToString(),

                    Text =
                        x.TemplateName
                })
                .ToListAsync();
    }

    // ------------------------------------------------------------
    // Validate widget model
    // ------------------------------------------------------------
    private void ValidateWidgetModel(
        DashboardWidgetDefinitionViewModel model,
        bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(model.WidgetCode))
        {
            ModelState.AddModelError(
                nameof(model.WidgetCode),
                "Widget Code is required.");
        }

        if (string.IsNullOrWhiteSpace(model.WidgetName))
        {
            ModelState.AddModelError(
                nameof(model.WidgetName),
                "Widget Title is required.");
        }

        if (string.IsNullOrWhiteSpace(model.SqlQuery))
        {
            ModelState.AddModelError(
                nameof(model.SqlQuery),
                "SQL Query is required.");
        }

        if (model.WidgetWidth <= 0 ||
            model.WidgetWidth > 12)
        {
            ModelState.AddModelError(
                nameof(model.WidgetWidth),
                "Widget Width must be between 1 and 12.");
        }

        if (model.RowPosition <= 0)
        {
            ModelState.AddModelError(
                nameof(model.RowPosition),
                "Row Position must be greater than 0.");
        }

        if (model.ColumnPosition <= 0)
        {
            ModelState.AddModelError(
                nameof(model.ColumnPosition),
                "Column Position must be greater than 0.");
        }

        if (model.Height <= 0)
        {
            ModelState.AddModelError(
                nameof(model.Height),
                "Height must be greater than 0.");
        }

        if (isCreate &&
            !string.IsNullOrWhiteSpace(model.WidgetCode))
        {
            var code =
                model.WidgetCode
                    .Trim()
                    .ToUpperInvariant();

            var exists =
                _context.DashboardWidgetDefinitions
                    .Any(x => x.WidgetCode == code);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.WidgetCode),
                    "Widget Code already exists.");
            }
        }
    }

// ------------------------------------------------------------
// Generate a unique widget code from a template code
// ------------------------------------------------------------
private async Task<string>
    GenerateWidgetCodeFromTemplateAsync(
        string templateCode)
{
    var normalizedCode =
        NormalizeWidgetCode(templateCode);

    if (string.IsNullOrWhiteSpace(normalizedCode))
    {
        normalizedCode =
            "DASHBOARD_WIDGET";
    }

    var candidateCode =
        normalizedCode;

    var counter =
        1;

    while (
        await _context.DashboardWidgetDefinitions
            .AnyAsync(
                x => x.WidgetCode == candidateCode))
    {
        counter++;

        candidateCode =
            $"{normalizedCode}_{counter}";
    }

    return candidateCode;
}

// ------------------------------------------------------------
// Normalize template code for use as a widget code
// ------------------------------------------------------------
private static string NormalizeWidgetCode(
    string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    var characters =
        value
            .Trim()
            .ToUpperInvariant()
            .Select(
                character =>
                    char.IsLetterOrDigit(character)
                        ? character
                        : '_')
            .ToArray();

    var normalized =
        new string(characters);

    while (normalized.Contains("__"))
    {
        normalized =
            normalized.Replace("__", "_");
    }

    return normalized.Trim('_');
}

// ------------------------------------------------------------
// Convert template grid height to widget pixel height
// ------------------------------------------------------------
private static int ConvertTemplateGridHeightToPixels(
    int gridHeight)
{
    /*
     * Existing Dashboard Designer stores Height in pixels,
     * while the template stores DefaultGridHeight as a
     * logical grid-unit value.
     */
    if (gridHeight <= 0)
    {
        return 300;
    }

    const int pixelsPerGridUnit =
        150;

    return Math.Max(
        150,
        gridHeight * pixelsPerGridUnit);
}

}
