using System.Text.Json;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.DashboardLayouts;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services.DashboardLayoutItems;

public class DashboardLayoutItemService
    : IDashboardLayoutItemService
{
    private readonly AppDbContext _db;

    public DashboardLayoutItemService(
        AppDbContext db)
    {
        _db = db;
    }

    // ------------------------------------------------------------
    // Get layout items
    // ------------------------------------------------------------
    public async Task<
        IReadOnlyList<DashboardLayoutItemViewModel>>
        GetItemsAsync(
            Guid dashboardLayoutId,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        /*
         * A tenant may view:
         *
         * 1. A global/system layout.
         * 2. Its own tenant layout.
         *
         * A tenant must never see another tenant's layout.
         */
        var layoutAccessible =
            await _db.DashboardLayouts
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id ==
                            dashboardLayoutId &&
                        (
                            x.TenantId == null ||
                            x.TenantId ==
                                requiredTenantId
                        ));

        if (!layoutAccessible)
        {
            return Array.Empty<
                DashboardLayoutItemViewModel>();
        }

        return await _db.DashboardLayoutItems
            .AsNoTracking()
            .Where(
                x =>
                    x.DashboardLayoutId ==
                        dashboardLayoutId)
            .OrderBy(
                x =>
                    x.DisplayOrder)
            .ThenBy(
                x =>
                    x.GridRow)
            .ThenBy(
                x =>
                    x.GridColumn)
            .Select(
                x =>
                    new DashboardLayoutItemViewModel
                    {
                        Id =
                            x.Id,

                        DashboardWidgetDefinitionId =
                            x.DashboardWidgetDefinitionId,

                        WidgetCode =
                            x.DashboardWidgetDefinition
                                .WidgetCode,

                        WidgetName =
                            x.DashboardWidgetDefinition
                                .WidgetTitle,

                        WidgetType =
                            x.DashboardWidgetDefinition
                                .WidgetType,

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
            .ToListAsync();
    }

    // ------------------------------------------------------------
    // Get one layout item
    // ------------------------------------------------------------
    public async Task<DashboardLayoutItemViewModel?>
        GetItemAsync(
            Guid id,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        return await _db.DashboardLayoutItems
            .AsNoTracking()
            .Where(
                x =>
                    x.Id == id &&
                    (
                        x.DashboardLayout.TenantId ==
                            null ||

                        x.DashboardLayout.TenantId ==
                            requiredTenantId
                    ))
            .Select(
                x =>
                    new DashboardLayoutItemViewModel
                    {
                        Id =
                            x.Id,

                        DashboardWidgetDefinitionId =
                            x.DashboardWidgetDefinitionId,

                        WidgetCode =
                            x.DashboardWidgetDefinition
                                .WidgetCode,

                        WidgetName =
                            x.DashboardWidgetDefinition
                                .WidgetTitle,

                        WidgetType =
                            x.DashboardWidgetDefinition
                                .WidgetType,

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
            .FirstOrDefaultAsync();
    }

    // ------------------------------------------------------------
    // Available widget options
    // ------------------------------------------------------------
    public async Task<IReadOnlyList<SelectListItem>>
        GetAvailableWidgetOptionsAsync(
            Guid dashboardLayoutId,
            Guid? tenantId,
            Guid? selectedWidgetDefinitionId = null)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        /*
         * Widget management is allowed only against
         * the tenant's own non-system layout.
         */
        await EnsureEditableLayoutAsync(
            dashboardLayoutId,
            requiredTenantId);

        /*
         * Exclude widgets already assigned to this layout.
         *
         * When editing, the currently selected widget may
         * remain available.
         */
        var assignedWidgetIds =
            await _db.DashboardLayoutItems
                .AsNoTracking()
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                            dashboardLayoutId &&
                        (
                            !selectedWidgetDefinitionId
                                .HasValue ||

                            x.DashboardWidgetDefinitionId !=
                                selectedWidgetDefinitionId
                                    .Value
                        ))
                .Select(
                    x =>
                        x.DashboardWidgetDefinitionId)
                .ToListAsync();

        /*
         * A tenant may use:
         *
         * 1. Global widget definitions.
         * 2. Its own tenant widget definitions.
         *
         * Only active widgets are offered.
         */
var widgets =
    await _db.DashboardWidgetDefinitions
        .AsNoTracking()
        .Where(
            x =>
                x.IsActive &&
                !assignedWidgetIds
                    .Contains(x.Id))
        .OrderBy(
            x =>
                x.WidgetTitle)
        .ThenBy(
            x =>
                x.WidgetCode)
        .Select(
            x =>
                new SelectListItem
                {
                    Value =
                        x.Id.ToString(),

                    Text =
                        x.WidgetTitle +
                        " (" +
                        x.WidgetCode +
                        ")"
                })
        .ToListAsync();

widgets.Insert(
    0,
    new SelectListItem
    {
        Value =
            string.Empty,

        Text =
            "Select widget"
    });

return widgets;
}

    // ------------------------------------------------------------
    // Create item
    // ------------------------------------------------------------
    public async Task<Guid>
        CreateAsync(
            Guid dashboardLayoutId,
            Guid dashboardWidgetDefinitionId,
            int gridRow,
            int gridColumn,
            int gridWidth,
            int gridHeight,
            int displayOrder,
            bool isVisible,
            string? settingsJson,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        if (dashboardLayoutId ==
            Guid.Empty)
        {
            throw new InvalidOperationException(
                "Dashboard Layout is required.");
        }

        if (dashboardWidgetDefinitionId ==
            Guid.Empty)
        {
            throw new InvalidOperationException(
                "Dashboard Widget is required.");
        }

        ValidateGrid(
            gridRow,
            gridColumn,
            gridWidth,
            gridHeight);

        settingsJson =
            NormalizeSettingsJson(
                settingsJson);

        await EnsureEditableLayoutAsync(
            dashboardLayoutId,
            requiredTenantId);

await EnsureWidgetAccessibleAsync(
    dashboardWidgetDefinitionId);

        // --------------------------------------------------------
        // Prevent the same widget being added twice
        // --------------------------------------------------------
        var duplicateExists =
            await _db.DashboardLayoutItems
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.DashboardLayoutId ==
                            dashboardLayoutId &&

                        x.DashboardWidgetDefinitionId ==
                            dashboardWidgetDefinitionId);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "This widget is already included in the selected dashboard layout.");
        }

        /*
         * If no valid order is supplied, append the
         * widget to the end of the layout.
         */
        if (displayOrder <= 0)
        {
            var maximumDisplayOrder =
                await _db.DashboardLayoutItems
                    .Where(
                        x =>
                            x.DashboardLayoutId ==
                                dashboardLayoutId)
                    .Select(
                        x =>
                            (int?)x.DisplayOrder)
                    .MaxAsync() ?? 0;

            displayOrder =
                maximumDisplayOrder + 1;
        }

        var entity =
            new DashboardLayoutItem
            {
                Id =
                    Guid.NewGuid(),

                DashboardLayoutId =
                    dashboardLayoutId,

                DashboardWidgetDefinitionId =
                    dashboardWidgetDefinitionId,

                GridRow =
                    gridRow,

                GridColumn =
                    gridColumn,

                GridWidth =
                    gridWidth,

                GridHeight =
                    gridHeight,

                DisplayOrder =
                    displayOrder,

                IsVisible =
                    isVisible,

                SettingsJson =
                    settingsJson
            };

        _db.DashboardLayoutItems.Add(
            entity);

        await _db.SaveChangesAsync();

        await NormalizeDisplayOrdersAsync(
            dashboardLayoutId);

        return entity.Id;
    }

    // ------------------------------------------------------------
    // Update item
    // ------------------------------------------------------------
    public async Task<bool>
        UpdateAsync(
            Guid id,
            int gridRow,
            int gridColumn,
            int gridWidth,
            int gridHeight,
            int displayOrder,
            bool isVisible,
            string? settingsJson,
            Guid? tenantId)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        ValidateGrid(
            gridRow,
            gridColumn,
            gridWidth,
            gridHeight);

        settingsJson =
            NormalizeSettingsJson(
                settingsJson);

        var entity =
            await _db.DashboardLayoutItems
                .Include(
                    x =>
                        x.DashboardLayout)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.DashboardLayout.IsSystem &&
                        x.DashboardLayout.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        entity.GridRow =
            gridRow;

        entity.GridColumn =
            gridColumn;

        entity.GridWidth =
            gridWidth;

        entity.GridHeight =
            gridHeight;

        entity.DisplayOrder =
            displayOrder <= 0
                ? entity.DisplayOrder
                : displayOrder;

        entity.IsVisible =
            isVisible;

        entity.SettingsJson =
            settingsJson;

        await _db.SaveChangesAsync();

        await NormalizeDisplayOrdersAsync(
            entity.DashboardLayoutId);

        return true;
    }

    // ------------------------------------------------------------
    // Set visibility
    // ------------------------------------------------------------
    public async Task<bool>
        SetVisibilityAsync(
            Guid id,
            bool isVisible,
            Guid? tenantId)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        var entity =
            await _db.DashboardLayoutItems
                .Include(
                    x =>
                        x.DashboardLayout)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.DashboardLayout.IsSystem &&
                        x.DashboardLayout.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        entity.IsVisible =
            isVisible;

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Move up
    // ------------------------------------------------------------
    public async Task<bool>
        MoveUpAsync(
            Guid id,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        var entity =
            await GetEditableEntityAsync(
                id,
                requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        await NormalizeDisplayOrdersAsync(
            entity.DashboardLayoutId);

        /*
         * Reload after normalization so that the
         * current DisplayOrder is guaranteed to be
         * sequential.
         */
        entity =
            await GetEditableEntityAsync(
                id,
                requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        var previous =
            await _db.DashboardLayoutItems
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                            entity.DashboardLayoutId &&

                        x.DisplayOrder <
                            entity.DisplayOrder)
                .OrderByDescending(
                    x =>
                        x.DisplayOrder)
                .FirstOrDefaultAsync();

        if (previous == null)
        {
            return true;
        }

        var currentOrder =
            entity.DisplayOrder;

        entity.DisplayOrder =
            previous.DisplayOrder;

        previous.DisplayOrder =
            currentOrder;

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Move down
    // ------------------------------------------------------------
    public async Task<bool>
        MoveDownAsync(
            Guid id,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        var entity =
            await GetEditableEntityAsync(
                id,
                requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        await NormalizeDisplayOrdersAsync(
            entity.DashboardLayoutId);

        entity =
            await GetEditableEntityAsync(
                id,
                requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        var next =
            await _db.DashboardLayoutItems
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                            entity.DashboardLayoutId &&

                        x.DisplayOrder >
                            entity.DisplayOrder)
                .OrderBy(
                    x =>
                        x.DisplayOrder)
                .FirstOrDefaultAsync();

        if (next == null)
        {
            return true;
        }

        var currentOrder =
            entity.DisplayOrder;

        entity.DisplayOrder =
            next.DisplayOrder;

        next.DisplayOrder =
            currentOrder;

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Delete
    // ------------------------------------------------------------
    public async Task<bool>
        DeleteAsync(
            Guid id,
            Guid? tenantId)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var requiredTenantId =
            GetRequiredTenantId(
                tenantId);

        var entity =
            await _db.DashboardLayoutItems
                .Include(
                    x =>
                        x.DashboardLayout)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.DashboardLayout.IsSystem &&
                        x.DashboardLayout.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        var dashboardLayoutId =
            entity.DashboardLayoutId;

        _db.DashboardLayoutItems.Remove(
            entity);

        await _db.SaveChangesAsync();

        /*
         * Compact remaining item orders after deletion.
         */
        await NormalizeDisplayOrdersAsync(
            dashboardLayoutId);

        return true;
    }

    // ------------------------------------------------------------
    // Get editable entity
    // ------------------------------------------------------------
    private async Task<DashboardLayoutItem?>
        GetEditableEntityAsync(
            Guid id,
            Guid tenantId)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _db.DashboardLayoutItems
            .Include(
                x =>
                    x.DashboardLayout)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    !x.DashboardLayout.IsSystem &&
                    x.DashboardLayout.TenantId ==
                        tenantId);
    }

    // ------------------------------------------------------------
    // Validate editable layout
    // ------------------------------------------------------------
    private async Task
        EnsureEditableLayoutAsync(
            Guid dashboardLayoutId,
            Guid tenantId)
    {
        var layoutExists =
            await _db.DashboardLayouts
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id ==
                            dashboardLayoutId &&

                        !x.IsSystem &&

                        x.TenantId ==
                            tenantId);

        if (!layoutExists)
        {
            throw new InvalidOperationException(
                "The dashboard layout does not exist or cannot be modified by the current tenant.");
        }
    }

    // ------------------------------------------------------------
    // Validate widget
    // ------------------------------------------------------------
