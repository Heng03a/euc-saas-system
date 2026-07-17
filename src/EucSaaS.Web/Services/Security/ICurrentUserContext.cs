namespace EucSaaS.Web.Services.Security;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? TenantId { get; }

    Guid? AppRoleId { get; }

    string? UserName { get; }

    string? RoleName { get; }

    string? Department { get; }

    bool IsInRole(string roleName);
}
