using EucSaaS.Domain.Common;

namespace EucSaaS.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
