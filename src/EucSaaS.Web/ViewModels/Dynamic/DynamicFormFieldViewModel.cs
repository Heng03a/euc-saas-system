namespace EucSaaS.Web.ViewModels.Dynamic;

public class DynamicFormFieldViewModel
{
    public string FieldName { get; set; } = "";
    public string DisplayLabel { get; set; } = "";
    public string ControlType { get; set; } = "";
    public string DataType { get; set; } = "";

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public string? LookupCode { get; set; }

    public List<DynamicFormFieldOptionViewModel> Options { get; set; } = new();
}

public class DynamicFormFieldOptionViewModel
{
    public string OptionLabel { get; set; } = "";
    public string OptionValue { get; set; } = "";
    public int DisplayOrder { get; set; }
}
