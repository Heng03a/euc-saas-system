using System.Security.Claims;

namespace EucSaaS.Web.Services.Security;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        GetGuidClaim(
            ClaimTypes.NameIdentifier,
            "UserId",
            "AppUserId");

    public Guid? TenantId =>
        GetGuidClaim(
            "TenantId",
            "tenant_id",
            "tenant");

    public Guid? AppRoleId =>
        GetGuidClaim(
            "AppRoleId",
            "RoleId",
            "role_id");

    public string? UserName =>
        GetStringClaim(
            ClaimTypes.Name,
            "UserName",
            "username");

    public string? RoleName =>
        GetStringClaim(
            ClaimTypes.Role,
            "RoleName",
            "role");

    public string? Department =>
        GetStringClaim(
            "Department",
            "DepartmentCode",
            "department");

    public bool IsInRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        return User?.IsInRole(roleName) == true;
    }

    private string? GetStringClaim(params string[] claimTypes)
    {
        if (User == null)
        {
            return null;
        }

        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirst(claimType)?.Value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private Guid? GetGuidClaim(params string[] claimTypes)
    {
        var value = GetStringClaim(claimTypes);

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }
}
