namespace EucSaaS.Web.ViewModels.Api.AuditLogs;

public class AuditLogResponse
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScreenCode { get; set; } = "";

    public string RecordId { get; set; } = "";

    public string ActionType { get; set; } = "";

    public string FieldName { get; set; } = "";

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = "";

    public DateTime ChangedAt { get; set; }
}