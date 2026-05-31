namespace EucSaaS.Application.Interfaces;

public interface IDataSourceDiscoveryService
{
    Task<List<string>> DiscoverTablesAsync(string provider, string connectionString);
}
