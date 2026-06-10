namespace EucSaaS.Domain.Entities;

public class ScreenDefinition
{
    public Guid Id { get; set; }

    public string ScreenCode { get; set; } = "";

    public string ScreenName { get; set; } = "";

    public string EntityName { get; set; } = "";

    public string RoutePath { get; set; } = "";

    public string Description { get; set; } = "";

    public string SchemaName { get; set; } = "";

    public string TableName { get; set; } = "";

    public string PrimaryKeyColumn { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public Guid? DataSourceId { get; set; }

    public DataSource? DataSource { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ColumnDefinition> Columns { get; set; }
        = new List<ColumnDefinition>();

    public ICollection<FormFieldDefinition> FormFields { get; set; }
        = new List<FormFieldDefinition>();

    public ICollection<ScreenPermission> Permissions { get; set; }
        = new List<ScreenPermission>();
}
