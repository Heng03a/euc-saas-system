using System.Security.Cryptography;
using System.Text;
using EucSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Infrastructure.Data;

public static class DatabaseSeeder
{
    // ------------------------------------------------------------
    // Phase 14.1 — Fixed dashboard layout IDs
    // ------------------------------------------------------------
    private static readonly Guid SystemDefaultLayoutId =
        Guid.Parse(
            "90000000-0000-0000-0000-000000000001");

    private const string SystemDefaultLayoutCode =
        "SYSTEM_DEFAULT_DASHBOARD";

    public static async Task SeedAsync(
        AppDbContext db)
    {
        // Apply all pending EF Core migrations first.
        await db.Database.MigrateAsync();

        // ------------------------------------------------------------
        // Fixed seed IDs
        // ------------------------------------------------------------
        var tenantId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111111111");

        var departmentId =
            Guid.Parse(
                "22222222-2222-2222-2222-222222222222");

        var adminRoleId =
            Guid.Parse(
                "33333333-3333-3333-3333-333333333333");

        var managerRoleId =
            Guid.Parse(
                "55555555-5555-5555-5555-555555555555");

        var userRoleId =
            Guid.Parse(
                "66666666-6666-6666-6666-666666666666");

        var readOnlyRoleId =
            Guid.Parse(
                "77777777-7777-7777-7777-777777777777");

        var adminUserId =
            Guid.Parse(
                "44444444-4444-4444-4444-444444444444");

        var managerUserId =
            Guid.Parse(
                "88888888-8888-8888-8888-888888888888");

        var standardUserId =
            Guid.Parse(
                "99999999-9999-9999-9999-999999999999");

        var readOnlyUserId =
            Guid.Parse(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // ------------------------------------------------------------
        // Tenant and department
        // ------------------------------------------------------------
        await SeedTenantAsync(
            db,
            tenantId);

        await SeedDepartmentAsync(
            db,
            departmentId,
            tenantId);

        // ------------------------------------------------------------
        // Roles
        // ------------------------------------------------------------
        await SeedRoleAsync(
            db,
            adminRoleId,
            tenantId,
            "Administrator",
            "ADMIN");

        await SeedRoleAsync(
            db,
            managerRoleId,
            tenantId,
            "Manager",
            "MANAGER");

        await SeedRoleAsync(
            db,
            userRoleId,
            tenantId,
            "User",
            "USER");

        await SeedRoleAsync(
            db,
            readOnlyRoleId,
            tenantId,
            "Read Only",
            "READONLY");

        // ------------------------------------------------------------
        // Users
        // ------------------------------------------------------------
        await SeedUserAsync(
            db,
            adminUserId,
            tenantId,
            departmentId,
            adminRoleId,
            "Boss Admin",
            "boss@example.com",
            "boss",
            "1234556");

        await SeedUserAsync(
            db,
            managerUserId,
            tenantId,
            departmentId,
            managerRoleId,
            "Manager User",
            "manager@example.com",
            "manager",
            "1234556");

        await SeedUserAsync(
            db,
            standardUserId,
            tenantId,
            departmentId,
            userRoleId,
            "Standard User",
            "user@example.com",
            "user",
            "1234556");

        await SeedUserAsync(
            db,
            readOnlyUserId,
            tenantId,
            departmentId,
            readOnlyRoleId,
            "Read Only User",
            "readonly@example.com",
            "readonly",
            "1234556");

        // ------------------------------------------------------------
        // Menus
        // ------------------------------------------------------------
        await SeedMenuAsync(
            db,
            tenantId,
            "Dashboard",
            "/dashboard",
            "bi bi-speedometer2",
            1);

        await SeedMenuAsync(
            db,
            tenantId,
            "Users",
            "/users",
            "bi bi-people",
            2);

        await SeedMenuAsync(
            db,
            tenantId,
            "Menus",
            "/menus",
            "bi bi-list",
            3);

        await SeedMenuAsync(
            db,
            tenantId,
            "Departments",
            "/departments",
            "bi bi-building",
            4);

        /*
         * Save tenant, department, roles, users and menus before
         * executing the separate dashboard seed operations.
         */
        await db.SaveChangesAsync();

        // ------------------------------------------------------------
        // Phase 13.1 — Dashboard Widget Templates
        // ------------------------------------------------------------
        await SeedDashboardWidgetTemplatesAsync(db);

        // ------------------------------------------------------------
        // Phase 14.1 — Dashboard Layout Management
        // ------------------------------------------------------------
        /*
         * Dashboard widget definitions must already exist before
         * layout items can reference them.
         */
        await SeedDashboardLayoutsAsync(db);
    }

    // ------------------------------------------------------------
    // Tenant
    // ------------------------------------------------------------
    private static async Task SeedTenantAsync(
        AppDbContext db,
        Guid tenantId)
    {
        if (await db.Tenants.AnyAsync(
                x => x.Id == tenantId))
        {
            return;
        }

        db.Tenants.Add(
            new Tenant
            {
                Id = tenantId,
                Name = "Default Tenant",
                Code = "DEFAULT"
            });
    }

    // ------------------------------------------------------------
    // Department
    // ------------------------------------------------------------
    private static async Task SeedDepartmentAsync(
        AppDbContext db,
        Guid departmentId,
        Guid tenantId)
    {
        if (await db.Departments.AnyAsync(
                x => x.Id == departmentId))
        {
            return;
        }

        db.Departments.Add(
            new Department
            {
                Id = departmentId,
                TenantId = tenantId,
                Name = "Administration",
                Code = "ADMIN"
            });
    }

    // ------------------------------------------------------------
    // Role
    // ------------------------------------------------------------
    private static async Task SeedRoleAsync(
        AppDbContext db,
        Guid roleId,
        Guid tenantId,
        string name,
        string code)
    {
        var roleExists =
            await db.AppRoles.AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.Code == code);

        if (roleExists)
        {
            return;
        }

        db.AppRoles.Add(
            new AppRole
            {
                Id = roleId,
                TenantId = tenantId,
                Name = name,
                Code = code
            });
    }

