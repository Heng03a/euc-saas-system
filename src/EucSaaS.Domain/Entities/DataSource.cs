namespace EucSaaS.Domain.Entities;

public class DataSource
{
    public Guid Id { get; set; }

    public string DataSourceCode { get; set; } = "";

    public string DataSourceName { get; set; } = "";

    public string DatabaseType { get; set; } = "";

    public string HostName { get; set; } = "";

    public int PortNumber { get; set; }

    public string DatabaseName { get; set; } = "";

    public string ReadOnlyUserName { get; set; } = "";

    public string EncryptedPassword { get; set; } = "";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
