namespace EucSaaS.Web.ViewModels.Admin;

public class AdminListViewModel
{
    public string ScreenCode { get; set; } = "";

    public string ScreenName { get; set; } = "";

    public List<string> Columns { get; set; } = new();

    public List<Dictionary<string, object?>> Rows { get; set; } = new();

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalRecords / PageSize);

        public string? FilterField { get; set; }

    public string? FilterOperator { get; set; }

    public string? FilterValue { get; set; }
    
}
