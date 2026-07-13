using ClosedXML.Excel;
using EucSaaS.Web.ViewModels.Dashboard;
using System.Globalization;

namespace EucSaaS.Web.Services.Export;

public class ExcelExportService : IExcelExportService
{
    private const string DashboardSummarySheetName =
        "Dashboard Summary";

    private const string DashboardTitle =
        "EUC SaaS Dashboard Export";

    private const int DashboardColumnCount = 8;

    public byte[] ExportDashboard(
        DashboardViewModel dashboard,
        string? department,
        string? status,
        string exportedBy)
    {
        ArgumentNullException.ThrowIfNull(dashboard);

        using var workbook =
            new XLWorkbook();

        AddDashboardSummaryWorksheet(
            workbook,
            dashboard,
            department,
            status,
            exportedBy);

        AddDashboardDataWorksheets(
            workbook,
            dashboard);

        return SaveWorkbookToBytes(
            workbook);
    }

    private static void AddDashboardSummaryWorksheet(
        XLWorkbook workbook,
        DashboardViewModel dashboard,
        string? department,
        string? status,
        string exportedBy)
    {
        var worksheet =
            workbook.Worksheets.Add(
                DashboardSummarySheetName);

        CreateWorksheetTitle(
            worksheet,
            DashboardTitle,
            DashboardColumnCount,
            fontSize: 18,
            rowHeight: 28);

        AddDashboardInformationSection(
            worksheet,
            department,
            status,
            exportedBy);

        const int headerRow = 8;

        var headers =
            new[]
            {
                "Widget",
                "Type",
                "Value",
                "Data Rows",
                "Display Order",
                "Width",
                "Row",
                "Column"
            };

        CreateTableHeader(
            worksheet,
            headerRow,
            headers);

        var nextRow =
            WriteDashboardSummaryRows(
                worksheet,
                dashboard,
                headerRow + 1);

        CreateExcelTable(
            worksheet,
            headerRow,
            1,
            nextRow - 1,
            headers.Length);

        ApplyWorksheetLayout(
            worksheet,
            freezeRow: headerRow,
            minimumFirstColumnWidth: 24,
            landscape: true);
    }

    private static void AddDashboardInformationSection(
        IXLWorksheet worksheet,
        string? department,
        string? status,
        string exportedBy)
    {
        AddInformationRow(
            worksheet,
            rowNumber: 3,
            label: "Generated At",
            value: DateTime.Now);

        worksheet.Cell(3, 2)
            .Style.DateFormat.Format =
            "dd MMM yyyy HH:mm:ss";

        AddInformationRow(
            worksheet,
            rowNumber: 4,
            label: "Exported By",
            value:
                string.IsNullOrWhiteSpace(
                    exportedBy)
                    ? "Unknown User"
                    : exportedBy);

        AddInformationRow(
            worksheet,
            rowNumber: 5,
            label: "Department",
            value:
                string.IsNullOrWhiteSpace(
                    department)
                    ? "All Departments"
                    : department);

        AddInformationRow(
            worksheet,
            rowNumber: 6,
            label: "Status",
            value:
                string.IsNullOrWhiteSpace(
                    status)
                    ? "All Statuses"
                    : status);

        StyleInformationLabels(
            worksheet,
            startRow: 3,
            endRow: 6);
    }

    private static int WriteDashboardSummaryRows(
        IXLWorksheet worksheet,
        DashboardViewModel dashboard,
        int startRow)
    {
        var currentRow =
            startRow;

        foreach (var widget in
                 dashboard.Widgets ??
                 Enumerable.Empty<
                     DashboardWidgetViewModel>())
        {
            worksheet.Cell(
                    currentRow,
                    1)
                .Value =
                widget.WidgetTitle ??
                string.Empty;

            worksheet.Cell(
                    currentRow,
                    2)
                .Value =
                widget.WidgetType ??
                string.Empty;

            SetExcelCellValue(
                worksheet.Cell(
                    currentRow,
                    3),
                widget.Value);

            worksheet.Cell(
                    currentRow,
                    4)
                .Value =
                widget.Rows?.Count ??
                0;

            worksheet.Cell(
                    currentRow,
                    5)
                .Value =
                widget.DisplayOrder;

            worksheet.Cell(
                    currentRow,
                    6)
                .Value =
                widget.WidgetWidth;

            worksheet.Cell(
                    currentRow,
                    7)
                .Value =
                widget.RowPosition;

            worksheet.Cell(
                    currentRow,
                    8)
                .Value =
                widget.ColumnPosition;

            currentRow++;
        }

        return currentRow;
    }

    private static void AddDashboardDataWorksheets(
        XLWorkbook workbook,
        DashboardViewModel dashboard)
    {
        var usedWorksheetNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                DashboardSummarySheetName
            };

