using EucSaaS.Domain.Common;

namespace EucSaaS.Domain.Entities;

public class AppUser : BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid RoleId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    //-----------------------------------
    // Navigation Properties
    //-----------------------------------

    public Tenant? Tenant { get; set; }

    public Department? Department { get; set; }

    public AppRole? Role { get; set; }
}
