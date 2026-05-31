namespace EucSaaS.Domain.Entities;

public class FormFieldOptionDefinition
{
    public Guid Id { get; set; }

    public Guid FormFieldDefinitionId { get; set; }
    public FormFieldDefinition FormFieldDefinition { get; set; } = null!;

    public string OptionLabel { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
