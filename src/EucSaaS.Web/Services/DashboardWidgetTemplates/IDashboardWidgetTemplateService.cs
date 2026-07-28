using EucSaaS.Web.ViewModels.DashboardWidgetTemplates;

namespace EucSaaS.Web.Services.DashboardWidgetTemplates;

public interface IDashboardWidgetTemplateService
{
    // ------------------------------------------------------------
    // Index
    // ------------------------------------------------------------
    Task<DashboardWidgetTemplateIndexViewModel>
        GetIndexAsync(
            string? search,
            string? category,
            string? status,
            string? ownership,
            string? sortBy,
            string? sortDirection,
            int page,
            int pageSize);

    // ------------------------------------------------------------
    // Details
    // ------------------------------------------------------------
    Task<DashboardWidgetTemplateDetailsViewModel?>
        GetDetailsAsync(
            Guid id);

    // ------------------------------------------------------------
    // Edit lookup
    // ------------------------------------------------------------
    Task<DashboardWidgetTemplateFormViewModel?>
        GetForEditAsync(
            Guid id);

    // ------------------------------------------------------------
    // Create
    // ------------------------------------------------------------
    Task<(bool Success, string? ErrorMessage)>
        CreateAsync(
            DashboardWidgetTemplateFormViewModel model);

    // ------------------------------------------------------------
    // Update
    // ------------------------------------------------------------
    Task<(bool Success, string? ErrorMessage)>
        UpdateAsync(
            DashboardWidgetTemplateFormViewModel model);

    // ------------------------------------------------------------
    // Toggle status
    // ------------------------------------------------------------
    Task<(bool Success, string? ErrorMessage)>
        ToggleStatusAsync(
            Guid id);

    // ------------------------------------------------------------
    // Clone
    // ------------------------------------------------------------
    Task<(
        bool Success,
        Guid? NewTemplateId,
        string? ErrorMessage)>
        CloneAsync(
            Guid id);

    // ------------------------------------------------------------
    // Delete
    // ------------------------------------------------------------
    Task<(bool Success, string? ErrorMessage)>
        DeleteAsync(
            Guid id);
}
