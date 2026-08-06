using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.DashboardLayouts;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services.DashboardLayouts;

public class DashboardLayoutService
    : IDashboardLayoutService
{
    private readonly AppDbContext _db;

    public DashboardLayoutService(
        AppDbContext db)
    {
        _db = db;
    }

    // ------------------------------------------------------------
    // Index
    // ------------------------------------------------------------
    public async Task<DashboardLayoutIndexViewModel>
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
            int pageSize)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        searchTerm =
            NormalizeNullable(searchTerm);

        ownership =
            NormalizeNullable(ownership)?
                .ToLowerInvariant();

        status =
            NormalizeNullable(status)?
                .ToLowerInvariant();

        sortBy =
            NormalizeSortBy(sortBy);

        sortDirection =
            NormalizeSortDirection(
                sortDirection);

        page =
            page < 1
                ? 1
                : page;

        pageSize =
            NormalizePageSize(pageSize);

        /*
         * A tenant may see:
         *
         * 1. Global system layouts.
         * 2. Its own tenant layouts.
         *
         * Layouts belonging to another tenant
         * must never be returned.
         */
        var accessibleLayoutsQuery =
            _db.DashboardLayouts
                .AsNoTracking()
                .Where(
                    x =>
                        x.TenantId == null ||
                        x.TenantId ==
                            requiredTenantId);

        var query =
            accessibleLayoutsQuery;

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(
                searchTerm))
        {
            var loweredSearchTerm =
                searchTerm.ToLower();

            query =
                query.Where(
                    x =>
                        x.LayoutCode
                            .ToLower()
                            .Contains(
                                loweredSearchTerm) ||

                        x.LayoutName
                            .ToLower()
                            .Contains(
                                loweredSearchTerm) ||

                        (
                            x.Description != null &&
                            x.Description
                                .ToLower()
                                .Contains(
                                    loweredSearchTerm)
                        ));
        }

        // --------------------------------------------------------
        // Ownership filter
        // --------------------------------------------------------
        if (ownership == "system")
        {
            query =
                query.Where(
                    x =>
                        x.IsSystem &&
                        x.TenantId == null);
        }
        else if (
            ownership == "tenant" ||
            ownership == "custom")
        {
            query =
                query.Where(
                    x =>
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId);
        }

        // --------------------------------------------------------
        // Status filter
        // --------------------------------------------------------
        if (status == "active")
        {
            query =
                query.Where(
                    x => x.IsActive);
        }
        else if (status == "inactive")
        {
            query =
                query.Where(
                    x => !x.IsActive);
        }

        // --------------------------------------------------------
        // Role filter
        // --------------------------------------------------------
        if (appRoleId.HasValue)
        {
            query =
                query.Where(
                    x =>
                        x.AppRoleId ==
                            appRoleId.Value);
        }

        // --------------------------------------------------------
        // Department filter
        // --------------------------------------------------------
        if (departmentId.HasValue)
        {
            query =
                query.Where(
                    x =>
                        x.DepartmentId ==
                            departmentId.Value);
        }

        // --------------------------------------------------------
        // Sorting
        // --------------------------------------------------------
        query =
            ApplySorting(
                query,
                sortBy,
                sortDirection);

        // --------------------------------------------------------
        // Total count
        // --------------------------------------------------------
        var totalItems =
            await query.CountAsync();

        // --------------------------------------------------------
        // Correct page after filtering
        // --------------------------------------------------------
        if (pageSize > 0)
        {
            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalItems /
                        (double)pageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }
        }

        // --------------------------------------------------------
        // Paging
        // --------------------------------------------------------
        List<DashboardLayout> records;

        if (pageSize == 0)
        {
            records =
                await query
                    .Include(x => x.Items)
                    .ToListAsync();
        }
        else
        {
            records =
                await query
                    .Include(x => x.Items)
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        // --------------------------------------------------------
        // Lookup dictionaries
        // --------------------------------------------------------
        var roleIds =
            records
                .Where(
                    x =>
                        x.AppRoleId.HasValue)
                .Select(
                    x =>
                        x.AppRoleId!.Value)
                .Distinct()
                .ToList();

        var departmentIds =
            records
                .Where(
                    x =>
                        x.DepartmentId.HasValue)
                .Select(
                    x =>
                        x.DepartmentId!.Value)
                .Distinct()
                .ToList();

        var roleNames =
            await _db.AppRoles
                .AsNoTracking()
                .Where(
                    x =>
                        roleIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.Name);

        var departmentNames =
            await _db.Departments
                .AsNoTracking()
                .Where(
                    x =>
                        departmentIds.Contains(
                            x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.Name);

        // --------------------------------------------------------
        // Filter options
        // --------------------------------------------------------
        var roleOptions =
            await BuildRoleOptionsAsync(
                requiredTenantId);

        var departmentOptions =
            await BuildDepartmentOptionsAsync(
                requiredTenantId);

        return new DashboardLayoutIndexViewModel
        {
            SearchTerm =
                searchTerm,

            Ownership =
                ownership,

            Status =
                status,

            AppRoleId =
                appRoleId,

            DepartmentId =
                departmentId,

            SortBy =
                sortBy,

            SortDirection =
                sortDirection,

            Page =
                page,

            PageSize =
                pageSize,

            TotalItems =
                totalItems,

            RoleOptions =
                roleOptions,

            DepartmentOptions =
                departmentOptions,

            Layouts =
                records
                    .Select(
                        x =>
                            new DashboardLayoutListItemViewModel
                            {
                                Id =
                                    x.Id,

                                TenantId =
                                    x.TenantId,

                                AppRoleId =
                                    x.AppRoleId,

                                DepartmentId =
                                    x.DepartmentId,

                                LayoutCode =
                                    x.LayoutCode,

                                LayoutName =
                                    x.LayoutName,

                                Description =
                                    x.Description,

                                RoleName =
                                    GetLookupName(
                                        x.AppRoleId,
                                        roleNames),

                                DepartmentName =
                                    GetLookupName(
                                        x.DepartmentId,
                                        departmentNames),

                                IsSystem =
                                    x.IsSystem,

                                IsDefault =
                                    x.IsDefault,

                                IsShared =
                                    x.IsShared,

                                IsActive =
                                    x.IsActive,

                                ItemCount =
                                    x.Items.Count,

                                CreatedAt =
                                    x.CreatedAt,

                                CreatedBy =
                                    x.CreatedBy ?? "UNKNOWN",

                                UpdatedAt =
                                    x.UpdatedAt,

                                UpdatedBy =
                                    x.UpdatedBy
                            })
                    .ToList()
        };
    }

    // ------------------------------------------------------------
    // Create model
    // ------------------------------------------------------------
    public async Task<DashboardLayoutEditViewModel>
        GetCreateModelAsync(
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        return new DashboardLayoutEditViewModel
        {
            TenantId =
                requiredTenantId,

            IsSystem =
                false,

            IsDefault =
                false,

            IsShared =
                true,

            IsActive =
                true,

            RoleOptions =
                await BuildRoleOptionsAsync(
                    requiredTenantId),

            DepartmentOptions =
                await BuildDepartmentOptionsAsync(
                    requiredTenantId)
        };
    }

    // ------------------------------------------------------------
    // Edit model
    // ------------------------------------------------------------
    public async Task<DashboardLayoutEditViewModel?>
        GetEditModelAsync(
            Guid id,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        /*
         * System layouts cannot be edited.
         *
         * A tenant may edit only its own layout.
         */
        var model =
            await _db.DashboardLayouts
                .AsNoTracking()
                .Where(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId)
                .Select(
                    x =>
                        new DashboardLayoutEditViewModel
                        {
                            Id =
                                x.Id,

                            TenantId =
                                x.TenantId,

                            AppRoleId =
                                x.AppRoleId,

                            DepartmentId =
                                x.DepartmentId,

                            LayoutCode =
                                x.LayoutCode,

                            LayoutName =
                                x.LayoutName,

                            Description =
                                x.Description,

                            IsSystem =
                                x.IsSystem,

                            IsDefault =
                                x.IsDefault,

                            IsShared =
                                x.IsShared,

                            IsActive =
                                x.IsActive,

                            CreatedAt =
                                x.CreatedAt,

                            CreatedBy =
                                x.CreatedBy,

                            UpdatedAt =
                                x.UpdatedAt,

                            UpdatedBy =
                                x.UpdatedBy,

                            ItemCount =
                                x.Items.Count
                        })
                .FirstOrDefaultAsync();

        if (model == null)
        {
            return null;
        }

        model.RoleOptions =
            await BuildRoleOptionsAsync(
                requiredTenantId);

        model.DepartmentOptions =
            await BuildDepartmentOptionsAsync(
                requiredTenantId);

        return model;
    }

    // ------------------------------------------------------------
    // Details
    // ------------------------------------------------------------
    public async Task<DashboardLayoutDetailsViewModel?>
        GetDetailsAsync(
            Guid id,
            Guid? tenantId)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        var layout =
            await _db.DashboardLayouts
                .AsNoTracking()
                .Include(x => x.Items)
                    .ThenInclude(
                        x =>
                            x.DashboardWidgetDefinition)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        (
                            x.TenantId == null ||
                            x.TenantId ==
                                requiredTenantId
                        ));

        if (layout == null)
        {
            return null;
        }

        string? roleName = null;
        string? departmentName = null;

        if (layout.AppRoleId.HasValue)
        {
            roleName =
                await _db.AppRoles
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.Id ==
                                layout.AppRoleId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
        }

        if (layout.DepartmentId.HasValue)
        {
            departmentName =
                await _db.Departments
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.Id ==
                                layout.DepartmentId.Value)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
        }

        return new DashboardLayoutDetailsViewModel
        {
            Id =
                layout.Id,

            TenantId =
                layout.TenantId,

            LayoutCode =
                layout.LayoutCode,

            LayoutName =
                layout.LayoutName,

            Description =
                layout.Description,

            AppRoleId =
                layout.AppRoleId,

            RoleName =
                roleName,

            DepartmentId =
                layout.DepartmentId,

            DepartmentName =
                departmentName,

            IsSystem =
                layout.IsSystem,

            IsDefault =
                layout.IsDefault,

            IsShared =
                layout.IsShared,

            IsActive =
                layout.IsActive,

            CreatedAt =
                layout.CreatedAt,

            CreatedBy =
                layout.CreatedBy ?? "UNKNOWN",

            UpdatedAt =
                layout.UpdatedAt,

            UpdatedBy =
                layout.UpdatedBy,

            Items =
                layout.Items
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
                    .ToList()
        };
    }

    // ------------------------------------------------------------
    // Create
    // ------------------------------------------------------------
    public async Task<Guid>
        CreateAsync(
            DashboardLayoutEditViewModel model,
            Guid? tenantId,
            string username)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        NormalizeModel(model);

        username =
            NormalizeUsername(username);

        var duplicateExists =
            await LayoutCodeExistsAsync(
                model.LayoutCode,
                requiredTenantId);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Layout Code '{model.LayoutCode}' already exists for this tenant.");
        }

        await ValidateAssignmentAsync(
            model.AppRoleId,
            model.DepartmentId,
            requiredTenantId);

        var now =
            DateTime.UtcNow;

        var entity =
            new DashboardLayout
            {
                Id =
                    Guid.NewGuid(),

                TenantId =
                    requiredTenantId,

                AppRoleId =
                    model.AppRoleId,

                DepartmentId =
                    model.DepartmentId,

                LayoutCode =
                    model.LayoutCode,

                LayoutName =
                    model.LayoutName,

                Description =
                    NormalizeNullable(
                        model.Description),

                IsSystem =
                    false,

                IsDefault =
                    model.IsDefault,

                IsShared =
                    model.IsShared,

                IsActive =
                    model.IsActive,

                CreatedBy =
                    username,

                CreatedAt =
                    now,

                UpdatedBy =
                    null,

                UpdatedAt =
                    null
            };

        /*
         * Only one active default layout is allowed for
         * the same tenant, role and department assignment.
         */
        if (entity.IsDefault)
        {
            await ClearExistingDefaultAsync(
                requiredTenantId,
                entity.AppRoleId,
                entity.DepartmentId,
                null,
                username,
                now);
        }

        _db.DashboardLayouts.Add(entity);

        await _db.SaveChangesAsync();

        return entity.Id;
    }

    // ------------------------------------------------------------
    // Update
    // ------------------------------------------------------------
    public async Task<bool>
        UpdateAsync(
            DashboardLayoutEditViewModel model,
            Guid? tenantId,
            string username)
    {
        if (model.Id == Guid.Empty)
        {
            return false;
        }

        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        NormalizeModel(model);

        username =
            NormalizeUsername(username);

        var entity =
            await _db.DashboardLayouts
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == model.Id &&
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        var duplicateExists =
            await LayoutCodeExistsAsync(
                model.LayoutCode,
                requiredTenantId,
                entity.Id);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Layout Code '{model.LayoutCode}' already exists for this tenant.");
        }

        await ValidateAssignmentAsync(
            model.AppRoleId,
            model.DepartmentId,
            requiredTenantId);

        var now =
            DateTime.UtcNow;

        if (model.IsDefault)
        {
            await ClearExistingDefaultAsync(
                requiredTenantId,
                model.AppRoleId,
                model.DepartmentId,
                entity.Id,
                username,
                now);
        }

        entity.AppRoleId =
            model.AppRoleId;

        entity.DepartmentId =
            model.DepartmentId;

        entity.LayoutCode =
            model.LayoutCode;

        entity.LayoutName =
            model.LayoutName;

        entity.Description =
            NormalizeNullable(
                model.Description);

        entity.IsDefault =
            model.IsDefault;

        entity.IsShared =
            model.IsShared;

        entity.IsActive =
            model.IsActive;

        /*
         * An inactive layout cannot remain default.
         */
        if (!entity.IsActive)
        {
            entity.IsDefault = false;
        }

        entity.UpdatedBy =
            username;

        entity.UpdatedAt =
            now;

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Clone
    // ------------------------------------------------------------
    public async Task<Guid?>
        CloneAsync(
            Guid id,
            Guid? tenantId,
            string username)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        username =
            NormalizeUsername(username);

        /*
         * A tenant may clone:
         *
         * 1. A global system layout.
         * 2. Its own tenant layout.
         */
        var source =
            await _db.DashboardLayouts
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        (
                            x.TenantId == null ||
                            x.TenantId ==
                                requiredTenantId
                        ));

        if (source == null)
        {
            return null;
        }

        var cloneCode =
            await GenerateUniqueCloneCodeAsync(
                source.LayoutCode,
                requiredTenantId);

        var cloneName =
            await GenerateUniqueCloneNameAsync(
                source.LayoutName,
                requiredTenantId);

        var cloneId =
            Guid.NewGuid();

        var now =
            DateTime.UtcNow;

        var clone =
            new DashboardLayout
            {
                Id =
                    cloneId,

                TenantId =
                    requiredTenantId,

                AppRoleId =
                    source.AppRoleId,

                DepartmentId =
                    source.DepartmentId,

                LayoutCode =
                    cloneCode,

                LayoutName =
                    cloneName,

                Description =
                    source.Description,

                IsSystem =
                    false,

                IsDefault =
                    false,

                IsShared =
                    source.IsShared,

                IsActive =
                    true,

                CreatedBy =
                    username,

                CreatedAt =
                    now
            };

        foreach (
            var sourceItem in source.Items
                .OrderBy(
                    x =>
                        x.DisplayOrder))
        {
            clone.Items.Add(
                new DashboardLayoutItem
                {
                    Id =
                        Guid.NewGuid(),

                    DashboardLayoutId =
                        cloneId,

                    DashboardWidgetDefinitionId =
                        sourceItem
                            .DashboardWidgetDefinitionId,

                    GridRow =
                        sourceItem.GridRow,

                    GridColumn =
                        sourceItem.GridColumn,

                    GridWidth =
                        sourceItem.GridWidth,

                    GridHeight =
                        sourceItem.GridHeight,

                    DisplayOrder =
                        sourceItem.DisplayOrder,

                    IsVisible =
                        sourceItem.IsVisible,

                    SettingsJson =
                        sourceItem.SettingsJson
                });
        }

        _db.DashboardLayouts.Add(clone);

        await _db.SaveChangesAsync();

        return clone.Id;
    }

    // ------------------------------------------------------------
    // Set default
    // ------------------------------------------------------------
    public async Task<bool>
        SetDefaultAsync(
            Guid id,
            Guid? tenantId,
            string username)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        username =
            NormalizeUsername(username);

        /*
         * A system/global layout must not be modified
         * by a tenant.
         */
        var entity =
            await _db.DashboardLayouts
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        if (!entity.IsActive)
        {
            throw new InvalidOperationException(
                "An inactive layout cannot be made the default layout.");
        }

        var now =
            DateTime.UtcNow;

        await ClearExistingDefaultAsync(
            requiredTenantId,
            entity.AppRoleId,
            entity.DepartmentId,
            entity.Id,
            username,
            now);

        entity.IsDefault =
            true;

        entity.UpdatedBy =
            username;

        entity.UpdatedAt =
            now;

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Activate / deactivate
    // ------------------------------------------------------------
    public async Task<bool>
        SetActiveAsync(
            Guid id,
            Guid? tenantId,
            bool isActive,
            string username)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        username =
            NormalizeUsername(username);

        var entity =
            await _db.DashboardLayouts
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        entity.IsActive =
            isActive;

        /*
         * Deactivated layouts cannot remain default.
         */
        if (!isActive)
        {
            entity.IsDefault =
                false;
        }

        entity.UpdatedBy =
            username;

        entity.UpdatedAt =
            DateTime.UtcNow;

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
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        var entity =
            await _db.DashboardLayouts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId ==
                            requiredTenantId);

        if (entity == null)
        {
            return false;
        }

        /*
         * Layout items use cascade delete.
         */
        _db.DashboardLayouts.Remove(entity);

        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------
    // Duplicate layout-code check
    // ------------------------------------------------------------
    public async Task<bool>
        LayoutCodeExistsAsync(
            string layoutCode,
            Guid? tenantId,
            Guid? excludedId = null)
    {
        var requiredTenantId =
            GetRequiredTenantId(tenantId);

        var normalizedCode =
            NormalizeLayoutCode(
                layoutCode);

        return await _db.DashboardLayouts
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TenantId ==
                        requiredTenantId &&

                    x.LayoutCode ==
                        normalizedCode &&

                    (
                        !excludedId.HasValue ||
                        x.Id != excludedId.Value
                    ));
    }

    // ------------------------------------------------------------
    // Clear existing default
    // ------------------------------------------------------------
    private async Task
        ClearExistingDefaultAsync(
            Guid tenantId,
            Guid? appRoleId,
            Guid? departmentId,
            Guid? excludedLayoutId,
            string username,
            DateTime changedAt)
    {
        var existingDefaults =
            await _db.DashboardLayouts
                .Where(
                    x =>
                        x.TenantId ==
                            tenantId &&

                        x.IsDefault &&

                        x.AppRoleId ==
                            appRoleId &&

                        x.DepartmentId ==
                            departmentId &&

                        (
                            !excludedLayoutId.HasValue ||
                            x.Id !=
                                excludedLayoutId.Value
                        ))
                .ToListAsync();

        foreach (
            var existingDefault
            in existingDefaults)
        {
            existingDefault.IsDefault =
                false;

            existingDefault.UpdatedBy =
                username;

            existingDefault.UpdatedAt =
                changedAt;
        }
    }

    // ------------------------------------------------------------
    // Validate role and department
    // ------------------------------------------------------------
    private async Task
        ValidateAssignmentAsync(
            Guid? appRoleId,
            Guid? departmentId,
            Guid tenantId)
    {
        if (appRoleId.HasValue)
        {
            var roleExists =
                await _db.AppRoles
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.Id ==
                                appRoleId.Value &&
                            x.TenantId ==
                                tenantId);

            if (!roleExists)
            {
                throw new InvalidOperationException(
                    "The selected role is invalid for the current tenant.");
            }
        }

        if (departmentId.HasValue)
        {
            var departmentExists =
                await _db.Departments
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.Id ==
                                departmentId.Value &&
                            x.TenantId ==
                                tenantId);

            if (!departmentExists)
            {
                throw new InvalidOperationException(
                    "The selected department is invalid for the current tenant.");
            }
        }
    }

    // ------------------------------------------------------------
    // Role options
    // ------------------------------------------------------------
    private async Task<IReadOnlyList<SelectListItem>>
        BuildRoleOptionsAsync(
            Guid tenantId)
    {
        var roles =
            await _db.AppRoles
                .AsNoTracking()
                .Where(
                    x =>
                        x.TenantId ==
                            tenantId)
                .OrderBy(x => x.Name)
                .Select(
                    x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                .ToListAsync();

        roles.Insert(
            0,
            new SelectListItem
            {
                Value =
                    string.Empty,

                Text =
                    "All roles"
            });

        return roles;
    }

    // ------------------------------------------------------------
    // Department options
    // ------------------------------------------------------------
    private async Task<IReadOnlyList<SelectListItem>>
        BuildDepartmentOptionsAsync(
            Guid tenantId)
    {
        var departments =
            await _db.Departments
                .AsNoTracking()
                .Where(
                    x =>
                        x.TenantId ==
                            tenantId)
                .OrderBy(x => x.Name)
                .Select(
                    x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                .ToListAsync();

        departments.Insert(
            0,
            new SelectListItem
            {
                Value =
                    string.Empty,

                Text =
                    "All departments"
            });

        return departments;
    }

    // ------------------------------------------------------------
    // Sorting
    // ------------------------------------------------------------
    private static IQueryable<DashboardLayout>
        ApplySorting(
            IQueryable<DashboardLayout> query,
            string sortBy,
            string sortDirection)
    {
        var descending =
            sortDirection == "desc";

        return sortBy switch
        {
            "layoutcode" =>
                descending
                    ? query.OrderByDescending(
                        x => x.LayoutCode)
                    : query.OrderBy(
                        x => x.LayoutCode),

            "displayorder" =>
                descending
                    ? query
                        .OrderBy(
                            x => x.IsDefault)
                        .ThenByDescending(
                            x => x.LayoutName)
                    : query
                        .OrderByDescending(
                            x => x.IsDefault)
                        .ThenBy(
                            x => x.LayoutName),

            "itemcount" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.Items.Count)
                        .ThenBy(
                            x => x.LayoutName)
                    : query
                        .OrderBy(
                            x => x.Items.Count)
                        .ThenBy(
                            x => x.LayoutName),

            "lastchanged" =>
                descending
                    ? query
                        .OrderByDescending(
                            x =>
                                x.UpdatedAt ??
                                x.CreatedAt)
                        .ThenBy(
                            x => x.LayoutName)
                    : query
                        .OrderBy(
                            x =>
                                x.UpdatedAt ??
                                x.CreatedAt)
                        .ThenBy(
                            x => x.LayoutName),

            _ =>
                descending
                    ? query.OrderByDescending(
                        x => x.LayoutName)
                    : query.OrderBy(
                        x => x.LayoutName)
        };
    }

    // ------------------------------------------------------------
    // Normalize sorting
    // ------------------------------------------------------------
    private static string
        NormalizeSortBy(
            string? sortBy)
    {
        var value =
            sortBy?
                .Trim()
                .ToLowerInvariant();

        return value switch
        {
            "layoutcode" =>
                "layoutcode",

            "layoutname" =>
                "layoutname",

            "displayorder" =>
                "displayorder",

            "itemcount" =>
                "itemcount",

            "lastchanged" =>
                "lastchanged",

            _ =>
                "layoutname"
        };
    }

    private static string
        NormalizeSortDirection(
            string? sortDirection)
    {
        return string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";
    }

    private static int
        NormalizePageSize(
            int pageSize)
    {
        return pageSize switch
        {
            0 => 0,
            6 => 6,
            10 => 10,
            20 => 20,
            50 => 50,
            100 => 100,
            _ => 10
        };
    }

    // ------------------------------------------------------------
    // Normalize form model
    // ------------------------------------------------------------
    private static void
        NormalizeModel(
            DashboardLayoutEditViewModel model)
    {
        model.LayoutCode =
            NormalizeLayoutCode(
                model.LayoutCode);

        model.LayoutName =
            model.LayoutName.Trim();

        model.Description =
            NormalizeNullable(
                model.Description);

        /*
         * System ownership is controlled by the
         * application and database seeder.
         */
        model.IsSystem =
            false;

        /*
         * An inactive layout cannot be default.
         */
        if (!model.IsActive)
        {
            model.IsDefault =
                false;
        }
    }

    private static string
        NormalizeLayoutCode(
            string layoutCode)
    {
        return string.IsNullOrWhiteSpace(
                layoutCode)
            ? string.Empty
            : layoutCode
                .Trim()
                .ToUpperInvariant();
    }

    private static string?
        NormalizeNullable(
            string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? null
            : value.Trim();
    }

    private static string
        NormalizeUsername(
            string? username)
    {
        return string.IsNullOrWhiteSpace(
                username)
            ? "UNKNOWN"
            : username.Trim();
    }

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

    private static string?
        GetLookupName(
            Guid? id,
            IReadOnlyDictionary<Guid, string>
                values)
    {
        if (!id.HasValue)
        {
            return null;
        }

        return values.TryGetValue(
                id.Value,
                out var name)
            ? name
            : null;
    }

    // ------------------------------------------------------------
    // Generate clone code
    // ------------------------------------------------------------
    private async Task<string>
        GenerateUniqueCloneCodeAsync(
            string sourceLayoutCode,
            Guid tenantId)
    {
        const int maximumLength = 100;
        const string copySuffix = "_COPY";

        var baseCode =
            string.IsNullOrWhiteSpace(
                sourceLayoutCode)
                ? "LAYOUT"
                : sourceLayoutCode
                    .Trim()
                    .ToUpperInvariant();

        var maximumBaseLength =
            maximumLength -
            copySuffix.Length -
            10;

        if (baseCode.Length >
            maximumBaseLength)
        {
            baseCode =
                baseCode[
                    ..maximumBaseLength];
        }

        var candidate =
            $"{baseCode}{copySuffix}";

        var candidateExists =
            await _db.DashboardLayouts
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.TenantId ==
                            tenantId &&
                        x.LayoutCode ==
                            candidate);

        if (!candidateExists)
        {
            return candidate;
        }

        var sequence = 2;

        while (true)
        {
            candidate =
                $"{baseCode}{copySuffix}{sequence}";

            candidateExists =
                await _db.DashboardLayouts
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.TenantId ==
                                tenantId &&
                            x.LayoutCode ==
                                candidate);

            if (!candidateExists)
            {
                return candidate;
            }

            sequence++;
        }
    }

    // ------------------------------------------------------------
    // Generate clone name
    // ------------------------------------------------------------
    private async Task<string>
        GenerateUniqueCloneNameAsync(
            string sourceLayoutName,
            Guid tenantId)
    {
        const int maximumLength = 200;
        const string copySuffix = " Copy";

        var baseName =
            string.IsNullOrWhiteSpace(
                sourceLayoutName)
                ? "Dashboard Layout"
                : sourceLayoutName.Trim();

        var maximumBaseLength =
            maximumLength -
            copySuffix.Length -
            10;

        if (baseName.Length >
            maximumBaseLength)
        {
            baseName =
                baseName[
                    ..maximumBaseLength];
        }

        var candidate =
            $"{baseName}{copySuffix}";

        var candidateExists =
            await _db.DashboardLayouts
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.TenantId ==
                            tenantId &&
                        x.LayoutName ==
                            candidate);

        if (!candidateExists)
        {
            return candidate;
        }

        var sequence = 2;

        while (true)
        {
            candidate =
                $"{baseName}{copySuffix} {sequence}";

            candidateExists =
                await _db.DashboardLayouts
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.TenantId ==
                                tenantId &&
                            x.LayoutName ==
                                candidate);

            if (!candidateExists)
            {
                return candidate;
            }

            sequence++;
        }
    }
}
