using EucSaaS.Domain.Entities;

namespace EucSaaS.Web.ViewModels.Dynamic;

public class DynamicEditViewModel
{
    public string ScreenCode { get; set; } = "";

    public string ScreenName { get; set; } = "";

    public Guid RecordId { get; set; }

    public List<FormFieldDefinition> Fields
        { get; set; } = new();

    public Dictionary<string, object?> Values
        { get; set; } = new();
}
