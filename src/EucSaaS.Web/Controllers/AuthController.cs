using System.Security.Claims;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedUsername =
            model.Username?.Trim();

        var user = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Department)
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x =>
                x.Username == normalizedUsername);

        if (user == null ||
            !DatabaseSeeder.VerifyPassword(
                model.Password,
                user.PasswordHash))
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username or password.");

            return View(model);
        }

        if (user.Role == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The user does not have an assigned role.");

            return View(model);
        }

        if (user.Tenant == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The user does not have an assigned tenant.");

            return View(model);
        }

        var roleCode =
            user.Role.Code?.Trim() ??
            string.Empty;

        var departmentCode =
            user.Department?.Code?.Trim() ??
            string.Empty;

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                "FullName",
                user.FullName ?? user.Username),

            new Claim(
                "TenantId",
                user.TenantId.ToString()),

            new Claim(
                "AppRoleId",
                user.RoleId.ToString()),

            new Claim(
                ClaimTypes.Role,
                roleCode)
        };

        // DepartmentId is a non-nullable Guid.
        if (user.DepartmentId != Guid.Empty)
        {
            claims.Add(
                new Claim(
                    "DepartmentId",
                    user.DepartmentId.ToString()));
        }

        // Used by dashboard row-level security.
        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            claims.Add(
                new Claim(
                    "Department",
                    departmentCode.ToUpperInvariant()));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        var authenticationProperties =
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            };

        // Remove any old authentication cookie first.
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            principal,
            authenticationProperties);

        return RedirectToAction(
            "Index",
            "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            "Login",
            "Auth");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
