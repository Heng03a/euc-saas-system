using EucSaaS.Domain.Common;

namespace EucSaaS.Domain.Entities;

public class AppRole : BaseEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    //-----------------------------------
    // Navigation Properties
    //-----------------------------------

    public Tenant? Tenant { get; set; }

public ICollection<DashboardWidgetPermission> DashboardWidgetPermissions
{
    get;
    set;
}
= new List<DashboardWidgetPermission>();

}
