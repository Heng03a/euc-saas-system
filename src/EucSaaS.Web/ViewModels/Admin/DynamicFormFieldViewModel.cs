using Microsoft.AspNetCore.Mvc.Rendering;

namespace EucSaaS.Web.ViewModels.Admin;

public class DynamicFormFieldViewModel
{
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public string? Value { get; set; }

    public List<SelectListItem> Options { get; set; } = new();
}
