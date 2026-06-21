namespace EucSaaS.Domain.Entities;

public class FormFieldDefinition
{
    public Guid Id { get; set; }

    public Guid ScreenDefinitionId { get; set; }
    public ScreenDefinition ScreenDefinition { get; set; } = null!;

    public string FieldName { get; set; } = "";
    public string DisplayLabel { get; set; } = "";
    public string ControlType { get; set; } = "text";
    public string DataType { get; set; } = "string";

    public string? Placeholder { get; set; }

    public ICollection<FormFieldOptionDefinition> Options { get; set; }
        = new List<FormFieldOptionDefinition>();

    public string? LookupCode { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsReadOnly { get; set; } = false;

    public bool IsRequired { get; set; } = false;
    public bool IsUnique { get; set; } = false;

    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public string? ValidationRegex { get; set; }
}
