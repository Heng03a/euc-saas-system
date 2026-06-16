namespace EucSaaS.Web.ViewModels.Dynamic;

public class DynamicScreenViewModel
{
    public string ScreenCode { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string ScreenMode { get; set; } = "Maintenance";

public string SortColumn { get; set; } = "";
public string SortDirection { get; set; } = "ASC";

    public bool IsMaintenanceMode =>
        ScreenMode.Equals("Maintenance", StringComparison.OrdinalIgnoreCase);

    public bool IsReportMode =>
        ScreenMode.Equals("Report", StringComparison.OrdinalIgnoreCase);

    public List<DynamicColumnViewModel> Columns { get; set; }
        = new();

    public List<DynamicFormFieldViewModel> FormFields { get; set; }
        = new();

    public List<Dictionary<string, object?>> Rows { get; set; }
        = new();

    public Dictionary<string, string> SearchValues { get; set; }
        = new();
}

public class DynamicColumnViewModel
{
    public string FieldName { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public string DataType { get; set; } = "text";

    public string Width { get; set; } = "150px";

    public bool IsSortable { get; set; }

    public bool IsSearchable { get; set; }

    public bool IsVisible { get; set; }
}
