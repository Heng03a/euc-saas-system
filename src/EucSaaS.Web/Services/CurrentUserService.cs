using System.Security.Claims;
using EucSaaS.Application.Interfaces;

namespace EucSaaS.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

 public Guid UserId
{
    get
    {
        var value = _httpContextAccessor.HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        return Guid.TryParse(value, out var id)
            ? id
            : Guid.Empty;
    }
}

public Guid TenantId
{
    get
    {
        var value = _httpContextAccessor.HttpContext?
            .User?
            .FindFirst("TenantId")?
            .Value;

        return Guid.TryParse(value, out var id)
            ? id
            : new Guid("11111111-1111-1111-1111-111111111111");
    }
}

    public string Username =>
        _httpContextAccessor.HttpContext?
            .User?
            .Identity?
            .Name ?? "";

    public string Role =>
        _httpContextAccessor.HttpContext?
            .User?
            .FindFirst(ClaimTypes.Role)?
            .Value ?? "";
}

