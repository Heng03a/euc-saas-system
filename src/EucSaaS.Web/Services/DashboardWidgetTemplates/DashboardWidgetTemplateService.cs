using EucSaaS.Application.Interfaces;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.DashboardWidgetTemplates;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Services.DashboardWidgetTemplates;

public class DashboardWidgetTemplateService
    : IDashboardWidgetTemplateService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public DashboardWidgetTemplateService(
        AppDbContext db,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    // ------------------------------------------------------------
    // Index
    // ------------------------------------------------------------
    public async Task<DashboardWidgetTemplateIndexViewModel>
        GetIndexAsync(
            string? search,
            string? category,
            string? status,
            string? ownership,
            string? sortBy,
            string? sortDirection,
            int page,
            int pageSize)
    {
        var tenantId = GetRequiredTenantId();

        search = search?.Trim();
        category = category?.Trim();
        status = status?.Trim();
        ownership = ownership?.Trim();

        sortBy = NormalizeSortBy(sortBy);
        sortDirection =
            NormalizeSortDirection(sortDirection);

        page = page < 1
            ? 1
            : page;

        pageSize = NormalizePageSize(pageSize);

        /*
         * A tenant may see:
         *
         * 1. Global system templates, where TenantId is null.
         * 2. Its own tenant custom templates.
         *
         * A tenant must never see templates belonging
         * to another tenant.
         */
        var accessibleTemplatesQuery =
            _db.DashboardWidgetTemplates
                .AsNoTracking()
                .Where(
                    x =>
                        x.TenantId == null ||
                        x.TenantId == tenantId);

        var query = accessibleTemplatesQuery;

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.ToLower();

            query = query.Where(
                x =>
                    x.TemplateCode
                        .ToLower()
                        .Contains(searchTerm) ||

                    x.TemplateName
                        .ToLower()
                        .Contains(searchTerm) ||

                    x.Category
                        .ToLower()
                        .Contains(searchTerm) ||

                    (
                        x.Description != null &&
                        x.Description
                            .ToLower()
                            .Contains(searchTerm)
                    ));
        }

        // --------------------------------------------------------
        // Category filter
        // --------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(
                x => x.Category == category);
        }

        // --------------------------------------------------------
        // Status filter
        // --------------------------------------------------------
        if (string.Equals(
                status,
                "active",
                StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(
                x => x.IsActive);
        }
        else if (string.Equals(
                     status,
                     "inactive",
                     StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(
                x => !x.IsActive);
        }

        // --------------------------------------------------------
        // Ownership filter
        // --------------------------------------------------------
        if (string.Equals(
                ownership,
                "system",
                StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(
                x =>
                    x.IsSystem &&
                    x.TenantId == null);
        }
        else if (string.Equals(
                     ownership,
                     "custom",
                     StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(
                x =>
                    !x.IsSystem &&
                    x.TenantId == tenantId);
        }

        // --------------------------------------------------------
        // Sorting
        // --------------------------------------------------------
        query = ApplySorting(
            query,
            sortBy,
            sortDirection);

        // --------------------------------------------------------
        // Total count
        // --------------------------------------------------------
        var totalRecords =
            await query.CountAsync();

        // --------------------------------------------------------
        // Correct page number when filters reduce results
        // --------------------------------------------------------
        if (pageSize > 0)
        {
            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalRecords /
                        (double)pageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }
        }

        // --------------------------------------------------------
        // Category filter options
        // --------------------------------------------------------
        var categories =
            await accessibleTemplatesQuery
                .Select(x => x.Category)
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

        // --------------------------------------------------------
        // Widget-type filter options
        // --------------------------------------------------------
        var widgetTypes =
            await accessibleTemplatesQuery
                .Select(
                    x => x.DefaultWidgetType)
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

        // --------------------------------------------------------
        // Paging
        // --------------------------------------------------------
        List<DashboardWidgetTemplate> records;

        if (pageSize == 0)
        {
            records =
                await query.ToListAsync();
        }
        else
        {
            records =
                await query
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        // --------------------------------------------------------
        // Return view model
        // --------------------------------------------------------
        return new DashboardWidgetTemplateIndexViewModel
        {
            Search = search,
            Category = category,
            Status = status,
            Ownership = ownership,

            SortBy = sortBy,
            SortDirection = sortDirection,

            Page = page,
            PageSize = pageSize,

            TotalRecords = totalRecords,

            Categories = categories,
            WidgetTypes = widgetTypes,

            Templates = records
                .Select(
                    x =>
                        new DashboardWidgetTemplateListItemViewModel
                        {
                            Id = x.Id,
                            TenantId = x.TenantId,

                            TemplateCode =
                                x.TemplateCode,

                            TemplateName =
                                x.TemplateName,

                            Category =
                                x.Category,

                            DefaultWidgetType =
                                x.DefaultWidgetType,

                            Description =
                                x.Description,

                            IsSystem =
                                x.IsSystem,

                            IsActive =
                                x.IsActive,

                            CreatedAt =
                                x.CreatedAt,

                            CreatedBy =
                                x.CreatedBy,

                            UpdatedAt =
                                x.UpdatedAt
                        })
                .ToList()
        };
    }

    // ------------------------------------------------------------
    // Details
    // ------------------------------------------------------------
    public async Task<DashboardWidgetTemplateDetailsViewModel?>
        GetDetailsAsync(
            Guid id)
    {
        var tenantId = GetRequiredTenantId();

        return await _db.DashboardWidgetTemplates
            .AsNoTracking()
            .Where(
                x =>
                    x.Id == id &&
                    (
                        x.TenantId == null ||
                        x.TenantId == tenantId
                    ))
            .Select(
                x =>
                    new DashboardWidgetTemplateDetailsViewModel
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,

                        TemplateCode =
                            x.TemplateCode,

                        TemplateName =
                            x.TemplateName,

                        Category =
                            x.Category,

                        Description =
                            x.Description,

                        DefaultWidgetType =
                            x.DefaultWidgetType,

                        DefaultSqlQuery =
                            x.DefaultSqlQuery,

                        DefaultIcon =
                            x.DefaultIcon,

                        DefaultColor =
                            x.DefaultColor,

                        DefaultGridWidth =
                            x.DefaultGridWidth,

                        DefaultGridHeight =
                            x.DefaultGridHeight,

                        IsSystem =
                            x.IsSystem,

                        IsActive =
                            x.IsActive,

                        CreatedBy =
                            x.CreatedBy,

                        CreatedAt =
                            x.CreatedAt,

                        UpdatedBy =
                            x.UpdatedBy,

                        UpdatedAt =
                            x.UpdatedAt
                    })
            .FirstOrDefaultAsync();
    }

    // ------------------------------------------------------------
    // Edit lookup
    // ------------------------------------------------------------
    public async Task<DashboardWidgetTemplateFormViewModel?>
        GetForEditAsync(
            Guid id)
    {
        var tenantId = GetRequiredTenantId();

        /*
         * System templates cannot be edited.
         * A tenant may edit only its own custom templates.
         */
        return await _db.DashboardWidgetTemplates
            .AsNoTracking()
            .Where(
                x =>
                    x.Id == id &&
                    !x.IsSystem &&
                    x.TenantId == tenantId)
            .Select(
                x =>
                    new DashboardWidgetTemplateFormViewModel
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,

                        TemplateCode =
                            x.TemplateCode,

                        TemplateName =
                            x.TemplateName,

                        Category =
                            x.Category,

                        Description =
                            x.Description,

                        DefaultWidgetType =
                            x.DefaultWidgetType,

                        DefaultSqlQuery =
                            x.DefaultSqlQuery,

                        DefaultIcon =
                            x.DefaultIcon,

                        DefaultColor =
                            x.DefaultColor,

                        DefaultGridWidth =
                            x.DefaultGridWidth,

                        DefaultGridHeight =
                            x.DefaultGridHeight,

                        IsSystem =
                            x.IsSystem,

                        IsActive =
                            x.IsActive
                    })
            .FirstOrDefaultAsync();
    }

    // ------------------------------------------------------------
    // Create
    // ------------------------------------------------------------
    public async Task<(bool Success, string? ErrorMessage)>
        CreateAsync(
            DashboardWidgetTemplateFormViewModel model)
    {
        var tenantId = GetRequiredTenantId();
        var username = GetCurrentUsername();

        NormalizeModel(model);

        var duplicateExists =
            await _db.DashboardWidgetTemplates
                .AnyAsync(
                    x =>
                        x.TenantId == tenantId &&
                        x.TemplateCode ==
                            model.TemplateCode);

        if (duplicateExists)
        {
            return (
                false,
                $"Template Code '{model.TemplateCode}' already exists for this tenant.");
        }

        var entity =
            new DashboardWidgetTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,

                TemplateCode =
                    model.TemplateCode,

                TemplateName =
                    model.TemplateName,

                Category =
                    model.Category,

                Description =
                    NormalizeNullable(
                        model.Description),

                DefaultWidgetType =
                    model.DefaultWidgetType,

                DefaultSqlQuery =
                    model.DefaultSqlQuery,

                DefaultIcon =
                    NormalizeNullable(
                        model.DefaultIcon),

                DefaultColor =
                    NormalizeNullable(
                        model.DefaultColor),

                DefaultGridWidth =
                    model.DefaultGridWidth,

                DefaultGridHeight =
                    model.DefaultGridHeight,

                IsSystem = false,
                IsActive = model.IsActive,

                CreatedBy = username,
                CreatedAt = DateTime.UtcNow
            };

        _db.DashboardWidgetTemplates.Add(entity);

        await _db.SaveChangesAsync();

        return (true, null);
    }

    // ------------------------------------------------------------
    // Update
    // ------------------------------------------------------------
    public async Task<(bool Success, string? ErrorMessage)>
        UpdateAsync(
            DashboardWidgetTemplateFormViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return (
                false,
                "Template ID is required.");
        }

        var tenantId = GetRequiredTenantId();
        var username = GetCurrentUsername();

        NormalizeModel(model);

        var entity =
            await _db.DashboardWidgetTemplates
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == model.Id.Value &&
                        !x.IsSystem &&
                        x.TenantId == tenantId);

        if (entity == null)
        {
            return (
                false,
                "Template was not found or cannot be edited.");
        }

        var duplicateExists =
            await _db.DashboardWidgetTemplates
                .AnyAsync(
                    x =>
                        x.Id != entity.Id &&
                        x.TenantId == tenantId &&
                        x.TemplateCode ==
                            model.TemplateCode);

        if (duplicateExists)
        {
            return (
                false,
                $"Template Code '{model.TemplateCode}' already exists for this tenant.");
        }

        entity.TemplateCode =
            model.TemplateCode;

        entity.TemplateName =
            model.TemplateName;

        entity.Category =
            model.Category;

        entity.Description =
            NormalizeNullable(
                model.Description);

        entity.DefaultWidgetType =
            model.DefaultWidgetType;

        entity.DefaultSqlQuery =
            model.DefaultSqlQuery;

        entity.DefaultIcon =
            NormalizeNullable(
                model.DefaultIcon);

        entity.DefaultColor =
            NormalizeNullable(
                model.DefaultColor);

        entity.DefaultGridWidth =
            model.DefaultGridWidth;

        entity.DefaultGridHeight =
            model.DefaultGridHeight;

        entity.IsActive =
            model.IsActive;

        entity.UpdatedBy =
            username;

        entity.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, null);
    }

    // ------------------------------------------------------------
    // Toggle status
    // ------------------------------------------------------------
    public async Task<(bool Success, string? ErrorMessage)>
        ToggleStatusAsync(
            Guid id)
    {
        var tenantId = GetRequiredTenantId();
        var username = GetCurrentUsername();

        var entity =
            await _db.DashboardWidgetTemplates
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId == tenantId);

        if (entity == null)
        {
            return (
                false,
                "Template was not found or its status cannot be changed.");
        }

        entity.IsActive =
            !entity.IsActive;

        entity.UpdatedBy =
            username;

        entity.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, null);
    }

