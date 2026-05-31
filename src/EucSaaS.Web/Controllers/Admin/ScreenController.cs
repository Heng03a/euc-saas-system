using Dapper;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers.Admin;

[Route("Admin/Screen")]
public class ScreenController : Controller
{
    private readonly AppDbContext _dbContext;

    public ScreenController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{screenCode}")]
    public async Task<IActionResult> Index(string screenCode)
    {
        var screen = await _dbContext.ScreenDefinitions
            .FirstOrDefaultAsync(x =>
                x.ScreenCode == screenCode &&
                x.IsActive);

        if (screen == null)
            return NotFound();

        var columns = await _dbContext.ColumnDefinitions
            .Where(x => x.ScreenDefinitionId == screen.Id)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var connection = _dbContext.Database.GetDbConnection();

        var sql = $"select * from \"{screen.TableName}\"";

        var rows = await connection.QueryAsync(sql);

        var vm = new DynamicScreenViewModel
        {
            ScreenCode = screen.ScreenCode,
            ScreenName = screen.ScreenName,

            Columns = columns.Select(c => new DynamicColumnViewModel
            {
                FieldName = c.FieldName,
                DisplayLabel = c.DisplayLabel,
                DataType = c.DataType,
                Width = c.Width ?? "150px",
                IsSortable = c.IsSortable,
                IsSearchable = c.IsSearchable,
                IsVisible = c.IsVisible
            }).ToList(),

Rows = rows
    .Select(r =>
        ((IDictionary<string, object?>)r)
            .ToDictionary(x => x.Key, x => x.Value))
    .ToList()        };

        return View(vm);
    }
}
