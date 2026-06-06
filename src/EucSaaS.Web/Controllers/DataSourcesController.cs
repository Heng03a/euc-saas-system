using EucSaaS.Application.Interfaces;
using EucSaaS.Domain.Entities;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EucSaaS.Web.Controllers;

public class DataSourcesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IDataSourceDiscoveryService _discoveryService;
    private readonly IDataSourceSchemaReader _schemaReader;

    public DataSourcesController(
        AppDbContext context,
        IDataSourceDiscoveryService discoveryService,
        IDataSourceSchemaReader schemaReader)
    {
        _context = context;
        _discoveryService = discoveryService;
        _schemaReader = schemaReader;
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

    public async Task<IActionResult> TestConnection(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        var connectionString = BuildPostgreSqlConnectionString(dataSource);

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

    public async Task<IActionResult> DiscoverTables(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        var connectionString = BuildPostgreSqlConnectionString(dataSource);

        var tables = await _discoveryService.DiscoverTablesAsync(
            dataSource.DatabaseType,
            connectionString
        );

        ViewBag.DataSourceName = dataSource.DataSourceName;

        return View(tables);
    }

    public async Task<IActionResult> Schema(Guid id)
    {
        var dataSource = await _context.DataSources.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        try
        {
            var tables = await _schemaReader.ReadSchemaAsync(dataSource);

            var model = new DataSourceSchemaViewModel
            {
                DataSource = dataSource,
                Tables = tables
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Schema discovery failed: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportTable(
        Guid dataSourceId,
        string schemaName,
        string tableName)
    {
        var dataSource = await _context.DataSources.FindAsync(dataSourceId);

        if (dataSource == null)
        {
            return NotFound();
        }

        var tables = await _schemaReader.ReadSchemaAsync(dataSource);

        var selectedTable = tables.FirstOrDefault(x =>
            x.SchemaName == schemaName &&
            x.TableName == tableName);

        if (selectedTable == null)
        {
            TempData["ErrorMessage"] = "Selected table was not found in the data source.";
            return RedirectToAction(nameof(Schema), new { id = dataSourceId });
        }

        var screenCode = tableName.ToUpperInvariant();

        var existingScreen = await _context.ScreenDefinitions
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (existingScreen != null)
        {
            TempData["ErrorMessage"] = $"Screen '{screenCode}' already exists.";
            return RedirectToAction(nameof(Schema), new { id = dataSourceId });
        }

        var screenDefinition = new ScreenDefinition
        {
            Id = Guid.NewGuid(),
            ScreenCode = screenCode,
            ScreenName = tableName,
            TableName = tableName,
            IsActive = true
        };

        _context.ScreenDefinitions.Add(screenDefinition);

        var displayOrder = 1;

        foreach (var column in selectedTable.Columns)
        {
            var columnDefinition = new ColumnDefinition
            {
                Id = Guid.NewGuid(),
                ScreenDefinitionId = screenDefinition.Id,
                FieldName = column.ColumnName,
                DataType = column.DataType,
                DisplayOrder = displayOrder,
                IsVisible = true
            };

            _context.ColumnDefinitions.Add(columnDefinition);

            displayOrder++;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Table '{schemaName}.{tableName}' imported successfully.";

        return RedirectToAction(nameof(Schema), new { id = dataSourceId });
    }

    private static string BuildPostgreSqlConnectionString(DataSource dataSource)
    {
        return
            $"Host={dataSource.HostName};" +
            $"Port={dataSource.PortNumber};" +
            $"Database={dataSource.DatabaseName};" +
            $"Username={dataSource.ReadOnlyUserName};" +
            $"Password={dataSource.EncryptedPassword}";
    }
}
