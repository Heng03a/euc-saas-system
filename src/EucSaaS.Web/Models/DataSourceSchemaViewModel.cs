using EucSaaS.Application.DTOs;
using EucSaaS.Domain.Entities;

namespace EucSaaS.Web.Models;

public class DataSourceSchemaViewModel
{
    public DataSource DataSource { get; set; } = new();

    public List<DatabaseTableDto> Tables { get; set; } = new();
}
