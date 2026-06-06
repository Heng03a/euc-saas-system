namespace EucSaaS.Application.DTOs;

public class DatabaseTableDto
{
    public string SchemaName { get; set; } = "";
    public string TableName { get; set; } = "";
    public List<DatabaseColumnDto> Columns { get; set; } = new();
}

public class DatabaseColumnDto
{
    public string ColumnName { get; set; } = "";
    public string DataType { get; set; } = "";
    public bool IsNullable { get; set; }
    public int OrdinalPosition { get; set; }
}
