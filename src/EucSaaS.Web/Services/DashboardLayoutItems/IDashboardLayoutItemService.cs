using EucSaaS.Web.ViewModels.DashboardLayouts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.Services.DashboardLayoutItems;

public interface IDashboardLayoutItemService
{
    // ------------------------------------------------------------
    // Read
    // ------------------------------------------------------------
    Task<IReadOnlyList<DashboardLayoutItemViewModel>>
        GetItemsAsync(
            Guid dashboardLayoutId,
            Guid? tenantId);

    Task<DashboardLayoutItemViewModel?>
        GetItemAsync(
            Guid id,
            Guid? tenantId);

    // ------------------------------------------------------------
    // Widget lookup
    // ------------------------------------------------------------
    Task<IReadOnlyList<SelectListItem>>
        GetAvailableWidgetOptionsAsync(
            Guid dashboardLayoutId,
            Guid? tenantId,
            Guid? selectedWidgetDefinitionId = null);

    // ------------------------------------------------------------
    // Create
    // ------------------------------------------------------------
    Task<Guid>
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
            Guid? tenantId);

    // ------------------------------------------------------------
    // Update
    // ------------------------------------------------------------
    Task<bool>
        UpdateAsync(
            Guid id,
            int gridRow,
            int gridColumn,
            int gridWidth,
            int gridHeight,
            int displayOrder,
            bool isVisible,
            string? settingsJson,
            Guid? tenantId);

    // ------------------------------------------------------------
    // Visibility
    // ------------------------------------------------------------
    Task<bool>
        SetVisibilityAsync(
            Guid id,
            bool isVisible,
            Guid? tenantId);

    // ------------------------------------------------------------
    // Reorder
    // ------------------------------------------------------------
    Task<bool>
        MoveUpAsync(
            Guid id,
            Guid? tenantId);

    Task<bool>
        MoveDownAsync(
            Guid id,
            Guid? tenantId);

    // ------------------------------------------------------------
    // Delete
    // ------------------------------------------------------------
    Task<bool>
        DeleteAsync(
            Guid id,
            Guid? tenantId);
}
