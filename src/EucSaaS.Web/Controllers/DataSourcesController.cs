using EucSaaS.Application.Interfaces;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DataSourcesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IDataSourceDiscoveryService _discoveryService;

    public DataSourcesController(
        AppDbContext context,
        IDataSourceDiscoveryService discoveryService)
    {
        _context = context;
        _discoveryService = discoveryService;
    }

    public async Task<IActionResult> Index()
    {
        var dataSources = await _context.DataSources
            .OrderBy(x => x.DataSourceName)
            .ToListAsync();

        return View(dataSources);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DataSource dataSource)
    {
        if (!ModelState.IsValid)
        {
            return View(dataSource);
        }

        dataSource.Id = Guid.NewGuid();

        _context.DataSources.Add(dataSource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        return View(dataSource);
    }

public async Task<IActionResult> TestConnection(Guid id)
{
    var dataSource = await _context.DataSources.FindAsync(id);

    if (dataSource == null)
    {
        return NotFound();
    }

    var connectionString =
        $"Host={dataSource.HostName};" +
        $"Port={dataSource.PortNumber};" +
        $"Database={dataSource.DatabaseName};" +
        $"Username={dataSource.ReadOnlyUserName};" +
        $"Password={dataSource.EncryptedPassword}";

    try
    {
        await _discoveryService.DiscoverTablesAsync(
            dataSource.DatabaseType,
            connectionString
        );

        TempData["SuccessMessage"] = "Connection successful.";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "Connection failed: " + ex.Message;
    }

    return RedirectToAction(nameof(Index));
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DataSource dataSource)
    {
        if (id != dataSource.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dataSource);
        }

        _context.DataSources.Update(dataSource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        return View(dataSource);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        return View(dataSource);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        _context.DataSources.Remove(dataSource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

public async Task<IActionResult> DiscoverTables(Guid id)
{
    var dataSource = await _context.DataSources.FindAsync(id);

    if (dataSource == null)
    {
        return NotFound();
    }

    var connectionString =
        $"Host={dataSource.HostName};" +
        $"Port={dataSource.PortNumber};" +
        $"Database={dataSource.DatabaseName};" +
        $"Username={dataSource.ReadOnlyUserName};" +
        $"Password={dataSource.EncryptedPassword}";

    var tables = await _discoveryService.DiscoverTablesAsync(
        dataSource.DatabaseType,
        connectionString
    );

    ViewBag.DataSourceName = dataSource.DataSourceName;

    return View(tables);
}
}
