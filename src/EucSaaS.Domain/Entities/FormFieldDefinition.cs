namespace EucSaaS.Domain.Entities;

public class FormFieldDefinition
{
    public Guid Id { get; set; }

    public Guid ScreenDefinitionId { get; set; }
    public ScreenDefinition ScreenDefinition { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;

    public string ControlType { get; set; } = "textbox";
    public string DataType { get; set; } = "text";

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool IsVisible { get; set; } = true;

    public int? MaxLength { get; set; }
    public string? Placeholder { get; set; }
    public ICollection<FormFieldOptionDefinition> Options { get; set; } = new List<FormFieldOptionDefinition>();

}
