namespace EucSaaS.Web.Services.Security;

public class DataAccessScope
{
    public Guid TenantId { get; init; }

    public Guid? UserId { get; init; }

    public Guid? AppRoleId { get; init; }

    public string? RoleName { get; init; }

    public string? Department { get; init; }

    public bool CanAccessAllDepartments { get; init; }

    public bool IsDepartmentRestricted =>
        !CanAccessAllDepartments;
}