// ------------------------------------------------------------
// Clone template
// ------------------------------------------------------------
public async Task<(
    bool Success,
    Guid? NewTemplateId,
    string? ErrorMessage)>
    CloneAsync(
        Guid id)
{
    var tenantId = GetRequiredTenantId();
    var username = GetCurrentUsername();

    /*
     * A tenant may clone:
     *
     * 1. A global system template.
     * 2. Its own tenant template.
     *
     * It must never clone another tenant's template.
     */
    var source =
        await _db.DashboardWidgetTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    (
                        x.TenantId == null ||
                        x.TenantId == tenantId
                    ));

    if (source == null)
    {
        return (
            false,
            null,
            "The source template was not found or cannot be cloned.");
    }

    var cloneCode =
        await GenerateUniqueCloneCodeAsync(
            source.TemplateCode,
            tenantId);

    var cloneName =
        await GenerateUniqueCloneNameAsync(
            source.TemplateName,
            tenantId);

    var clone =
        new DashboardWidgetTemplate
        {
            Id = Guid.NewGuid(),

            TenantId = tenantId,

            TemplateCode = cloneCode,

            TemplateName = cloneName,

            Category = source.Category,

            Description = source.Description,

            DefaultWidgetType =
                source.DefaultWidgetType,

            DefaultSqlQuery =
                source.DefaultSqlQuery,

            DefaultIcon =
                source.DefaultIcon,

            DefaultColor =
                source.DefaultColor,

            DefaultGridWidth =
                source.DefaultGridWidth,

            DefaultGridHeight =
                source.DefaultGridHeight,

            /*
             * The cloned record is always a tenant-owned
             * custom template, even when cloned from
             * a system template.
             */
            IsSystem = false,

            IsActive = true,

            CreatedBy = username,

            CreatedAt = DateTime.UtcNow,

            UpdatedBy = null,

            UpdatedAt = null
        };

    _db.DashboardWidgetTemplates.Add(clone);

    await _db.SaveChangesAsync();

    return (
        true,
        clone.Id,
        null);
}


    // ------------------------------------------------------------
    // Delete
    // ------------------------------------------------------------
    public async Task<(bool Success, string? ErrorMessage)>
        DeleteAsync(
            Guid id)
    {
        var tenantId = GetRequiredTenantId();

        var entity =
            await _db.DashboardWidgetTemplates
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        !x.IsSystem &&
                        x.TenantId == tenantId);

        if (entity == null)
        {
            return (
                false,
                "Template was not found or cannot be deleted.");
        }

        _db.DashboardWidgetTemplates.Remove(entity);

        await _db.SaveChangesAsync();

        return (true, null);
    }

    // ------------------------------------------------------------
    // Current tenant
    // ------------------------------------------------------------
    private Guid GetRequiredTenantId()
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

    // ------------------------------------------------------------
    // Current username
    // ------------------------------------------------------------
    private string GetCurrentUsername()
    {
        return string.IsNullOrWhiteSpace(
                _currentUserService.Username)
            ? "UNKNOWN"
            : _currentUserService.Username;
    }

    // ------------------------------------------------------------
    // Sorting
    // ------------------------------------------------------------
    private static IQueryable<DashboardWidgetTemplate>
        ApplySorting(
            IQueryable<DashboardWidgetTemplate> query,
            string sortBy,
            string sortDirection)
    {
        var descending =
            sortDirection == "desc";

        return sortBy switch
        {
            "templatecode" =>
                descending
                    ? query.OrderByDescending(
                        x => x.TemplateCode)
                    : query.OrderBy(
                        x => x.TemplateCode),

            "category" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.Category)
                        .ThenBy(
                            x => x.TemplateName)
                    : query
                        .OrderBy(
                            x => x.Category)
                        .ThenBy(
                            x => x.TemplateName),

            "widgettype" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.DefaultWidgetType)
                        .ThenBy(
                            x => x.TemplateName)
                    : query
                        .OrderBy(
                            x => x.DefaultWidgetType)
                        .ThenBy(
                            x => x.TemplateName),

            "status" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.IsActive)
                        .ThenBy(
                            x => x.TemplateName)
                    : query
                        .OrderBy(
                            x => x.IsActive)
                        .ThenBy(
                            x => x.TemplateName),

            "ownership" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.IsSystem)
                        .ThenBy(
                            x => x.TemplateName)
                    : query
                        .OrderBy(
                            x => x.IsSystem)
                        .ThenBy(
                            x => x.TemplateName),

            "createdat" =>
                descending
                    ? query.OrderByDescending(
                        x => x.CreatedAt)
                    : query.OrderBy(
                        x => x.CreatedAt),

            "updatedat" =>
                descending
                    ? query
                        .OrderByDescending(
                            x => x.UpdatedAt)
                        .ThenBy(
                            x => x.TemplateName)
                    : query
                        .OrderBy(
                            x => x.UpdatedAt)
                        .ThenBy(
                            x => x.TemplateName),

            _ =>
                descending
                    ? query.OrderByDescending(
                        x => x.TemplateName)
                    : query.OrderBy(
                        x => x.TemplateName)
        };
    }

    // ------------------------------------------------------------
    // Normalise sort column
    // ------------------------------------------------------------
    private static string NormalizeSortBy(
        string? sortBy)
    {
        var value =
            sortBy?
                .Trim()
                .ToLowerInvariant();

        return value switch
        {
            "templatecode" =>
                "templatecode",

            "templatename" =>
                "templatename",

            "category" =>
                "category",

            "widgettype" =>
                "widgettype",

            "status" =>
                "status",

            "ownership" =>
                "ownership",

            "createdat" =>
                "createdat",

            "updatedat" =>
                "updatedat",

            _ =>
                "templatename"
        };
    }

    // ------------------------------------------------------------
    // Normalise sort direction
    // ------------------------------------------------------------
    private static string NormalizeSortDirection(
        string? sortDirection)
    {
        return string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";
    }

    // ------------------------------------------------------------
    // Normalise page size
    // ------------------------------------------------------------
    private static int NormalizePageSize(
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
    // Normalise form model
    // ------------------------------------------------------------
    private static void NormalizeModel(
        DashboardWidgetTemplateFormViewModel model)
    {
        model.TemplateCode =
            model.TemplateCode
                .Trim()
                .ToUpperInvariant();

        model.TemplateName =
            model.TemplateName.Trim();

        model.Category =
            model.Category.Trim();

        model.DefaultWidgetType =
            model.DefaultWidgetType.Trim();

        model.DefaultSqlQuery =
            model.DefaultSqlQuery.Trim();
    }

    // ------------------------------------------------------------
    // Normalise optional string
    // ------------------------------------------------------------
    private static string? NormalizeNullable(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

// ------------------------------------------------------------
// Generate unique clone code
// ------------------------------------------------------------
private async Task<string>
    GenerateUniqueCloneCodeAsync(
        string sourceTemplateCode,
        Guid tenantId)
{
    const int maximumLength = 100;
    const string copySuffix = "_COPY";

    var baseCode =
        string.IsNullOrWhiteSpace(sourceTemplateCode)
            ? "TEMPLATE"
            : sourceTemplateCode
                .Trim()
                .ToUpperInvariant();

    /*
     * Reserve space for "_COPY" and a numeric suffix.
     */
    var maximumBaseLength =
        maximumLength -
        copySuffix.Length -
        10;

    if (baseCode.Length > maximumBaseLength)
    {
        baseCode =
            baseCode[..maximumBaseLength];
    }

    var candidate =
        $"{baseCode}{copySuffix}";

    var candidateExists =
        await _db.DashboardWidgetTemplates
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.TemplateCode == candidate);

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
            await _db.DashboardWidgetTemplates
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.TenantId == tenantId &&
                        x.TemplateCode == candidate);

        if (!candidateExists)
        {
            return candidate;
        }

        sequence++;
    }
}

// ------------------------------------------------------------
// Generate unique clone name
// ------------------------------------------------------------
private async Task<string>
    GenerateUniqueCloneNameAsync(
        string sourceTemplateName,
        Guid tenantId)
{
    const int maximumLength = 200;
    const string copySuffix = " Copy";

    var baseName =
        string.IsNullOrWhiteSpace(sourceTemplateName)
            ? "Template"
            : sourceTemplateName.Trim();

    /*
     * Reserve space for " Copy" and a numeric suffix.
     */
    var maximumBaseLength =
        maximumLength -
        copySuffix.Length -
        10;

    if (baseName.Length > maximumBaseLength)
    {
        baseName =
            baseName[..maximumBaseLength];
    }

    var candidate =
        $"{baseName}{copySuffix}";

    var candidateExists =
        await _db.DashboardWidgetTemplates
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.TemplateName == candidate);

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
            await _db.DashboardWidgetTemplates
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.TenantId == tenantId &&
                        x.TemplateName == candidate);

        if (!candidateExists)
        {
            return candidate;
        }

        sequence++;
    }
}

}
