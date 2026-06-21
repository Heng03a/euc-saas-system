using EucSaaS.Application.Services;
using EucSaaS.Domain.Entities;
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
    public async Task<IActionResult> Index(
        string screenCode,
        int pageNumber = 1,
        int pageSize = 10)
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

        var sortColumn = Request.Query["sortColumn"].ToString();
        var sortDirection = Request.Query["sortDirection"].ToString();

        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = screen.DefaultSortColumn ?? "";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = screen.DefaultSortDirection ?? "ASC";

        if (pageSize <= 0)
            pageSize = 10;

        if (!new[] { 1, 2, 10, 25, 50, 100 }.Contains(pageSize))
            pageSize = 10;

        if (pageNumber < 1)
            pageNumber = 1;

        var totalRecords = await _dynamicDataService.GetTableCountAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            filters
        );

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var validColumns = screen.Columns.Select(x => x.FieldName).ToList();

        if (!validColumns.Contains(sortColumn))
            sortColumn = screen.DefaultSortColumn ?? "";

        sortDirection = sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        var table = await _dynamicDataService.GetTableDataAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            filters,
            sortColumn,
            sortDirection,
            pageNumber,
            pageSize,
            true
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
            ScreenMode = screen.ScreenMode,
            SortColumn = sortColumn,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,

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
        Guid id,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

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

        var record = await _dynamicDataService.GetRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id
        );

        if (record == null)
            return Content($"Record '{id}' was not found.");

        var model = BuildEditViewModel(screen, id, record);

        return View(model);
    }

    [HttpPost("/Screen/{screenCode}/Edit/{id}")]
    public async Task<IActionResult> Update(
        string screenCode,
        Guid id,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

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

        var submittedValues = ReadSubmittedValues(screen);

        ValidateDynamicFields(screen, submittedValues);

        await ValidateUniqueFieldsAsync(
            screen,
            submittedValues,
            id
        );

        if (!ModelState.IsValid)
        {
            var invalidModel = BuildEditViewModel(screen, id, submittedValues);
            return View("Edit", invalidModel);
        }

        var displayValue = GetDisplayValue(screen, submittedValues, id);

        await _dynamicDataService.UpdateRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id,
            submittedValues
        );

        TempData["Message"] =
            $"{screen.ScreenName} {displayValue} updated successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Redirect($"/Screen/{screenCode}");
    }

    [HttpGet("/Screen/{screenCode}/Create")]
    public async Task<IActionResult> Create(
        string screenCode,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        var screen = await _context.ScreenDefinitions
            .Include(x => x.FormFields)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        var model = BuildEditViewModel(
            screen,
            Guid.Empty,
            new Dictionary<string, string?>()
        );

        return View(model);
    }

    [HttpPost("/Screen/{screenCode}")]
    public async Task<IActionResult> Save(
        string screenCode,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

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

        var submittedValues = ReadSubmittedValues(screen);

        ValidateDynamicFields(screen, submittedValues);

        await ValidateUniqueFieldsAsync(
            screen,
            submittedValues,
            null
        );

        if (!ModelState.IsValid)
        {
            var invalidModel = BuildEditViewModel(
                screen,
                Guid.Empty,
                submittedValues
            );

            return View("Create", invalidModel);
        }

        var displayValue = GetDisplayValue(screen, submittedValues, Guid.Empty);

        await _dynamicDataService.InsertRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            submittedValues
        );

        TempData["Message"] =
            $"{screen.ScreenName} {displayValue} created successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Redirect($"/Screen/{screenCode}");
    }

    [HttpPost("/Screen/{screenCode}/Delete/{id}")]
    public async Task<IActionResult> Delete(
        string screenCode,
        Guid id,
        string? returnUrl = null)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.DataSource)
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

        var record = await _dynamicDataService.GetRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id
        );

        var displayValue = GetDisplayValue(screen, record, id);

        await _dynamicDataService.DeleteRecordAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            screen.PrimaryKeyColumn,
            id
        );

        TempData["Message"] =
            $"{screen.ScreenName} {displayValue} deleted successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Redirect($"/Screen/{screenCode}");
    }

    [HttpGet("/Screen/{screenCode}/Export")]
    public async Task<IActionResult> Export(string screenCode)
    {
        var screen = await _context.ScreenDefinitions
            .Include(x => x.DataSource)
            .Include(x => x.Columns)
            .FirstOrDefaultAsync(x => x.ScreenCode == screenCode);

        if (screen == null)
            return Content($"Screen definition '{screenCode}' was not found.");

        if (screen.DataSource == null)
            return Content($"Screen '{screenCode}' has no data source assigned.");

        var filters = ReadFilters(Request.Query);

        var sortColumn = Request.Query["sortColumn"].ToString();
        var sortDirection = Request.Query["sortDirection"].ToString();

        if (string.IsNullOrWhiteSpace(sortColumn))
            sortColumn = screen.DefaultSortColumn ?? "";

        if (string.IsNullOrWhiteSpace(sortDirection))
            sortDirection = screen.DefaultSortDirection ?? "ASC";

        var validColumns = screen.Columns.Select(x => x.FieldName).ToList();

        if (!validColumns.Contains(sortColumn))
            sortColumn = screen.DefaultSortColumn ?? "";

        sortDirection = sortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        var table = await _dynamicDataService.GetTableDataAsync(
            screen.DataSource,
            screen.SchemaName,
            screen.TableName,
            filters,
            sortColumn,
            sortDirection
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

    private Dictionary<string, string?> ReadSubmittedValues(
        ScreenDefinition screen)
    {
        var submittedValues = new Dictionary<string, string?>();

        foreach (var field in screen.FormFields.OrderBy(x => x.DisplayOrder))
        {
            submittedValues[field.FieldName] =
                Request.Form[field.FieldName].ToString();
        }

        return submittedValues;
    }

    private void ValidateDynamicFields(
        ScreenDefinition screen,
        Dictionary<string, string?> values)
    {
        foreach (var field in screen.FormFields
            .Where(x => x.IsVisible)
            .OrderBy(x => x.DisplayOrder))
        {
            values.TryGetValue(field.FieldName, out var value);

            var label = string.IsNullOrWhiteSpace(field.DisplayLabel)
                ? field.FieldName
                : field.DisplayLabel;

            if (field.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(
                    field.FieldName,
                    $"{label} is required.");
            }

            if (field.MinLength.HasValue &&
                !string.IsNullOrWhiteSpace(value) &&
                value.Length < field.MinLength.Value)
            {
                ModelState.AddModelError(
                    field.FieldName,
                    $"{label} minimum length is {field.MinLength.Value}.");
            }

            if (field.MaxLength.HasValue &&
                !string.IsNullOrWhiteSpace(value) &&
                value.Length > field.MaxLength.Value)
            {
                ModelState.AddModelError(
                    field.FieldName,
                    $"{label} maximum length is {field.MaxLength.Value}.");
            }

            if (!string.IsNullOrWhiteSpace(field.ValidationRegex) &&
                !string.IsNullOrWhiteSpace(value))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(
                    value,
                    field.ValidationRegex))
                {
                    ModelState.AddModelError(
                        field.FieldName,
                        $"{label} format is invalid.");
                }
            }
        }
    }

    private DynamicEditViewModel BuildEditViewModel(
        ScreenDefinition screen,
        Guid recordId,
        Dictionary<string, string?> values)
    {
        return new DynamicEditViewModel
        {
            ScreenCode = screen.ScreenCode,
            ScreenName = screen.ScreenName,
            RecordId = recordId,
            Fields = screen.FormFields
                .OrderBy(x => x.DisplayOrder)
                .ToList(),
            Values = values.ToDictionary(
                x => x.Key,
                x => (object?)x.Value
            )
        };
    }

    private DynamicEditViewModel BuildEditViewModel(
        ScreenDefinition screen,
        Guid recordId,
        Dictionary<string, object?> values)
    {
        return new DynamicEditViewModel
        {
            ScreenCode = screen.ScreenCode,
            ScreenName = screen.ScreenName,
            RecordId = recordId,
            Fields = screen.FormFields
                .OrderBy(x => x.DisplayOrder)
                .ToList(),
            Values = values
        };
    }

    private string GetDisplayValue(
        ScreenDefinition screen,
        Dictionary<string, object?>? record,
        Guid id)
    {
        if (record != null &&
            !string.IsNullOrWhiteSpace(screen.DisplayField) &&
            record.ContainsKey(screen.DisplayField))
        {
            return record[screen.DisplayField]?.ToString() ?? id.ToString();
        }

        return id == Guid.Empty ? "record" : id.ToString();
    }

    private string GetDisplayValue(
        ScreenDefinition screen,
        Dictionary<string, string?> values,
        Guid id)
    {
        if (!string.IsNullOrWhiteSpace(screen.DisplayField) &&
            values.ContainsKey(screen.DisplayField))
        {
            return values[screen.DisplayField] ?? id.ToString();
        }

        return id == Guid.Empty ? "record" : id.ToString();
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

    private async Task ValidateUniqueFieldsAsync(
        ScreenDefinition screen,
        Dictionary<string, string?> values,
        Guid? currentRecordId = null)
    {
        if (screen.DataSource == null)
            return;

        foreach (var field in screen.FormFields
            .Where(x => x.IsVisible && x.IsUnique)
            .OrderBy(x => x.DisplayOrder))
        {
            if (!values.TryGetValue(field.FieldName, out var value))
                continue;

            if (string.IsNullOrWhiteSpace(value))
                continue;

            var filters = new Dictionary<string, string>
            {
                { field.FieldName, value }
            };

            var count = await _dynamicDataService.GetTableCountAsync(
                screen.DataSource,
                screen.SchemaName,
                screen.TableName,
                filters
            );

            if (count <= 0)
                continue;

            if (currentRecordId.HasValue && currentRecordId.Value != Guid.Empty)
            {
                var existingRows = await _dynamicDataService.GetTableDataAsync(
                    screen.DataSource,
                    screen.SchemaName,
                    screen.TableName,
                    filters,
                    "",
                    "ASC"
                );

                var isSameRecordOnly = true;

                foreach (System.Data.DataRow row in existingRows.Rows)
                {
                    if (!row.Table.Columns.Contains(screen.PrimaryKeyColumn))
                    {
                        isSameRecordOnly = false;
                        break;
                    }

                    var existingIdText = row[screen.PrimaryKeyColumn]?.ToString();

                    if (!Guid.TryParse(existingIdText, out var existingId) ||
                        existingId != currentRecordId.Value)
                    {
                        isSameRecordOnly = false;
                        break;
                    }
                }

                if (isSameRecordOnly)
                    continue;
            }

            var label = string.IsNullOrWhiteSpace(field.DisplayLabel)
                ? field.FieldName
                : field.DisplayLabel;

            ModelState.AddModelError(
                field.FieldName,
                $"{label} already exists.");
        }
    }
}