private async Task
    EnsureWidgetAccessibleAsync(
        Guid dashboardWidgetDefinitionId)
{
    var widgetExists =
        await _db.DashboardWidgetDefinitions
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id ==
                        dashboardWidgetDefinitionId &&

                    x.IsActive);

    if (!widgetExists)
    {
        throw new InvalidOperationException(
            "The selected dashboard widget is invalid or inactive.");
    }
}

    // ------------------------------------------------------------
    // Normalize display orders
    // ------------------------------------------------------------
    private async Task
        NormalizeDisplayOrdersAsync(
            Guid dashboardLayoutId)
    {
        var items =
            await _db.DashboardLayoutItems
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                            dashboardLayoutId)
                .OrderBy(
                    x =>
                        x.DisplayOrder)
                .ThenBy(
                    x =>
                        x.GridRow)
                .ThenBy(
                    x =>
                        x.GridColumn)
                .ThenBy(
                    x =>
                        x.Id)
                .ToListAsync();

        var changed =
            false;

        for (var index = 0;
             index < items.Count;
             index++)
        {
            var expectedDisplayOrder =
                index + 1;

            if (items[index].DisplayOrder ==
                expectedDisplayOrder)
            {
                continue;
            }

            items[index].DisplayOrder =
                expectedDisplayOrder;

            changed =
                true;
        }

        if (changed)
        {
            await _db.SaveChangesAsync();
        }
    }

    // ------------------------------------------------------------
    // Grid validation
    // ------------------------------------------------------------
    private static void
        ValidateGrid(
            int gridRow,
            int gridColumn,
            int gridWidth,
            int gridHeight)
    {
        if (gridRow < 1)
        {
            throw new InvalidOperationException(
                "Grid Row must be greater than or equal to 1.");
        }

        if (gridColumn < 1)
        {
            throw new InvalidOperationException(
                "Grid Column must be greater than or equal to 1.");
        }

        if (gridWidth < 1)
        {
            throw new InvalidOperationException(
                "Grid Width must be greater than or equal to 1.");
        }

        if (gridHeight < 1)
        {
            throw new InvalidOperationException(
                "Grid Height must be greater than or equal to 1.");
        }
    }

    // ------------------------------------------------------------
    // Normalize / validate Settings JSON
    // ------------------------------------------------------------
    private static string?
        NormalizeSettingsJson(
            string? settingsJson)
    {
        if (string.IsNullOrWhiteSpace(
                settingsJson))
        {
            return null;
        }

        var normalized =
            settingsJson.Trim();

        try
        {
            using var document =
                JsonDocument.Parse(
                    normalized);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Settings JSON must contain valid JSON.");
        }

        return normalized;
    }

    // ------------------------------------------------------------
    // Required tenant
    // ------------------------------------------------------------
    private static Guid
        GetRequiredTenantId(
            Guid? tenantId)
    {
        if (!tenantId.HasValue ||
            tenantId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The authenticated user does not have a valid tenant.");
        }

        return tenantId.Value;
    }
}
