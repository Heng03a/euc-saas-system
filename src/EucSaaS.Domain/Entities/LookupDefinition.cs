namespace EucSaaS.Domain.Entities;

public class LookupDefinition
{
    public Guid Id { get; set; }

    public string LookupCode { get; set; } = "";

    public string LookupName { get; set; } = "";

    public string SqlQuery { get; set; } = "";

    public bool IsActive { get; set; } = true;
}
