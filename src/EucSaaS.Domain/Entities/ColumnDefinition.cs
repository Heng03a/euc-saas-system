namespace EucSaaS.Domain.Entities;

public class ColumnDefinition
{
    public string? Width { get; set; }
    
    public Guid Id { get; set; }

    public Guid ScreenDefinitionId { get; set; }
    public ScreenDefinition ScreenDefinition { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public string DataType { get; set; } = "text";

    public int DisplayOrder { get; set; }

    public bool IsVisible { get; set; } = true;
    public bool IsSortable { get; set; } = true;
    public bool IsSearchable { get; set; } = false;
}
