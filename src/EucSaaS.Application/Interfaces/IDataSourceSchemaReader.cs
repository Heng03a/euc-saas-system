using EucSaaS.Application.DTOs;
using EucSaaS.Domain.Entities;

namespace EucSaaS.Application.Interfaces;

public interface IDataSourceSchemaReader
{
    Task<List<DatabaseTableDto>> ReadSchemaAsync(DataSource dataSource);
}
