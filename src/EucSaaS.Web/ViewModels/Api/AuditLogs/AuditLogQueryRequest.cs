namespace EucSaaS.Web.ViewModels.Api.AuditLogs;

public class AuditLogQueryRequest
{
    public string? Search { get; set; }

    public string? ScreenCode { get; set; }

    public string? RecordId { get; set; }

    public string? ActionType { get; set; }

    public string? FieldName { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string SortBy { get; set; } = "ChangedAt";

    public string SortDirection { get; set; } = "desc";

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
