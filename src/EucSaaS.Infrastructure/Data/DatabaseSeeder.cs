using System.Security.Cryptography;
using System.Text;
using EucSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var departmentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var adminRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var managerRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var userRoleId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var readOnlyRoleId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        var adminUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var managerUserId = Guid.Parse("88888888-8888-8888-8888-888888888888");

var standardUserId =
    Guid.Parse("99999999-9999-9999-9999-999999999999");

var readOnlyUserId =
    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        await SeedTenantAsync(db, tenantId);
        await SeedDepartmentAsync(db, departmentId, tenantId);

        await SeedRoleAsync(db, adminRoleId, tenantId, "Administrator", "ADMIN");
        await SeedRoleAsync(db, managerRoleId, tenantId, "Manager", "MANAGER");
        await SeedRoleAsync(db, userRoleId, tenantId, "User", "USER");
        await SeedRoleAsync(db, readOnlyRoleId, tenantId, "Read Only", "READONLY");

        await SeedUserAsync(
            db,
            adminUserId,
            tenantId,
            departmentId,
            adminRoleId,
            "Boss Admin",
            "boss@example.com",
            "boss",
            "1234556"
        );

        await SeedUserAsync(
            db,
            managerUserId,
            tenantId,
            departmentId,
            managerRoleId,
            "Manager User",
            "manager@example.com",
            "manager",
            "1234556"
        );

await SeedUserAsync(
    db,
    standardUserId,
    tenantId,
    departmentId,
    userRoleId,
    "Standard User",
    "user@example.com",
    "user",
    "1234556"
);

await SeedUserAsync(
    db,
    readOnlyUserId,
    tenantId,
    departmentId,
    readOnlyRoleId,
    "Read Only User",
    "readonly@example.com",
    "readonly",
    "1234556"
);


        await SeedMenuAsync(db, tenantId, "Dashboard", "/dashboard", "bi bi-speedometer2", 1);
        await SeedMenuAsync(db, tenantId, "Users", "/users", "bi bi-people", 2);
        await SeedMenuAsync(db, tenantId, "Menus", "/menus", "bi bi-list", 3);
        await SeedMenuAsync(db, tenantId, "Departments", "/departments", "bi bi-building", 4);

        await db.SaveChangesAsync();
    }

    private static async Task SeedTenantAsync(AppDbContext db, Guid tenantId)
    {
        if (await db.Tenants.AnyAsync(x => x.Id == tenantId))
        {
            return;
        }

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Default Tenant",
            Code = "DEFAULT"
        });
    }

    private static async Task SeedDepartmentAsync(AppDbContext db, Guid departmentId, Guid tenantId)
    {
        if (await db.Departments.AnyAsync(x => x.Id == departmentId))
        {
            return;
        }

        db.Departments.Add(new Department
        {
            Id = departmentId,
            TenantId = tenantId,
            Name = "Administration",
            Code = "ADMIN"
        });
    }

    private static async Task SeedRoleAsync(
        AppDbContext db,
        Guid roleId,
        Guid tenantId,
        string name,
        string code)
    {
        if (await db.AppRoles.AnyAsync(x => x.TenantId == tenantId && x.Code == code))
        {
            return;
        }

        db.AppRoles.Add(new AppRole
        {
            Id = roleId,
            TenantId = tenantId,
            Name = name,
            Code = code
        });
    }

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
        if (await db.AppUsers.AnyAsync(x => x.Id == userId || x.Username == username))
        {
            return;
        }

        db.AppUsers.Add(new AppUser
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

    private static async Task SeedMenuAsync(
        AppDbContext db,
        Guid tenantId,
        string name,
        string url,
        string icon,
        int displayOrder)
    {
        if (await db.AppMenus.AnyAsync(x => x.TenantId == tenantId && x.Url == url))
        {
            return;
        }

        db.AppMenus.Add(new AppMenu
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Url = url,
            Icon = icon,
            DisplayOrder = displayOrder
        });
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }
}
