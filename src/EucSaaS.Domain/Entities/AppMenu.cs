using EucSaaS.Domain.Common;

namespace EucSaaS.Domain.Entities;

public class AppMenu : BaseEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    //-----------------------------------
    // Navigation Properties
    //-----------------------------------

    public Tenant? Tenant { get; set; }
}
