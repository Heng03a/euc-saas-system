namespace EucSaaS.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }

    public string ScreenCode { get; set; } = "";

    public string RecordId { get; set; } = "";

    public string ActionType { get; set; } = "";

    public string FieldName { get; set; } = "";

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = "";

    public DateTime ChangedAt { get; set; }
}