        foreach (var widget in
                 dashboard.Widgets ??
                 Enumerable.Empty<
                     DashboardWidgetViewModel>())
        {
            if (!HasTabularData(widget))
            {
                continue;
            }

            AddWidgetDataWorksheet(
                workbook,
                widget,
                usedWorksheetNames);
        }
    }

    private static bool HasTabularData(
        DashboardWidgetViewModel widget)
    {
        return widget.Columns != null &&
               widget.Columns.Count > 0 &&
               widget.Rows != null &&
               widget.Rows.Count > 0;
    }

    private static void AddWidgetDataWorksheet(
        XLWorkbook workbook,
        DashboardWidgetViewModel widget,
        HashSet<string> usedWorksheetNames)
    {
        var worksheetDisplayName =
            $"{widget.WidgetTitle} - {widget.WidgetType}";

        var worksheetName =
            CreateUniqueWorksheetName(
                worksheetDisplayName,
                usedWorksheetNames);

        var worksheet =
            workbook.Worksheets.Add(
                worksheetName);

        var columnCount =
            widget.Columns!.Count;

        CreateWorksheetTitle(
            worksheet,
            $"{widget.WidgetTitle} ({widget.WidgetType})",
            columnCount,
            fontSize: 14,
            rowHeight: 24);

        const int headerRow = 3;

        CreateTableHeader(
            worksheet,
            headerRow,
            widget.Columns);

        var nextRow =
            WriteWidgetDataRows(
                worksheet,
                widget,
                headerRow + 1);

        CreateExcelTable(
            worksheet,
            headerRow,
            1,
            nextRow - 1,
            columnCount);

        ApplyWorksheetLayout(
            worksheet,
            freezeRow: headerRow,
            minimumFirstColumnWidth: null,
            landscape: true);
    }

    private static int WriteWidgetDataRows(
        IXLWorksheet worksheet,
        DashboardWidgetViewModel widget,
        int startRow)
    {
        var rowIndex =
            startRow;

        foreach (var row in widget.Rows!)
        {
            for (var columnIndex = 0;
                 columnIndex <
                 widget.Columns!.Count;
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

        return rowIndex;
    }

    private static void CreateWorksheetTitle(
        IXLWorksheet worksheet,
        string title,
        int columnCount,
        double fontSize,
        double rowHeight)
    {
        worksheet.Cell(1, 1).Value =
            title;

        worksheet.Range(
                1,
                1,
                1,
                Math.Max(
                    1,
                    columnCount))
            .Merge();

        var titleCell =
            worksheet.Cell(
                1,
                1);

        titleCell.Style.Font.Bold =
            true;

        titleCell.Style.Font.FontSize =
            fontSize;

        titleCell.Style.Font.FontColor =
            XLColor.White;

        titleCell.Style.Fill.BackgroundColor =
            XLColor.DarkBlue;

        titleCell.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        titleCell.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Row(1).Height =
            rowHeight;
    }

    private static void AddInformationRow(
        IXLWorksheet worksheet,
        int rowNumber,
        string label,
        object value)
    {
        worksheet.Cell(
                rowNumber,
                1)
            .Value =
            label;

        worksheet.Cell(
                rowNumber,
                2)
            .Value =
            XLCellValue.FromObject(
                value);
    }

    private static void StyleInformationLabels(
        IXLWorksheet worksheet,
        int startRow,
        int endRow)
    {
        var labelRange =
            worksheet.Range(
                startRow,
                1,
                endRow,
                1);

        labelRange.Style.Font.Bold =
            true;

        labelRange.Style.Fill.BackgroundColor =
            XLColor.LightBlue;
    }

    private static void CreateTableHeader(
        IXLWorksheet worksheet,
        int headerRow,
        IReadOnlyList<string> headers)
    {
        for (var index = 0;
             index < headers.Count;
             index++)
        {
            worksheet.Cell(
                    headerRow,
                    index + 1)
                .Value =
                headers[index];
        }

        var headerRange =
            worksheet.Range(
                headerRow,
                1,
                headerRow,
                headers.Count);

        headerRange.Style.Font.Bold =
            true;

        headerRange.Style.Font.FontColor =
            XLColor.White;

        headerRange.Style.Fill.BackgroundColor =
            XLColor.DarkBlue;

        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        headerRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;
    }

    private static void CreateExcelTable(
        IXLWorksheet worksheet,
        int startRow,
        int startColumn,
        int endRow,
        int endColumn)
    {
        if (endRow <= startRow)
        {
            return;
        }

        var tableRange =
            worksheet.Range(
                startRow,
                startColumn,
                endRow,
                endColumn);

        var table =
            tableRange.CreateTable();

        table.Theme =
            XLTableTheme.TableStyleMedium2;
    }

    private static void ApplyWorksheetLayout(
        IXLWorksheet worksheet,
        int freezeRow,
        double? minimumFirstColumnWidth,
        bool landscape)
    {
        worksheet.Columns()
            .AdjustToContents();

        if (minimumFirstColumnWidth.HasValue)
        {
            worksheet.Column(1).Width =
                Math.Max(
                    worksheet.Column(1).Width,
                    minimumFirstColumnWidth.Value);
        }

        worksheet.SheetView
            .FreezeRows(
                freezeRow);

        if (landscape)
        {
            worksheet.PageSetup.PageOrientation =
                XLPageOrientation.Landscape;
        }

        worksheet.PageSetup
            .FitToPages(
                1,
                0);
    }

    private static byte[] SaveWorkbookToBytes(
        XLWorkbook workbook)
    {
        using var stream =
            new MemoryStream();

        workbook.SaveAs(
            stream);

        return stream.ToArray();
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
