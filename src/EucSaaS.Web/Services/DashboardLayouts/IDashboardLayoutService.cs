using EucSaaS.Web.ViewModels.DashboardLayouts;

namespace EucSaaS.Web.Services.DashboardLayouts;

public interface IDashboardLayoutService
{
    Task<DashboardLayoutIndexViewModel>
        GetIndexAsync(
            Guid? tenantId,
            string? searchTerm,
            string? ownership,
            string? status,
            Guid? appRoleId,
            Guid? departmentId,
            string? sortBy,
            string? sortDirection,
            int page,
            int pageSize);

    Task<DashboardLayoutEditViewModel>
        GetCreateModelAsync(
            Guid? tenantId);

    Task<DashboardLayoutEditViewModel?>
        GetEditModelAsync(
            Guid id,
            Guid? tenantId);

    Task<DashboardLayoutDetailsViewModel?>
        GetDetailsAsync(
            Guid id,
            Guid? tenantId);

    Task<Guid>
        CreateAsync(
            DashboardLayoutEditViewModel model,
            Guid? tenantId,
            string username);

    Task<bool>
        UpdateAsync(
            DashboardLayoutEditViewModel model,
            Guid? tenantId,
            string username);

    Task<Guid?>
        CloneAsync(
            Guid id,
            Guid? tenantId,
            string username);

    Task<bool>
        SetDefaultAsync(
            Guid id,
            Guid? tenantId,
            string username);

    Task<bool>
        SetActiveAsync(
            Guid id,
            Guid? tenantId,
            bool isActive,
            string username);

    Task<bool>
        DeleteAsync(
            Guid id,
            Guid? tenantId);

    Task<bool>
        LayoutCodeExistsAsync(
            string layoutCode,
            Guid? tenantId,
            Guid? excludedId = null);
}
