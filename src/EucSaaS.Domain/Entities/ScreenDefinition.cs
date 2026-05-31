namespace EucSaaS.Domain.Entities;

public class ScreenDefinition
{
    public string TableName { get; set; } = string.Empty;
    public Guid Id { get; set; }

    public string ScreenCode { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string RoutePath { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
    public ICollection<FormFieldDefinition> FormFields { get; set; } = new List<FormFieldDefinition>();
    public ICollection<ScreenPermission> Permissions { get; set; } = new List<ScreenPermission>();
}
