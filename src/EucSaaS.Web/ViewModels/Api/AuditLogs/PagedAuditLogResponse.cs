namespace EucSaaS.Web.ViewModels.Api.AuditLogs;

public class PagedAuditLogResponse
{
    public IReadOnlyList<AuditLogResponse> Items { get; set; } =
        Array.Empty<AuditLogResponse>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage =>
        PageNumber > 1;

    public bool HasNextPage =>
        PageNumber < TotalPages;
}
