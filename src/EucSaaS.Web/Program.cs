using EucSaaS.Infrastructure.Data;
using EucSaaS.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using EucSaaS.Web.Security;
using EucSaaS.Web.Services;
using EucSaaS.Application.Interfaces;
using EucSaaS.Infrastructure.Services;
using EucSaaS.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<DynamicDataService>();

builder.Services.AddScoped<MenuService>();

builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<IDataSourceDiscoveryService, DataSourceDiscoveryService>();
builder.Services.AddScoped<IDataSourceSchemaReader, PostgreSqlSchemaReader>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IDataSourceSchemaReader, PostgreSqlSchemaReader>();

    builder.Services.AddScoped<IDataSourceDiscoveryService, DataSourceDiscoveryService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.AdminOnly, policy =>
    {
        policy.RequireRole(AppRoles.Admin);
    });

    options.AddPolicy(AppPolicies.ManagerOrAdmin, policy =>
    {
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager);
    });

    options.AddPolicy(AppPolicies.AuthenticatedOnly, policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy(AppPolicies.ReadAccess, policy =>
    {
        policy.RequireRole(
            AppRoles.Admin,
            AppRoles.Manager,
            AppRoles.User,
            AppRoles.ReadOnly
        );
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DatabaseSeeder.SeedAsync(db);
    await MetadataSeeder.SeedAsync(db);
    await ScreenMetadataSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
