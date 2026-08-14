using EucSaaS.Application.Interfaces;
using EucSaaS.Web.Services.DashboardLayoutItems;
using EucSaaS.Web.Services.DashboardLayouts;
using EucSaaS.Web.ViewModels.DashboardLayoutItems;
using EucSaaS.Web.ViewModels.DashboardLayouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EucSaaS.Web.Controllers;

[Authorize(Policy = "AuthenticatedOnly")]
public class DashboardLayoutItemsController : Controller
{
    private readonly IDashboardLayoutItemService
        _dashboardLayoutItemService;

    private readonly IDashboardLayoutService
        _dashboardLayoutService;

    private readonly ICurrentUserService
        _currentUserService;

    public DashboardLayoutItemsController(
        IDashboardLayoutItemService dashboardLayoutItemService,
        IDashboardLayoutService dashboardLayoutService,
        ICurrentUserService currentUserService)
    {
        _dashboardLayoutItemService =
            dashboardLayoutItemService;

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
        Guid dashboardLayoutId)
    {
        if (dashboardLayoutId == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var model =
                await BuildIndexModelAsync(
                    dashboardLayoutId);

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found.";

                return RedirectToAction(
                    "Index",
                    "DashboardLayouts");
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                "Index",
                "DashboardLayouts");
        }
    }

    // ============================================================
    // Create
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create(
        Guid dashboardLayoutId)
    {
        if (dashboardLayoutId == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var model =
                await BuildCreateModelAsync(
                    dashboardLayoutId);

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToAction(
                    "Index",
                    "DashboardLayouts");
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Guid dashboardLayoutId,
        DashboardLayoutItemEditViewModel model)
    {
        if (dashboardLayoutId == Guid.Empty)
        {
            return BadRequest();
        }

        /*
         * DashboardLayoutId is controlled by the route.
         *
         * Never trust the posted parent-layout id.
         */
        model.DashboardLayoutId =
            dashboardLayoutId;

        RemoveProtectedModelStateEntries();

        ValidateItemRules(
            model);

        if (!ModelState.IsValid)
        {
            await ReloadCreateModelAsync(
                model);

            return View(model);
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            await _dashboardLayoutItemService
                .CreateAsync(
                    dashboardLayoutId,
                    model.DashboardWidgetDefinitionId,
                    model.GridRow,
                    model.GridColumn,
                    model.GridWidth,
                    model.GridHeight,
                    model.DisplayOrder,
                    model.IsVisible,
                    model.SettingsJson,
                    GetTenantId());

            TempData["SuccessMessage"] =
                "Dashboard widget was added to the layout successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await ReloadCreateModelAsync(
                model);

            return View(model);
        }
    }

    // ============================================================
    // Edit
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid dashboardLayoutId,
        Guid id)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var model =
                await BuildEditModelAsync(
                    dashboardLayoutId,
                    id);

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or cannot be edited.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid dashboardLayoutId,
        Guid id,
        DashboardLayoutItemEditViewModel model)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty ||
            id != model.Id)
        {
            return BadRequest();
        }

        model.DashboardLayoutId =
            dashboardLayoutId;

        RemoveProtectedModelStateEntries();

        /*
         * Widget selection itself is not changed by UpdateAsync.
         *
         * Load the original item so browser manipulation cannot
         * replace DashboardWidgetDefinitionId.
         */
        DashboardLayoutItemViewModel? existingItem;

        try
        {
            existingItem =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (existingItem == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or cannot be edited.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            model.DashboardWidgetDefinitionId =
                existingItem.DashboardWidgetDefinitionId;

            model.WidgetCode =
                existingItem.WidgetCode;

            model.WidgetName =
                existingItem.WidgetName;

            model.WidgetType =
                existingItem.WidgetType;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }

        ValidateItemRules(
            model);

        if (!ModelState.IsValid)
        {
            await ReloadEditModelAsync(
                model);

            return View(model);
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            var updated =
                await _dashboardLayoutItemService
                    .UpdateAsync(
                        id,
                        model.GridRow,
                        model.GridColumn,
                        model.GridWidth,
                        model.GridHeight,
                        model.DisplayOrder,
                        model.IsVisible,
                        model.SettingsJson,
                        GetTenantId());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or cannot be edited.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            TempData["SuccessMessage"] =
                "Dashboard layout item was updated successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await ReloadEditModelAsync(
                model);

            return View(model);
        }
    }

    // ============================================================
    // Delete
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Delete(
        Guid dashboardLayoutId,
        Guid id)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            var item =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (item == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            var model =
                new DashboardLayoutItemEditViewModel
                {
                    Id =
                        item.Id,

                    DashboardLayoutId =
                        dashboardLayoutId,

                    LayoutCode =
                        layout.LayoutCode,

                    LayoutName =
                        layout.LayoutName,

                    IsSystemLayout =
                        layout.IsSystem,

                    DashboardWidgetDefinitionId =
                        item.DashboardWidgetDefinitionId,

                    WidgetCode =
                        item.WidgetCode,

                    WidgetName =
                        item.WidgetName,

                    WidgetType =
                        item.WidgetType,

                    GridRow =
                        item.GridRow,

                    GridColumn =
                        item.GridColumn,

                    GridWidth =
                        item.GridWidth,

                    GridHeight =
                        item.GridHeight,

                    DisplayOrder =
                        item.DisplayOrder,

                    IsVisible =
                        item.IsVisible,

                    SettingsJson =
                        item.SettingsJson
                };

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index),
                new
                {
                    dashboardLayoutId
                });
        }
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
        DeleteConfirmed(
            Guid dashboardLayoutId,
            Guid id)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            /*
             * Verify that this item actually belongs to the
             * dashboardLayoutId supplied in the route.
             *
             * This prevents a user from posting an item id
             * belonging to another layout.
             */
            var item =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (item == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or cannot be deleted.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            var deleted =
                await _dashboardLayoutItemService
                    .DeleteAsync(
                        id,
                        GetTenantId());

            if (!deleted)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or cannot be deleted.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        dashboardLayoutId
                    });
            }

            TempData["SuccessMessage"] =
                $"Widget '{item.WidgetName}' was removed from the dashboard layout successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrIndex(
            dashboardLayoutId);
    }

    // ============================================================
    // Move up
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(
        Guid dashboardLayoutId,
        Guid id)
    {
        return await ChangeItemOrderAsync(
            dashboardLayoutId,
            id,
            moveUp: true);
    }

    // ============================================================
    // Move down
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(
        Guid dashboardLayoutId,
        Guid id)
    {
        return await ChangeItemOrderAsync(
            dashboardLayoutId,
            id,
            moveUp: false);
    }

    // ============================================================
    // Show
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Show(
        Guid dashboardLayoutId,
        Guid id)
    {
        return await ChangeVisibilityAsync(
            dashboardLayoutId,
            id,
            true);
    }

    // ============================================================
    // Hide
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(
        Guid dashboardLayoutId,
        Guid id)
    {
        return await ChangeVisibilityAsync(
            dashboardLayoutId,
            id,
            false);
    }

    // ============================================================
    // Toggle visibility
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVisibility(
        Guid dashboardLayoutId,
        Guid id)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            var item =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (item == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            var newVisibility =
                !item.IsVisible;

            var updated =
                await _dashboardLayoutItemService
                    .SetVisibilityAsync(
                        id,
                        newVisibility,
                        GetTenantId());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or its visibility cannot be changed.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    newVisibility
                        ? $"Widget '{item.WidgetName}' is now visible."
                        : $"Widget '{item.WidgetName}' is now hidden.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrIndex(
            dashboardLayoutId);
    }

    // ============================================================
    // Private: Change item order
    // ============================================================

    private async Task<IActionResult>
        ChangeItemOrderAsync(
            Guid dashboardLayoutId,
            Guid id,
            bool moveUp)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            var item =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (item == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            bool updated;

            if (moveUp)
            {
                updated =
                    await _dashboardLayoutItemService
                        .MoveUpAsync(
                            id,
                            GetTenantId());
            }
            else
            {
                updated =
                    await _dashboardLayoutItemService
                        .MoveDownAsync(
                            id,
                            GetTenantId());
            }

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or its order cannot be changed.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    moveUp
                        ? $"Widget '{item.WidgetName}' was moved up."
                        : $"Widget '{item.WidgetName}' was moved down.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrIndex(
            dashboardLayoutId);
    }

    // ============================================================
    // Private: Change visibility
    // ============================================================

    private async Task<IActionResult>
        ChangeVisibilityAsync(
            Guid dashboardLayoutId,
            Guid id,
            bool isVisible)
    {
        if (dashboardLayoutId == Guid.Empty ||
            id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    dashboardLayoutId);

            if (layout == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout was not found or cannot be modified.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            var item =
                await GetLayoutItemAsync(
                    dashboardLayoutId,
                    id);

            if (item == null)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found.";

                return RedirectToReturnUrlOrIndex(
                    dashboardLayoutId);
            }

            var updated =
                await _dashboardLayoutItemService
                    .SetVisibilityAsync(
                        id,
                        isVisible,
                        GetTenantId());

            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "Dashboard layout item was not found or its visibility cannot be changed.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    isVisible
                        ? $"Widget '{item.WidgetName}' is now visible."
                        : $"Widget '{item.WidgetName}' is now hidden.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToReturnUrlOrIndex(
            dashboardLayoutId);
    }

    // ============================================================
    // Private: Build index model
    // ============================================================

    private async Task<DashboardLayoutItemIndexViewModel?>
        BuildIndexModelAsync(
            Guid dashboardLayoutId)
    {
        var layout =
            await _dashboardLayoutService
                .GetDetailsAsync(
                    dashboardLayoutId,
                    GetTenantId());

        if (layout == null)
        {
            return null;
        }

        var items =
            await _dashboardLayoutItemService
                .GetItemsAsync(
                    dashboardLayoutId,
                    GetTenantId());

        return new DashboardLayoutItemIndexViewModel
        {
            DashboardLayoutId =
                layout.Id,

            TenantId =
                layout.TenantId,

            LayoutCode =
                layout.LayoutCode,

            LayoutName =
                layout.LayoutName,

            Description =
                layout.Description,

            RoleName =
                layout.RoleName,

            DepartmentName =
                layout.DepartmentName,

            IsSystem =
                layout.IsSystem,

            IsDefault =
                layout.IsDefault,

            IsShared =
                layout.IsShared,

            IsActive =
                layout.IsActive,

            Items =
                items
                    .Select(
                        x =>
                            new DashboardLayoutItemListItemViewModel
                            {
                                Id =
                                    x.Id,

                                DashboardLayoutId =
                                    dashboardLayoutId,

                                DashboardWidgetDefinitionId =
                                    x.DashboardWidgetDefinitionId,

                                WidgetCode =
                                    x.WidgetCode,

                                WidgetName =
                                    x.WidgetName,

                                WidgetType =
                                    x.WidgetType,

                                GridRow =
                                    x.GridRow,

                                GridColumn =
                                    x.GridColumn,

                                GridWidth =
                                    x.GridWidth,

                                GridHeight =
                                    x.GridHeight,

                                DisplayOrder =
                                    x.DisplayOrder,

                                IsVisible =
                                    x.IsVisible,

                                SettingsJson =
                                    x.SettingsJson
                            })
                    .ToList()
        };
    }

    // ============================================================
    // Private: Build create model
    // ============================================================

    private async Task<DashboardLayoutItemEditViewModel?>
        BuildCreateModelAsync(
            Guid dashboardLayoutId)
    {
        var layout =
            await GetEditableLayoutAsync(
                dashboardLayoutId);

        if (layout == null)
        {
            return null;
        }

        var widgetOptions =
            await _dashboardLayoutItemService
                .GetAvailableWidgetOptionsAsync(
                    dashboardLayoutId,
                    GetTenantId());

        return new DashboardLayoutItemEditViewModel
        {
            Id =
                Guid.Empty,

            DashboardLayoutId =
                dashboardLayoutId,

            LayoutCode =
                layout.LayoutCode,

            LayoutName =
                layout.LayoutName,

            IsSystemLayout =
                layout.IsSystem,

            GridRow =
                1,

            GridColumn =
                1,

            GridWidth =
                4,

            GridHeight =
                2,

            DisplayOrder =
                0,

            IsVisible =
                true,

            WidgetOptions =
                widgetOptions
        };
    }

    // ============================================================
    // Private: Build edit model
    // ============================================================

    private async Task<DashboardLayoutItemEditViewModel?>
        BuildEditModelAsync(
            Guid dashboardLayoutId,
            Guid id)
    {
        var layout =
            await GetEditableLayoutAsync(
                dashboardLayoutId);

        if (layout == null)
        {
            return null;
        }

        var item =
            await GetLayoutItemAsync(
                dashboardLayoutId,
                id);

        if (item == null)
        {
            return null;
        }

        var widgetOptions =
            await _dashboardLayoutItemService
                .GetAvailableWidgetOptionsAsync(
                    dashboardLayoutId,
                    GetTenantId(),
                    item.DashboardWidgetDefinitionId);

        return new DashboardLayoutItemEditViewModel
        {
            Id =
                item.Id,

            DashboardLayoutId =
                dashboardLayoutId,

            LayoutCode =
                layout.LayoutCode,

            LayoutName =
                layout.LayoutName,

            IsSystemLayout =
                layout.IsSystem,

            DashboardWidgetDefinitionId =
                item.DashboardWidgetDefinitionId,

            WidgetCode =
                item.WidgetCode,

            WidgetName =
                item.WidgetName,

            WidgetType =
                item.WidgetType,

            GridRow =
                item.GridRow,

            GridColumn =
                item.GridColumn,

            GridWidth =
                item.GridWidth,

            GridHeight =
                item.GridHeight,

            DisplayOrder =
                item.DisplayOrder,

            IsVisible =
                item.IsVisible,

            SettingsJson =
                item.SettingsJson,

            WidgetOptions =
                widgetOptions
        };
    }

    // ============================================================
    // Private: Reload create model
    // ============================================================

    private async Task ReloadCreateModelAsync(
        DashboardLayoutItemEditViewModel model)
    {
        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    model.DashboardLayoutId);

            if (layout == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dashboard layout was not found or cannot be modified.");

                return;
            }

            model.LayoutCode =
                layout.LayoutCode;

            model.LayoutName =
                layout.LayoutName;

            model.IsSystemLayout =
                layout.IsSystem;

            model.WidgetOptions =
                await _dashboardLayoutItemService
                    .GetAvailableWidgetOptionsAsync(
                        model.DashboardLayoutId,
                        GetTenantId());
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
    }

    // ============================================================
    // Private: Reload edit model
    // ============================================================

    private async Task ReloadEditModelAsync(
        DashboardLayoutItemEditViewModel model)
    {
        try
        {
            var layout =
                await GetEditableLayoutAsync(
                    model.DashboardLayoutId);

            if (layout == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dashboard layout was not found or cannot be modified.");

                return;
            }

            var item =
                await GetLayoutItemAsync(
                    model.DashboardLayoutId,
                    model.Id);

            if (item == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dashboard layout item was not found.");

                return;
            }

            model.LayoutCode =
                layout.LayoutCode;

            model.LayoutName =
                layout.LayoutName;

            model.IsSystemLayout =
                layout.IsSystem;

            model.DashboardWidgetDefinitionId =
                item.DashboardWidgetDefinitionId;

            model.WidgetCode =
                item.WidgetCode;

            model.WidgetName =
                item.WidgetName;

            model.WidgetType =
                item.WidgetType;

            model.WidgetOptions =
                await _dashboardLayoutItemService
                    .GetAvailableWidgetOptionsAsync(
                        model.DashboardLayoutId,
                        GetTenantId(),
                        item.DashboardWidgetDefinitionId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
    }

    // ============================================================
    // Private: Get editable layout
    // ============================================================

    private async Task<DashboardLayoutDetailsViewModel?>
        GetEditableLayoutAsync(
            Guid dashboardLayoutId)
    {
        var layout =
            await _dashboardLayoutService
                .GetDetailsAsync(
                    dashboardLayoutId,
                    GetTenantId());

        if (layout == null)
        {
            return null;
        }

        /*
         * System/global layouts are readable but never
         * editable by a tenant.
         */
        if (layout.IsSystem ||
            layout.TenantId != GetTenantId())
        {
            return null;
        }

        return layout;
    }

    // ============================================================
    // Private: Get item belonging to layout
    // ============================================================

    private async Task<DashboardLayoutItemViewModel?>
        GetLayoutItemAsync(
            Guid dashboardLayoutId,
            Guid id)
    {
        var items =
            await _dashboardLayoutItemService
                .GetItemsAsync(
                    dashboardLayoutId,
                    GetTenantId());

        return items
            .FirstOrDefault(
                x =>
                    x.Id == id);
    }

    // ============================================================
    // Private: Business-rule validation
    // ============================================================

    private void ValidateItemRules(
        DashboardLayoutItemEditViewModel model)
    {
        if (model.DashboardLayoutId ==
            Guid.Empty)
        {
            ModelState.AddModelError(
                nameof(
                    model.DashboardLayoutId),
                "Dashboard Layout is required.");
        }

        if (model.DashboardWidgetDefinitionId ==
            Guid.Empty)
        {
            ModelState.AddModelError(
                nameof(
                    model.DashboardWidgetDefinitionId),
                "Dashboard Widget is required.");
        }

        if (model.GridRow < 1)
        {
            ModelState.AddModelError(
                nameof(
                    model.GridRow),
                "Grid Row must be greater than or equal to 1.");
        }

        if (model.GridColumn < 1)
        {
            ModelState.AddModelError(
                nameof(
                    model.GridColumn),
                "Grid Column must be greater than or equal to 1.");
        }

        if (model.GridWidth < 1)
        {
            ModelState.AddModelError(
                nameof(
                    model.GridWidth),
                "Grid Width must be greater than or equal to 1.");
        }

        if (model.GridHeight < 1)
        {
            ModelState.AddModelError(
                nameof(
                    model.GridHeight),
                "Grid Height must be greater than or equal to 1.");
        }

        if (model.DisplayOrder < 0)
        {
            ModelState.AddModelError(
                nameof(
                    model.DisplayOrder),
                "Display Order cannot be negative.");
        }
    }

    // ============================================================
    // Private: Prevent browser manipulation
    // ============================================================

    private void RemoveProtectedModelStateEntries()
    {
        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .DashboardLayoutId));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .LayoutCode));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .LayoutName));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .IsSystemLayout));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .WidgetCode));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .WidgetName));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .WidgetType));

        ModelState.Remove(
            nameof(
                DashboardLayoutItemEditViewModel
                    .WidgetOptions));
    }

    // ============================================================
    // Private: Redirect back safely
    // ============================================================

    private IActionResult
        RedirectToReturnUrlOrIndex(
            Guid dashboardLayoutId)
    {
        var returnUrl =
            Request.Form["returnUrl"]
                .ToString();

        if (!string.IsNullOrWhiteSpace(
                returnUrl) &&
            Url.IsLocalUrl(
                returnUrl))
        {
            return LocalRedirect(
                returnUrl);
        }

        return RedirectToAction(
            nameof(Index),
            new
            {
                dashboardLayoutId
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
