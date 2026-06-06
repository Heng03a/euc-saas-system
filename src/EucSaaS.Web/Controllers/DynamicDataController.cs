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
}
