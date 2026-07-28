using EucSaaS.Web.Services.DashboardWidgetTemplates;
using EucSaaS.Web.ViewModels.DashboardWidgetTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardWidgetTemplatesController
    : Controller
{
    private readonly IDashboardWidgetTemplateService
        _templateService;

    public DashboardWidgetTemplatesController(
        IDashboardWidgetTemplateService templateService)
    {
        _templateService = templateService;
    }

    // ------------------------------------------------------------
    // Index
    // ------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? category,
        string? status,
        string? ownership,
        string? sortBy = "templatename",
        string? sortDirection = "asc",
        int page = 1,
        int pageSize = 10)
    {
        var model =
            await _templateService.GetIndexAsync(
                search,
                category,
                status,
                ownership,
                sortBy,
                sortDirection,
                page,
                pageSize);

        return View(model);
    }

    // ------------------------------------------------------------
    // Details
    // ------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id)
    {
        var model =
            await _templateService.GetDetailsAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] =
                "The dashboard widget template was not found.";

            return RedirectToAction(
                nameof(Index));
        }

        return View(model);
    }

    // ------------------------------------------------------------
    // Create GET
    // ------------------------------------------------------------
    [HttpGet]
    public IActionResult Create()
    {
        var model =
            new DashboardWidgetTemplateFormViewModel
            {
                IsActive = true,
                IsSystem = false,
                DefaultGridWidth = 3,
                DefaultGridHeight = 2
            };

        return View(model);
    }

    // ------------------------------------------------------------
    // Create POST
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DashboardWidgetTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _templateService.CreateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "The dashboard widget template could not be created.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "The dashboard widget template was created successfully.";

        return RedirectToAction(
            nameof(Index));
    }

    // ------------------------------------------------------------
    // Edit GET
    // ------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id)
    {
        var model =
            await _templateService.GetForEditAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] =
                "The template was not found or cannot be edited.";

            return RedirectToAction(
                nameof(Index));
        }

        return View(model);
    }

    // ------------------------------------------------------------
    // Edit POST
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        DashboardWidgetTemplateFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            model.Id = id;
        }

        if (model.Id.Value != id)
        {
            return BadRequest(
                "The template ID is invalid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _templateService.UpdateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "The dashboard widget template could not be updated.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "The dashboard widget template was updated successfully.";

        return RedirectToAction(
            nameof(Index));
    }

    // ------------------------------------------------------------
    // Toggle status
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        string? returnSearch,
        string? returnCategory,
        string? returnStatus,
        string? returnOwnership,
        string? returnSortBy,
        string? returnSortDirection,
        int returnPage = 1,
        int returnPageSize = 10)
    {
        var result =
            await _templateService.ToggleStatusAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] =
                "The template status was changed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ??
                "The template status could not be changed.";
        }

        return RedirectToIndex(
            returnSearch,
            returnCategory,
            returnStatus,
            returnOwnership,
            returnSortBy,
            returnSortDirection,
            returnPage,
            returnPageSize);
    }

    // ------------------------------------------------------------
    // Clone
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(
        Guid id)
    {
        var result =
            await _templateService.CloneAsync(id);

        if (!result.Success ||
            !result.NewTemplateId.HasValue)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ??
                "The dashboard widget template could not be cloned.";

            return RedirectToAction(
                nameof(Index));
        }

        TempData["SuccessMessage"] =
            "The dashboard widget template was cloned successfully. " +
            "You may now customise the tenant-owned copy.";

        return RedirectToAction(
            nameof(Edit),
            new
            {
                id = result.NewTemplateId.Value
            });
    }

    // ------------------------------------------------------------
    // Delete
    // ------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        Guid id,
        string? returnSearch,
        string? returnCategory,
        string? returnStatus,
        string? returnOwnership,
        string? returnSortBy,
        string? returnSortDirection,
        int returnPage = 1,
        int returnPageSize = 10)
    {
        var result =
            await _templateService.DeleteAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] =
                "The dashboard widget template was deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ??
                "The dashboard widget template could not be deleted.";
        }

        return RedirectToIndex(
            returnSearch,
            returnCategory,
            returnStatus,
            returnOwnership,
            returnSortBy,
            returnSortDirection,
            returnPage,
            returnPageSize);
    }

    // ------------------------------------------------------------
    // Redirect to Index while preserving filters and paging
    // ------------------------------------------------------------
    private IActionResult RedirectToIndex(
        string? search,
        string? category,
        string? status,
        string? ownership,
        string? sortBy,
        string? sortDirection,
        int page,
        int pageSize)
    {
        return RedirectToAction(
            nameof(Index),
            new
            {
                search,
                category,
                status,
                ownership,
                sortBy,
                sortDirection,
                page,
                pageSize
            });
    }
}
