using EucSaaS.Domain.Entities;

namespace EucSaaS.Web.ViewModels.Dynamic;

public class DynamicEditViewModel
{
    public string ScreenCode { get; set; } = "";

    public string ScreenName { get; set; } = "";

    public Guid RecordId { get; set; }

public List<AuditLogViewModel> AuditLogs { get; set; } = new();

    public List<FormFieldDefinition> Fields
        { get; set; } = new();

    public Dictionary<string, object?> Values
        { get; set; } = new();
}

public class AuditLogViewModel
{
    public DateTime ChangedAt { get; set; }
    public string ActionType { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = "";
}
