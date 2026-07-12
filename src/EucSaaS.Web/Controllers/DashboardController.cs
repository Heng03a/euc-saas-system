using ClosedXML.Excel;
using EucSaaS.Web.Services;
using EucSaaS.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace EucSaaS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(
        DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Loads the complete Dashboard page.
    /// </summary>
    [HttpGet("/Dashboard")]
    public async Task<IActionResult> Index(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var model =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        return View(model);
    }

    /// <summary>
    /// Reloads only the dashboard widget area through AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Refresh(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var model =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        return PartialView(
            "_DashboardContent",
            model);
    }

    /// <summary>
    /// Reloads one dashboard widget through AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshWidget(
        Guid id,
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var dashboard =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        var widget =
            dashboard.Widgets?
                .FirstOrDefault(
                    x => x.Id == id);

        if (widget == null)
        {
            return NotFound(
                "The requested dashboard widget could not be found.");
        }

        return PartialView(
            "_DashboardWidget",
            widget);
    }

    /// <summary>
    /// Exports the currently filtered dashboard to Excel.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportExcel(
        string? department,
        string? status)
    {
        var appRoleId =
            GetCurrentAppRoleId();

        var dashboard =
            await _dashboardService.GetDashboardAsync(
                appRoleId,
                department,
                status);

        using var workbook =
            new XLWorkbook();

        AddDashboardSummaryWorksheet(
            workbook,
            dashboard,
            department,
            status);

        AddDashboardDataWorksheets(
            workbook,
            dashboard);

        using var stream =
            new MemoryStream();

        workbook.SaveAs(stream);

        var fileName =
            $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private Guid? GetCurrentAppRoleId()
    {
        var appRoleIdValue =
            User.FindFirstValue(
                "AppRoleId");

        if (Guid.TryParse(
                appRoleIdValue,
                out var appRoleId))
        {
            return appRoleId;
        }

        return null;
    }

    private static void AddDashboardSummaryWorksheet(
        XLWorkbook workbook,
        DashboardViewModel dashboard,
        string? department,
        string? status)
    {
        var worksheet =
            workbook.Worksheets.Add(
                "Dashboard Summary");

        worksheet.Cell(1, 1).Value =
            "EUC SaaS Dashboard Export";

        worksheet.Range(
                1,
                1,
                1,
                4)
            .Merge();

        worksheet.Cell(1, 1)
            .Style.Font.Bold = true;

        worksheet.Cell(1, 1)
            .Style.Font.FontSize = 16;

        worksheet.Cell(3, 1).Value =
            "Generated At";

        worksheet.Cell(3, 2).Value =
            DateTime.Now;

        worksheet.Cell(3, 2)
            .Style.DateFormat.Format =
            "dd MMM yyyy HH:mm:ss";

        worksheet.Cell(4, 1).Value =
            "Department";

        worksheet.Cell(4, 2).Value =
            string.IsNullOrWhiteSpace(
                department)
                ? "All Departments"
                : department;

        worksheet.Cell(5, 1).Value =
            "Status";

        worksheet.Cell(5, 2).Value =
            string.IsNullOrWhiteSpace(
                status)
                ? "All Statuses"
                : status;

        worksheet.Cell(7, 1).Value =
            "Widget";

        worksheet.Cell(7, 2).Value =
            "Type";

        worksheet.Cell(7, 3).Value =
            "Value";

        worksheet.Cell(7, 4).Value =
            "Data Rows";

        var headerRange =
            worksheet.Range(
                7,
                1,
                7,
                4);

        headerRange.Style.Font.Bold =
            true;

        headerRange.Style.Fill
            .BackgroundColor =
            XLColor.LightGray;

        var currentRow = 8;

        foreach (var widget in
                 dashboard.Widgets ??
                 Enumerable.Empty<
                     DashboardWidgetViewModel>())
        {
            worksheet.Cell(
                    currentRow,
                    1)
                .Value =
                widget.WidgetTitle;

            worksheet.Cell(
                    currentRow,
                    2)
                .Value =
                widget.WidgetType;

            worksheet.Cell(
                    currentRow,
                    3)
                .Value =
                widget.Value ??
                string.Empty;

            worksheet.Cell(
                    currentRow,
                    4)
                .Value =
                widget.Rows?.Count ??
                0;

            currentRow++;
        }

        if (currentRow > 8)
        {
            var summaryTableRange =
                worksheet.Range(
                    7,
                    1,
                    currentRow - 1,
                    4);

            summaryTableRange.CreateTable();
        }

        worksheet.Columns()
            .AdjustToContents();

        worksheet.SheetView
            .FreezeRows(7);
    }

    private static void AddDashboardDataWorksheets(
        XLWorkbook workbook,
        DashboardViewModel dashboard)
    {
        var usedWorksheetNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "Dashboard Summary"
            };

        foreach (var widget in
                 dashboard.Widgets ??
                 Enumerable.Empty<
                     DashboardWidgetViewModel>())
        {
            if (widget.Columns == null ||
                widget.Columns.Count == 0 ||
                widget.Rows == null ||
                widget.Rows.Count == 0)
            {
                continue;
            }

            var worksheetName =
                CreateUniqueWorksheetName(
                    widget.WidgetTitle,
                    usedWorksheetNames);

            var worksheet =
                workbook.Worksheets.Add(
                    worksheetName);

            worksheet.Cell(1, 1).Value =
                widget.WidgetTitle;

            worksheet.Range(
                    1,
                    1,
                    1,
                    Math.Max(
                        1,
                        widget.Columns.Count))
                .Merge();

            worksheet.Cell(1, 1)
                .Style.Font.Bold = true;

            worksheet.Cell(1, 1)
                .Style.Font.FontSize = 14;

            for (var columnIndex = 0;
                 columnIndex <
                 widget.Columns.Count;
                 columnIndex++)
            {
                worksheet.Cell(
                        3,
                        columnIndex + 1)
                    .Value =
                    widget.Columns[
                        columnIndex];
            }

            var headerRange =
                worksheet.Range(
                    3,
                    1,
                    3,
                    widget.Columns.Count);

            headerRange.Style.Font.Bold =
                true;

            headerRange.Style.Fill
                .BackgroundColor =
                XLColor.LightGray;

            var rowIndex = 4;

            foreach (var row in
                     widget.Rows)
            {
                for (var columnIndex = 0;
                     columnIndex <
                     widget.Columns.Count;
                     columnIndex++)
                {
                    var columnName =
                        widget.Columns[
                            columnIndex];

                    var rawValue =
                        row.TryGetValue(
                            columnName,
                            out var value)
                            ? value
                            : string.Empty;

                    SetExcelCellValue(
                        worksheet.Cell(
                            rowIndex,
                            columnIndex + 1),
                        rawValue);
                }

                rowIndex++;
            }

            var dataRange =
                worksheet.Range(
                    3,
                    1,
                    rowIndex - 1,
                    widget.Columns.Count);

            dataRange.CreateTable();

            worksheet.Columns()
                .AdjustToContents();

            worksheet.SheetView
                .FreezeRows(3);
        }
    }

    private static string CreateUniqueWorksheetName(
        string? widgetTitle,
        HashSet<string> usedWorksheetNames)
    {
        var invalidCharacters =
            new[]
            {
                ':',
                '\\',
                '/',
                '?',
                '*',
                '[',
                ']'
            };

        var baseName =
            string.IsNullOrWhiteSpace(
                widgetTitle)
                ? "Widget"
                : widgetTitle.Trim();

        foreach (var invalidCharacter in
                 invalidCharacters)
        {
            baseName =
                baseName.Replace(
                    invalidCharacter,
                    '-');
        }

        if (baseName.Length > 31)
        {
            baseName =
                baseName[..31];
        }

        var worksheetName =
            baseName;

        var suffixNumber = 2;

        while (usedWorksheetNames.Contains(
                   worksheetName))
        {
            var suffix =
                $" ({suffixNumber})";

            var maximumBaseLength =
                31 - suffix.Length;

            var shortenedBaseName =
                baseName.Length >
                maximumBaseLength
                    ? baseName[
                        ..maximumBaseLength]
                    : baseName;

            worksheetName =
                shortenedBaseName +
                suffix;

            suffixNumber++;
        }

        usedWorksheetNames.Add(
            worksheetName);

        return worksheetName;
    }

    private static void SetExcelCellValue(
        IXLCell cell,
        string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(
                rawValue))
        {
            cell.Value =
                string.Empty;

            return;
        }

        if (long.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var integerValue))
        {
            cell.Value =
                integerValue;

            return;
        }

        if (decimal.TryParse(
                rawValue,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var decimalValue))
        {
            cell.Value =
                decimalValue;

            return;
        }

        if (DateTime.TryParse(
                rawValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateValue))
        {
            cell.Value =
                dateValue;

            cell.Style.DateFormat.Format =
                "dd MMM yyyy HH:mm:ss";

            return;
        }

        if (bool.TryParse(
                rawValue,
                out var booleanValue))
        {
            cell.Value =
                booleanValue;

            return;
        }

        cell.Value =
            rawValue;
    }
}
