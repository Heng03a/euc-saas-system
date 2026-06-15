using EucSaaS.Application.Services;
using EucSaaS.Infrastructure.Data;
using EucSaaS.Web.ViewModels.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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

var filters = ReadFilters(Request.Query);

var table = await _dynamicDataService.GetTableDataAsync(
    screen.DataSource,
    screen.SchemaName,
    screen.TableName,
    filters
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
    ScreenCode = screen.ScreenCode,
    ScreenName = screen.ScreenName,

SearchValues = Request.Query.ToDictionary(
    x => x.Key,
    x => x.Value.ToString()
),

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

[HttpGet("/Screen/{screenCode}/Create")]
public async Task<IActionResult> Create(string screenCode)
{
    var screen = await _context.ScreenDefinitions
        .Include(x => x.FormFields)
            .ThenInclude(x => x.Options)
        .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

    if (screen == null)
        return Content($"Screen definition '{screenCode}' was not found.");

    var model = new DynamicEditViewModel
    {
        ScreenCode = screen.ScreenCode,
        ScreenName = screen.ScreenName,
        RecordId = Guid.Empty,
        Fields = screen.FormFields
            .OrderBy(x => x.DisplayOrder)
            .ToList(),
        Values = new Dictionary<string, object?>()
    };

    return View(model);
}

[HttpPost("/Screen/{screenCode}")]
public async Task<IActionResult> Save(string screenCode)
{
    var screen = await _context.ScreenDefinitions
        .Include(x => x.DataSource)
        .Include(x => x.FormFields)
        .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

    if (screen == null)
        return Content($"Screen definition '{screenCode}' was not found.");

    if (screen.DataSource == null)
        return Content($"Screen '{screenCode}' has no data source assigned.");

    if (string.IsNullOrWhiteSpace(screen.SchemaName))
        return Content($"Screen '{screenCode}' has no schema name configured.");


    var submittedValues = new Dictionary<string, string?>();

    foreach (var field in screen.FormFields.OrderBy(x => x.DisplayOrder))
    {
        var value = Request.Form[field.FieldName].ToString();
        submittedValues[field.FieldName] = value;
    }

    await _dynamicDataService.InsertRecordAsync(
        screen.DataSource,
        screen.SchemaName,
        screen.TableName,
        submittedValues
    );

    TempData["Message"] = "Record created successfully.";

    return Redirect($"/Screen/{screenCode}");
}

[HttpGet("/Screen/{screenCode}/Export")]
public async Task<IActionResult> Export(
    string screenCode,
    string? searchField,
    string? searchOperator,
    string? searchValue)
{
    searchField ??= "";
    searchOperator ??= "contains";
    searchValue ??= "";

    var screen = await _context.ScreenDefinitions
        .Include(x => x.DataSource)
        .Include(x => x.Columns)
        .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

    if (screen == null)
        return Content($"Screen definition '{screenCode}' was not found.");

    if (screen.DataSource == null)
        return Content($"Screen '{screenCode}' has no data source assigned.");

var filters = ReadFilters(Request.Query);

var table = await _dynamicDataService.GetTableDataAsync(
    screen.DataSource,
    screen.SchemaName,
    screen.TableName,
    filters
);


    var visibleColumns = screen.Columns
        .Where(x => x.IsVisible)
        .OrderBy(x => x.DisplayOrder)
        .ToList();

    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add(screen.ScreenName);

    for (int i = 0; i < visibleColumns.Count; i++)
    {
        worksheet.Cell(1, i + 1).Value = visibleColumns[i].DisplayLabel;
        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
    }

    int rowIndex = 2;

    foreach (System.Data.DataRow dataRow in table.Rows)
    {

        for (int colIndex = 0; colIndex < visibleColumns.Count; colIndex++)
        {
            var columnName = visibleColumns[colIndex].FieldName;

            worksheet.Cell(rowIndex, colIndex + 1).Value =
                dataRow[columnName]?.ToString() ?? "";
        }

        rowIndex++;
    }

    worksheet.Columns().AdjustToContents();

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);

    var fileName = $"{screen.ScreenName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
}

private Dictionary<string, string> ReadFilters(IQueryCollection query)
{
    var filters = new Dictionary<string, string>();

    var searchField1 = query["searchField1"].ToString();
    var searchValue1 = query["searchValue1"].ToString();

    if (!string.IsNullOrWhiteSpace(searchField1)
        && !string.IsNullOrWhiteSpace(searchValue1))
    {
        filters[searchField1] = searchValue1;
    }

    var searchField2 = query["searchField2"].ToString();
    var searchValue2 = query["searchValue2"].ToString();

    if (!string.IsNullOrWhiteSpace(searchField2)
        && !string.IsNullOrWhiteSpace(searchValue2))
    {
        filters[searchField2] = searchValue2;
    }

    return filters;
}


}
