using EucSaaS.Application.Interfaces;
using EucSaaS.Web.Services.DashboardLayouts;
using EucSaaS.Web.ViewModels.DashboardLayouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = "AuthenticatedOnly")]
public class DashboardLayoutsController : Controller
{
    private readonly IDashboardLayoutService
        _dashboardLayoutService;

    private readonly ICurrentUserService
        _currentUserService;

    public DashboardLayoutsController(
        IDashboardLayoutService dashboardLayoutService,
        ICurrentUserService currentUserService)
    {
        _dashboardLayoutService =
            dashboardLayoutService;

        _currentUserService =
            currentUserService;
    }

    // ============================================================
    // Index
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? ownership,
        string? status,
        Guid? appRoleId,
        Guid? departmentId,
        string? sortBy,
        string? sortDirection,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var model =
                await _dashboardLayoutService
                    .GetIndexAsync(
                        GetTenantId(),
                        searchTerm,
                        ownership,
                        status,
                        appRoleId,
                        departmentId,
                        sortBy,
                        sortDirection,
                        page,
                        pageSize);

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return View(
                new DashboardLayoutIndexViewModel());
        }
    }

    // ============================================================
    // Create
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var model =
                await _dashboardLayoutService
                    .GetCreateModelAsync(
                        GetTenantId());

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DashboardLayoutEditViewModel model)
    {
        RemoveProtectedModelStateEntries();

        await ValidateLayoutCodeAsync(
            model.LayoutCode,
            null);

        ValidateLayoutRules(model);

        if (!ModelState.IsValid)
        {
            await ReloadCreateOptionsAsync(model);

            return View(model);
        }

        try
        {
            var id =
                await _dashboardLayoutService
                    .CreateAsync(
                        model,
                        GetTenantId(),
                        GetUsername());

            TempData["SuccessMessage"] =
                $"Dashboard layout '{model.LayoutName}' was created successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await ReloadCreateOptionsAsync(model);

            return View(model);
        }
    }

    // ============================================================
    // Edit
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id)
    {
        try
        {
            var model =
                await _dashboardLayoutService
                    .GetEditModelAsync(
                        id,
                        GetTenantId());

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be edited.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        DashboardLayoutEditViewModel model)
    {
        if (id == Guid.Empty ||
            id != model.Id)
        {
            return BadRequest();
        }

        RemoveProtectedModelStateEntries();

        await ValidateLayoutCodeAsync(
            model.LayoutCode,
            model.Id);

        ValidateLayoutRules(model);

        if (!ModelState.IsValid)
        {
            await ReloadEditOptionsAsync(
                model);

            return View(model);
        }

        try
        {
            var updated =
                await _dashboardLayoutService
                    .UpdateAsync(
                        model,
                        GetTenantId(),
                        GetUsername());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be edited.";

                return RedirectToAction(
                    nameof(Index));
            }

            TempData["SuccessMessage"] =
                $"Dashboard layout '{model.LayoutName}' was updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = model.Id
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await ReloadEditOptionsAsync(
                model);

            return View(model);
        }
    }

    // ============================================================
    // Details
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id)
    {
        try
        {
            var model =
                await _dashboardLayoutService
                    .GetDetailsAsync(
                        id,
                        GetTenantId());

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    // ============================================================
    // Clone
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(
        Guid id)
    {
        try
        {
            var cloneId =
                await _dashboardLayoutService
                    .CloneAsync(
                        id,
                        GetTenantId(),
                        GetUsername());

            if (!cloneId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found and could not be cloned.";

                return RedirectToAction(
                    nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Dashboard layout was cloned successfully.";

            return RedirectToAction(
                nameof(Edit),
                new
                {
                    id = cloneId.Value
                });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    // ============================================================
    // Set default
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(
        Guid id)
    {
        try
        {
            var updated =
                await _dashboardLayoutService
                    .SetDefaultAsync(
                        id,
                        GetTenantId(),
                        GetUsername());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be made the default.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "The dashboard layout is now the default layout.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrDetails(
            id);
    }

    // ============================================================
    // Activate
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(
        Guid id)
    {
        return await ChangeActiveStatusAsync(
            id,
            true);
    }

    // ============================================================
    // Deactivate
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(
        Guid id)
    {
        return await ChangeActiveStatusAsync(
            id,
            false);
    }

    // ============================================================
    // Delete
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        try
        {
            var model =
                await _dashboardLayoutService
                    .GetDetailsAsync(
                        id,
                        GetTenantId());

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found.";
                
                return RedirectToAction(
                    nameof(Index));
            }

            if (model.IsSystem)
            {
                TempData["ErrorMessage"] =
                    "System dashboard layouts cannot be deleted.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
        DeleteConfirmed(
            Guid id)
    {
        try
        {
            var deleted =
                await _dashboardLayoutService
                    .DeleteAsync(
                        id,
                        GetTenantId());

            if (!deleted)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be deleted.";

                return RedirectToAction(
                    nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Dashboard layout was deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    // ============================================================
    // Private: Change active status
    // ============================================================

    private async Task<IActionResult>
        ChangeActiveStatusAsync(
            Guid id,
            bool isActive)
    {
        try
        {
            var updated =
                await _dashboardLayoutService
                    .SetActiveAsync(
                        id,
                        GetTenantId(),
                        isActive,
                        GetUsername());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or its status cannot be changed.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    isActive
                        ? "Dashboard layout was activated successfully."
                        : "Dashboard layout was deactivated successfully.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrDetails(
            id);
    }

    // ============================================================
    // Private: Duplicate layout-code validation
    // ============================================================

    private async Task ValidateLayoutCodeAsync(
        string? layoutCode,
        Guid? excludedId)
    {
        if (string.IsNullOrWhiteSpace(
                layoutCode))
        {
            return;
        }

        try
        {
            var exists =
                await _dashboardLayoutService
                    .LayoutCodeExistsAsync(
                        layoutCode,
                        GetTenantId(),
                        excludedId);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(
                        DashboardLayoutEditViewModel
                            .LayoutCode),
                    $"Layout Code '{layoutCode.Trim().ToUpperInvariant()}' already exists for this tenant.");
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
    }

    // ============================================================
    // Private: Business-rule validation
    // ============================================================

    private void ValidateLayoutRules(
        DashboardLayoutEditViewModel model)
    {
        if (model.IsDefault &&
            !model.IsActive)
        {
            ModelState.AddModelError(
                nameof(model.IsDefault),
                "An inactive layout cannot be the default layout.");
        }

        if (!string.IsNullOrWhiteSpace(
                model.LayoutCode))
        {
            var normalizedCode =
                model.LayoutCode
                    .Trim()
                    .ToUpperInvariant();

            if (normalizedCode.Any(
                    character =>
                        !char.IsLetterOrDigit(
                            character) &&
                        character != '_'))
            {
                ModelState.AddModelError(
                    nameof(model.LayoutCode),
                    "Layout Code may contain only letters, numbers, and underscores.");
            }
        }
    }

    // ============================================================
    // Private: Reload create dropdown options
    // ============================================================

    private async Task ReloadCreateOptionsAsync(
        DashboardLayoutEditViewModel model)
    {
        try
        {
            var source =
                await _dashboardLayoutService
                    .GetCreateModelAsync(
                        GetTenantId());

            model.RoleOptions =
                source.RoleOptions;

            model.DepartmentOptions =
                source.DepartmentOptions;

            model.TenantId =
                source.TenantId;

            model.IsSystem =
                false;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
    }

    // ============================================================
    // Private: Reload edit dropdown options
    // ============================================================

    private async Task ReloadEditOptionsAsync(
        DashboardLayoutEditViewModel model)
    {
        try
        {
            var source =
                await _dashboardLayoutService
                    .GetEditModelAsync(
                        model.Id,
                        GetTenantId());

            if (source == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dashboard layout was not found or cannot be edited.");

                return;
            }

            model.RoleOptions =
                source.RoleOptions;

            model.DepartmentOptions =
                source.DepartmentOptions;

            model.TenantId =
                source.TenantId;

            model.IsSystem =
                source.IsSystem;

            model.CreatedAt =
                source.CreatedAt;

            model.CreatedBy =
                source.CreatedBy;

            model.UpdatedAt =
                source.UpdatedAt;

            model.UpdatedBy =
                source.UpdatedBy;

            model.ItemCount =
                source.ItemCount;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
    }

    // ============================================================
    // Private: Prevent browser manipulation
    // ============================================================

    private void RemoveProtectedModelStateEntries()
    {
        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .TenantId));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .IsSystem));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .CreatedAt));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .CreatedBy));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .UpdatedAt));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .UpdatedBy));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .ItemCount));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .RoleOptions));

        ModelState.Remove(
            nameof(
                DashboardLayoutEditViewModel
                    .DepartmentOptions));
    }

    // ============================================================
    // Private: Redirect back safely
    // ============================================================

    private IActionResult
        RedirectToReturnUrlOrDetails(
            Guid id)
    {
        var returnUrl =
            Request.Form["returnUrl"]
                .ToString();

        if (!string.IsNullOrWhiteSpace(
                returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(
                returnUrl);
        }

        return RedirectToAction(
            nameof(Details),
            new
            {
                id
            });
    }

    // ============================================================
    // Private: Current tenant
    // ============================================================

    private Guid GetTenantId()
    {
        var tenantId =
            _currentUserService.TenantId;

        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The authenticated user does not have a valid tenant.");
        }

        return tenantId;
    }

    // ============================================================
    // Private: Current username
    // ============================================================

    private string GetUsername()
    {
        return string.IsNullOrWhiteSpace(
                _currentUserService.Username)
            ? User.Identity?.Name ??
              "UNKNOWN"
            : _currentUserService.Username;
    }
}
