using EucSaaS.Application.Services;
using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DynamicDataController : Controller
{
    private readonly AppDbContext _context;
    private readonly DynamicDataService _dynamicDataService;

    public DynamicDataController(
        AppDbContext context,
        DynamicDataService dynamicDataService)
    {
        _context = context;
        _dynamicDataService = dynamicDataService;
    }

    public async Task<IActionResult> Index()
    {
        var dataSource = await _context.DataSources
            .FirstOrDefaultAsync(x => x.DataSourceCode == "LEGACY_HR_DB" && x.IsActive);

        if (dataSource == null)
        {
            ViewBag.ErrorMessage = "Active data source LEGACY_HR_DB was not found.";
            return View();
        }

        try
        {
            var table = await _dynamicDataService.GetTableDataAsync(
                dataSource,
                "Employees"
            );

            return View(table);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View();
        }
    }

public async Task<IActionResult> ConfigureEmployeesScreen()
{
    var dataSource = await _context.DataSources
        .FirstOrDefaultAsync(x => x.DataSourceCode == "LEGACY_HR_DB");

    if (dataSource == null)
    {
        return Content("LEGACY_HR_DB data source not found.");
    }

    var screen = await _context.ScreenDefinitions
        .FirstOrDefaultAsync(x => x.ScreenCode == "EMPLOYEES");

    if (screen == null)
    {
        return Content("EMPLOYEES screen definition not found.");
    }

    screen.DataSourceId = dataSource.Id;
    screen.SchemaName = "public";
    screen.TableName = "Employees";

    await _context.SaveChangesAsync();

    return Content("EMPLOYEES screen configured successfully.");
}
}
