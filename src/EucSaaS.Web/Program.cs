using EucSaaS.Application.Interfaces;
using EucSaaS.Application.Services;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Infrastructure.Data.Seed;
using EucSaaS.Infrastructure.Services;
using EucSaaS.Web.Security;
using EucSaaS.Web.Services;
using EucSaaS.Web.Services.Export;
using EucSaaS.Web.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// MVC and API
// ------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// ------------------------------------------------------------
// Swagger / OpenAPI
// ------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "EUC SaaS API",
        Description =
            "REST API for the EUC SaaS Enterprise Operational Enablement Platform."
    });

    // Include only routes beginning with /api/.
    // Normal MVC controllers are excluded from Swagger.
    options.DocInclusionPredicate((documentName, apiDescription) =>
    {
        var relativePath = apiDescription.RelativePath;

        return !string.IsNullOrWhiteSpace(relativePath) &&
               relativePath.StartsWith(
                   "api/",
                   StringComparison.OrdinalIgnoreCase);
    });

    // JWT Bearer authentication definition.
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Enter the JWT access token returned by POST /api/auth/login. " +
                "Paste the token only; do not type the word Bearer."
        });

    // Apply the Bearer scheme to Swagger API operations.
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document)] = []
        });
});

// ------------------------------------------------------------
// Database
// ------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ------------------------------------------------------------
// HTTP context and current-user services
// ------------------------------------------------------------
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

builder.Services.AddScoped<
    ICurrentUserContext,
    CurrentUserContext>();

builder.Services.AddScoped<
    IDataAccessScopeResolver,
    DataAccessScopeResolver>();

// ------------------------------------------------------------
// Application services
// ------------------------------------------------------------
builder.Services.AddScoped<DynamicDataService>();
builder.Services.AddScoped<MenuService>();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<DashboardQueryService>();
builder.Services.AddScoped<DashboardSqlBuilder>();
builder.Services.AddScoped<DashboardFilterService>();

builder.Services.AddScoped<
    IDataSourceDiscoveryService,
    DataSourceDiscoveryService>();

builder.Services.AddScoped<
    IDataSourceSchemaReader,
    PostgreSqlSchemaReader>();

builder.Services.AddScoped<
    IExcelExportService,
    ExcelExportService>();

// ------------------------------------------------------------
// JWT configuration
// ------------------------------------------------------------
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer configuration is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience configuration is missing.");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key configuration is missing.");

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 characters.");
}

// ------------------------------------------------------------
// Authentication
//
// Default:
//   MVC/browser authentication uses Cookies.
//
// Additional:
//   REST clients may authenticate with JWT Bearer tokens.
// ------------------------------------------------------------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultAuthenticateScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignOutScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })

    // Existing MVC cookie authentication
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/Auth/AccessDenied";

            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            // API requests must receive HTTP status codes,
            // not redirects to MVC pages.
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;

                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        })

    // JWT Bearer authentication for Angular, mobile apps,
    // Swagger authorization and other REST clients.
    .AddJwtBearer(
        JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),

                    ValidateLifetime = true,
                    RequireExpirationTime = true,

                    ClockSkew = TimeSpan.FromMinutes(1),

NameClaimType =
    System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType =
                        System.Security.Claims.ClaimTypes.Role
                };
        });

// ------------------------------------------------------------
// Authorization policies
// ------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.AdminOnly, policy =>
    {
        policy.RequireRole(AppRoles.Admin);
    });

    options.AddPolicy(AppPolicies.ManagerOrAdmin, policy =>
    {
        policy.RequireRole(
            AppRoles.Admin,
            AppRoles.Manager);
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
            AppRoles.ReadOnly);
    });
});

var app = builder.Build();

// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "EUC SaaS API v1");

        options.DocumentTitle =
            "EUC SaaS API Documentation";
    });
}

// ------------------------------------------------------------
// Database seeders
// ------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DatabaseSeeder.SeedAsync(db);
    await MetadataSeeder.SeedAsync(db);
    await ScreenMetadataSeeder.SeedAsync(db);
}

// ------------------------------------------------------------
// Production exception handling
// ------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ------------------------------------------------------------
// HTTP middleware pipeline
// ------------------------------------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Attribute-routed REST API controllers
app.MapControllers();

// Conventional MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
