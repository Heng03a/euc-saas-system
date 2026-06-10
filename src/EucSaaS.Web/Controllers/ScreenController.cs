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
            .Include(x => x.FormFields)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        if (screen.DataSource == null)
            return Content($"Screen '{screenCode}' has no data source assigned.");

        if (string.IsNullOrWhiteSpace(screen.SchemaName))
            return Content($"Screen '{screenCode}' has no schema name configured.");

        if (string.IsNullOrWhiteSpace(screen.TableName))
            return Content($"Screen '{screenCode}' has no table name configured.");

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
                row[dataColumn.ColumnName] =
                    dataRow[dataColumn] == DBNull.Value
                        ? null
                        : dataRow[dataColumn];
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
                    Width = x.Width ?? ""
                })
                .ToList(),

            FormFields = screen.FormFields
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new DynamicFormFieldViewModel
                {
                    FieldName = x.FieldName,
                    DisplayLabel = x.DisplayLabel,
                    ControlType = x.ControlType,
                    DataType = x.DataType,
                    IsRequired = x.IsRequired,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList(),

            Rows = rows
        };

        return View(model);
    }

    [HttpGet("/Screen/{screenCode}/Edit/{id}")]
    public async Task<IActionResult> Edit(
        string screenCode,
        Guid id)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.DataSource)
            .Include(x => x.FormFields)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        if (screen.DataSource == null)
            return Content($"Screen '{screenCode}' has no data source assigned.");

        if (string.IsNullOrWhiteSpace(screen.SchemaName))
            return Content($"Screen '{screenCode}' has no schema name configured.");

        if (string.IsNullOrWhiteSpace(screen.TableName))
            return Content($"Screen '{screenCode}' has no table name configured.");

        if (string.IsNullOrWhiteSpace(screen.PrimaryKeyColumn))
            return Content($"Screen '{screenCode}' has no primary key column configured.");

        foreach (var field in screen.FormFields)
        {
            await _context.Entry(field)
                .Collection(x => x.Options)
                .LoadAsync();
        }

        var record = await _dynamicDataService.GetRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id
        );

        if (record == null)
            return Content($"Record '{id}' was not found.");

        var model = new DynamicEditViewModel
        {
            ScreenCode = screen.ScreenCode,
            ScreenName = screen.ScreenName,
            RecordId = id,

            Fields = screen.FormFields
                .OrderBy(x => x.DisplayOrder)
                .ToList(),

            Values = record
        };

        return View(model);
    }

    [HttpPost("/Screen/{screenCode}/Edit/{id}")]
    public async Task<IActionResult> Update(
        string screenCode,
        Guid id)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.DataSource)
            .Include(x => x.FormFields)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        if (screen.DataSource == null)
            return Content($"Screen '{screenCode}' has no data source assigned.");

        if (string.IsNullOrWhiteSpace(screen.SchemaName))
            return Content($"Screen '{screenCode}' has no schema name configured.");

        if (string.IsNullOrWhiteSpace(screen.TableName))
            return Content($"Screen '{screenCode}' has no table name configured.");

        if (string.IsNullOrWhiteSpace(screen.PrimaryKeyColumn))
            return Content($"Screen '{screenCode}' has no primary key column configured.");

        var submittedValues = new Dictionary<string, string?>();

        foreach (var field in screen.FormFields.OrderBy(x => x.DisplayOrder))
        {
            var value = Request.Form[field.FieldName].ToString();
            submittedValues[field.FieldName] = value;
        }

        await _dynamicDataService.UpdateRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id,
            submittedValues
        );

        TempData["Message"] =
            $"Record {id} updated successfully.";

        return Redirect($"/Screen/{screenCode}");
    }

    [HttpPost("/Screen/{screenCode}")]
    public async Task<IActionResult> Save(string screenCode)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.FormFields)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        TempData["Message"] =
            "Form submitted successfully. Dynamic create save logic will be added later.";

        return Redirect($"/Screen/{screenCode}");
    }
}
