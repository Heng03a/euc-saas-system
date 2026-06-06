using EucSaaS.Application.Services;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class ScreenController : Controller
{
    private readonly AppDbContext _context;
    private readonly DynamicDataService _dynamicDataService;

    public ScreenController(
        AppDbContext context,
        DynamicDataService dynamicDataService)
    {
        _context = context;
        _dynamicDataService = dynamicDataService;
    }

    [HttpGet("/Screen/{screenCode}")]
    public async Task<IActionResult> Index(string screenCode)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.DataSource)
            .Include(x => x.Columns)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
        {
            return Content($"Screen definition '{screenCode}' was not found.");
        }

        if (screen.DataSource == null)
        {
            return Content($"Screen '{screenCode}' has no data source assigned.");
        }

        if (string.IsNullOrWhiteSpace(screen.TableName))
        {
            return Content($"Screen '{screenCode}' has no table name configured.");
        }

        var table = await _dynamicDataService.GetTableDataAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName
        );

        var rows = new List<Dictionary<string, object?>>();

        foreach (System.Data.DataRow dataRow in table.Rows)
        {
            var row = new Dictionary<string, object?>();

            foreach (System.Data.DataColumn dataColumn in table.Columns)
            {
                row[dataColumn.ColumnName] = dataRow[dataColumn];
            }

            rows.Add(row);
        }

        var model = new DynamicScreenViewModel
        {
            ScreenName = screen.ScreenName,
Columns = screen.Columns
    .OrderBy(x => x.DisplayOrder)
    .Select(x => new DynamicColumnViewModel
    {
        FieldName = x.FieldName,
        DisplayLabel = x.DisplayLabel,
        DataType = x.DataType,
        IsVisible = x.IsVisible,
        Width = x.Width
    })
    .ToList(),
            Rows = rows
        };

        return View(model);
    }
}