    // ------------------------------------------------------------
    // User
    // ------------------------------------------------------------
    private static async Task SeedUserAsync(
        AppDbContext db,
        Guid userId,
        Guid tenantId,
        Guid departmentId,
        Guid roleId,
        string fullName,
        string email,
        string username,
        string password)
    {
        var userExists =
            await db.AppUsers.AnyAsync(
                x =>
                    x.Id == userId ||
                    x.Username == username);

        if (userExists)
        {
            return;
        }

        db.AppUsers.Add(
            new AppUser
            {
                Id = userId,
                TenantId = tenantId,
                DepartmentId = departmentId,
                RoleId = roleId,
                FullName = fullName,
                Email = email,
                Username = username,
                PasswordHash = HashPassword(password)
            });
    }

    // ------------------------------------------------------------
    // Menu
    // ------------------------------------------------------------
    private static async Task SeedMenuAsync(
        AppDbContext db,
        Guid tenantId,
        string name,
        string url,
        string icon,
        int displayOrder)
    {
        var menuExists =
            await db.AppMenus.AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.Url == url);

        if (menuExists)
        {
            return;
        }

        db.AppMenus.Add(
            new AppMenu
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Url = url,
                Icon = icon,
                DisplayOrder = displayOrder
            });
    }

    // ------------------------------------------------------------
    // Phase 13.1 — Dashboard Widget Template Library
    // ------------------------------------------------------------
    private static async Task SeedDashboardWidgetTemplatesAsync(
        AppDbContext db)
    {
        var createdAt = DateTime.UtcNow;

        var systemTemplates =
            new List<DashboardWidgetTemplate>
            {
                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000001"),

                    TenantId = null,

                    TemplateCode = "EMPLOYEE_COUNT",

                    TemplateName = "Employee Count",

                    Category = "Employees",

                    Description =
                        "Displays the total number of employees for the current tenant.",

                    DefaultWidgetType = "Card",

                    DefaultSqlQuery = """
                        SELECT
                            COUNT(*) AS "Value"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId;
                        """,

                    DefaultIcon = "bi-people-fill",

                    DefaultColor = "primary",

                    DefaultGridWidth = 3,

                    DefaultGridHeight = 2,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                },

                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000002"),

                    TenantId = null,

                    TemplateCode = "ACTIVE_EMPLOYEE_COUNT",

                    TemplateName = "Active Employee Count",

                    Category = "Employees",

                    Description =
                        "Displays the total number of active employees for the current tenant.",

                    DefaultWidgetType = "Card",

                    DefaultSqlQuery = """
                        SELECT
                            COUNT(*) AS "Value"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId
                          AND "Status" = 'Active';
                        """,

                    DefaultIcon = "bi-person-check-fill",

                    DefaultColor = "success",

                    DefaultGridWidth = 3,

                    DefaultGridHeight = 2,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                },

                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000003"),

                    TenantId = null,

                    TemplateCode = "EMPLOYEES_BY_STATUS",

                    TemplateName = "Employees by Status",

                    Category = "Employees",

                    Description =
                        "Groups employees by employment status.",

                    DefaultWidgetType = "Doughnut",

                    DefaultSqlQuery = """
                        SELECT
                            COALESCE(
                                "Status",
                                'Unknown'
                            ) AS "Label",
                            COUNT(*) AS "Value"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId
                        GROUP BY "Status"
                        ORDER BY "Value" DESC;
                        """,

                    DefaultIcon = "bi-pie-chart-fill",

                    DefaultColor = "info",

                    DefaultGridWidth = 6,

                    DefaultGridHeight = 4,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                },

                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000004"),

                    TenantId = null,

                    TemplateCode = "EMPLOYEES_BY_DEPARTMENT",

                    TemplateName = "Employees by Department",

                    Category = "Employees",

                    Description =
                        "Groups employees by department.",

                    DefaultWidgetType = "Bar",

                    DefaultSqlQuery = """
                        SELECT
                            COALESCE(
                                "Department",
                                'Unassigned'
                            ) AS "Label",
                            COUNT(*) AS "Value"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId
                        GROUP BY "Department"
                        ORDER BY "Value" DESC;
                        """,

                    DefaultIcon = "bi-bar-chart-fill",

                    DefaultColor = "primary",

                    DefaultGridWidth = 6,

                    DefaultGridHeight = 4,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                },

                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000005"),

                    TenantId = null,

                    TemplateCode = "EMPLOYEE_DIRECTORY",

                    TemplateName = "Employee Directory",

                    Category = "Employees",

                    Description =
                        "Displays an employee table containing employee code, name, department and status.",

                    DefaultWidgetType = "Table",

                    DefaultSqlQuery = """
                        SELECT
                            "EmployeeCode",
                            "FullName",
                            "Department",
                            "JobTitle",
                            "Email",
                            "Status"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId
                        ORDER BY "FullName";
                        """,

                    DefaultIcon = "bi-table",

                    DefaultColor = "secondary",

                    DefaultGridWidth = 12,

                    DefaultGridHeight = 5,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                },

                new()
                {
                    Id = Guid.Parse(
                        "81000000-0000-0000-0000-000000000006"),

                    TenantId = null,

                    TemplateCode = "FILTERED_EMPLOYEE_COUNT",

                    TemplateName = "Filtered Employee Count",

                    Category = "Employees",

                    Description =
                        "Displays the employee count using the dashboard Department and Status filters.",

                    DefaultWidgetType = "Card",

                    DefaultSqlQuery = """
                        SELECT
                            COUNT(*) AS "Value"
                        FROM "Employees"
                        WHERE "TenantId" = @TenantId
                          AND (
                              @Department IS NULL
                              OR @Department = ''
                              OR "Department" = @Department
                          )
                          AND (
                              @Status IS NULL
                              OR @Status = ''
                              OR "Status" = @Status
                          );
                        """,

                    DefaultIcon = "bi-funnel-fill",

                    DefaultColor = "warning",

                    DefaultGridWidth = 3,

                    DefaultGridHeight = 2,

                    IsSystem = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = createdAt
                }
            };

        var systemTemplateCodes =
            systemTemplates
                .Select(x => x.TemplateCode)
                .ToList();

        var existingTemplateCodes =
            await db.DashboardWidgetTemplates
                .AsNoTracking()
                .Where(
                    x =>
                        x.TenantId == null &&
                        systemTemplateCodes.Contains(
                            x.TemplateCode))
                .Select(x => x.TemplateCode)
                .ToListAsync();

        var existingCodeSet =
            existingTemplateCodes.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        var templatesToAdd =
            systemTemplates
                .Where(
                    x =>
                        !existingCodeSet.Contains(
                            x.TemplateCode))
                .ToList();

        if (templatesToAdd.Count == 0)
        {
            return;
        }

        await db.DashboardWidgetTemplates.AddRangeAsync(
            templatesToAdd);

        await db.SaveChangesAsync();
    }

    // ------------------------------------------------------------
    // Phase 14.1 — Dashboard Layout Management
    // ------------------------------------------------------------
    private static async Task SeedDashboardLayoutsAsync(
        AppDbContext db)
    {
        var systemLayout =
            await db.DashboardLayouts
                .FirstOrDefaultAsync(
                    x => x.Id == SystemDefaultLayoutId);

        if (systemLayout is null)
        {
            systemLayout =
                new DashboardLayout
                {
                    Id = SystemDefaultLayoutId,

                    TenantId = null,

                    AppRoleId = null,

                    DepartmentId = null,

                    LayoutCode =
                        SystemDefaultLayoutCode,

                    LayoutName =
                        "System Default Dashboard",

                    Description =
                        "Default system dashboard layout created from the existing widget positions.",

                    IsSystem = true,

                    IsDefault = true,

                    IsShared = true,

                    IsActive = true,

                    CreatedBy = "SYSTEM",

                    CreatedAt = DateTime.UtcNow
                };

            await db.DashboardLayouts.AddAsync(
                systemLayout);

            await db.SaveChangesAsync();
        }

        var existingWidgetIds =
            await db.DashboardLayoutItems
                .AsNoTracking()
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                        SystemDefaultLayoutId)
                .Select(
                    x =>
                        x.DashboardWidgetDefinitionId)
                .ToListAsync();

        var existingWidgetIdSet =
            existingWidgetIds.ToHashSet();

        var widgets =
            await db.DashboardWidgetDefinitions
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.WidgetCode)
                .ToListAsync();

        var currentMaximumDisplayOrder =
            await db.DashboardLayoutItems
                .Where(
                    x =>
                        x.DashboardLayoutId ==
                        SystemDefaultLayoutId)
                .Select(x => (int?)x.DisplayOrder)
                .MaxAsync() ?? 0;

        var nextDisplayOrder =
            currentMaximumDisplayOrder;

        var layoutItemsToAdd =
            new List<DashboardLayoutItem>();

        foreach (var widget in widgets)
        {
            if (existingWidgetIdSet.Contains(
                    widget.Id))
            {
                continue;
            }

            nextDisplayOrder++;

            var displayOrder =
                widget.DisplayOrder > 0
                    ? widget.DisplayOrder
                    : nextDisplayOrder;

            layoutItemsToAdd.Add(
                new DashboardLayoutItem
                {
                    Id = Guid.NewGuid(),

                    DashboardLayoutId =
                        SystemDefaultLayoutId,

                    DashboardWidgetDefinitionId =
                        widget.Id,

                    GridRow =
                        NormalizeGridRow(
                            widget.GridRow),

                    GridColumn =
                        NormalizeGridColumn(
                            widget.GridColumn),

                    GridWidth =
                        NormalizeGridWidth(
                            widget.GridWidth),

                    GridHeight =
                        NormalizeGridHeight(
                            widget.GridHeight),

                    DisplayOrder =
                        displayOrder,

                    IsVisible = true,

                    SettingsJson = null
                });
        }

        if (layoutItemsToAdd.Count == 0)
        {
            return;
        }

        await db.DashboardLayoutItems.AddRangeAsync(
            layoutItemsToAdd);

        await db.SaveChangesAsync();
    }

    // ------------------------------------------------------------
    // Phase 14.1 — Grid normalization helpers
    // ------------------------------------------------------------
    private static int NormalizeGridRow(
        int value)
    {
        return value < 1
            ? 1
            : value;
    }

    private static int NormalizeGridColumn(
        int value)
    {
        return value is < 1 or > 12
            ? 1
            : value;
    }

    private static int NormalizeGridWidth(
        int value)
    {
        return value is < 1 or > 12
            ? 4
            : value;
    }

    private static int NormalizeGridHeight(
        int value)
    {
        return value < 1
            ? 2
            : value;
    }

    // ------------------------------------------------------------
    // Password hashing
    // ------------------------------------------------------------
    private static string HashPassword(
        string password)
    {
        var passwordBytes =
            Encoding.UTF8.GetBytes(password);

        var hashBytes =
            SHA256.HashData(passwordBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static bool VerifyPassword(
        string password,
        string passwordHash)
    {
        return HashPassword(password) ==
               passwordHash;
    }
}


