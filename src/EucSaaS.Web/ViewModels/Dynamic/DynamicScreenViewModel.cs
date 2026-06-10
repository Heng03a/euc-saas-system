namespace EucSaaS.Web.ViewModels.Dynamic;



public class DynamicScreenViewModel
{
	public List<DynamicFormFieldViewModel> FormFields { get; set; } = new();

    public string ScreenCode { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public List<DynamicColumnViewModel> Columns { get; set; }
        = new();

    public List<Dictionary<string, object?>> Rows { get; set; }
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
