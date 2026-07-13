using ClosedXML.Excel;
using EucSaaS.Web.Services.Export;
using EucSaaS.Web.ViewModels.Dashboard;

namespace EucSaaS.Web.Tests.Services.Export;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service;

    public ExcelExportServiceTests()
    {
        _service = new ExcelExportService();
    }

    [Fact]
    public void ExportDashboard_ReturnsNonEmptyExcelFile()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: "IT",
                status: "Active",
                exportedBy: "boss");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ExportDashboard_CreatesDashboardSummaryWorksheet()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: "IT",
                status: "Active",
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Dashboard Summary");

        Assert.NotNull(worksheet);

        Assert.Equal(
            "EUC SaaS Dashboard Export",
            worksheet.Cell(1, 1)
                .GetString());
    }

    [Fact]
    public void ExportDashboard_WritesFilterInformation()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: "HR",
                status: "Active",
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Dashboard Summary");

        Assert.Equal(
            "boss",
            worksheet.Cell(4, 2)
                .GetString());

        Assert.Equal(
            "HR",
            worksheet.Cell(5, 2)
                .GetString());

        Assert.Equal(
            "Active",
            worksheet.Cell(6, 2)
                .GetString());
    }

    [Fact]
    public void ExportDashboard_WritesDefaultFilterDescriptions()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: null,
                status: null,
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Dashboard Summary");

        Assert.Equal(
            "All Departments",
            worksheet.Cell(5, 2)
                .GetString());

        Assert.Equal(
            "All Statuses",
            worksheet.Cell(6, 2)
                .GetString());
    }

    [Fact]
    public void ExportDashboard_WritesDashboardWidgetSummaryRows()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: "IT",
                status: null,
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Dashboard Summary");

        Assert.Equal(
            "Employees Count",
            worksheet.Cell(9, 1)
                .GetString());

        Assert.Equal(
            "Card",
            worksheet.Cell(9, 2)
                .GetString());

        Assert.Equal(
            3,
            worksheet.Cell(9, 3)
                .GetValue<int>());

        Assert.Equal(
            "Employee Status",
            worksheet.Cell(10, 1)
                .GetString());

        Assert.Equal(
            "Table",
            worksheet.Cell(10, 2)
                .GetString());

        Assert.Equal(
            2,
            worksheet.Cell(10, 4)
                .GetValue<int>());
    }

    [Fact]
    public void ExportDashboard_CreatesWidgetDataWorksheet()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: "IT",
                status: null,
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Employee Status - Table");

        Assert.NotNull(worksheet);

        Assert.Equal(
            "Employee Status (Table)",
            worksheet.Cell(1, 1)
                .GetString());

        Assert.Equal(
            "Status",
            worksheet.Cell(3, 1)
                .GetString());

        Assert.Equal(
            "Count",
            worksheet.Cell(3, 2)
                .GetString());

        Assert.Equal(
            "Active",
            worksheet.Cell(4, 1)
                .GetString());

        Assert.Equal(
            2,
            worksheet.Cell(4, 2)
                .GetValue<int>());

        Assert.Equal(
            "Inactive",
            worksheet.Cell(5, 1)
                .GetString());

        Assert.Equal(
            1,
            worksheet.Cell(5, 2)
                .GetValue<int>());
    }

    [Fact]
    public void ExportDashboard_HandlesDuplicateWorksheetNames()
    {
        // Arrange
        var dashboard =
            new DashboardViewModel
            {
                Widgets =
                [
                    CreateTableWidget(
                        title: "Employee Status",
                        widgetType: "Table"),

                    CreateTableWidget(
                        title: "Employee Status",
                        widgetType: "Table")
                ]
            };

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: null,
                status: null,
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        Assert.True(
            workbook.TryGetWorksheet(
                "Employee Status - Table",
                out _));

        Assert.True(
            workbook.TryGetWorksheet(
                "Employee Status - Table (2)",
                out _));
    }

    [Fact]
    public void ExportDashboard_DoesNotCreateDataSheetForCardWidget()
    {
        // Arrange
        var dashboard =
            new DashboardViewModel
            {
                Widgets =
                [
                    new DashboardWidgetViewModel
                    {
                        WidgetTitle =
                            "Employees Count",

                        WidgetType =
                            "Card",

                        Value =
                            "3",

                        DisplayOrder =
                            1,

                        WidgetWidth =
                            4,

                        RowPosition =
                            1,

                        ColumnPosition =
                            1
                    }
                ]
            };

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: null,
                status: null,
                exportedBy: "boss");

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        Assert.Single(
            workbook.Worksheets);

        Assert.Equal(
            "Dashboard Summary",
            workbook.Worksheet(1).Name);
    }

    [Fact]
    public void ExportDashboard_HandlesEmptyDashboard()
    {
        // Arrange
        var dashboard =
            new DashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: null,
                status: null,
                exportedBy: "boss");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        Assert.Single(
            workbook.Worksheets);

        Assert.Equal(
            "Dashboard Summary",
            workbook.Worksheet(1).Name);
    }

    [Fact]
    public void ExportDashboard_UsesUnknownUserWhenExportedByIsBlank()
    {
        // Arrange
        var dashboard =
            CreateDashboardViewModel();

        // Act
        var result =
            _service.ExportDashboard(
                dashboard,
                department: null,
                status: null,
                exportedBy: string.Empty);

        // Assert
        using var stream =
            new MemoryStream(result);

        using var workbook =
            new XLWorkbook(stream);

        var worksheet =
            workbook.Worksheet(
                "Dashboard Summary");

        Assert.Equal(
            "Unknown User",
            worksheet.Cell(4, 2)
                .GetString());
    }

    private static DashboardViewModel
        CreateDashboardViewModel()
    {
        return new DashboardViewModel
        {
            Widgets =
            [
                new DashboardWidgetViewModel
                {
                    WidgetTitle =
                        "Employees Count",

                    WidgetType =
                        "Card",

                    Value =
                        "3",

                    DisplayOrder =
                        1,

                    WidgetWidth =
                        4,

                    RowPosition =
                        1,

                    ColumnPosition =
                        1
                },

                CreateTableWidget(
                    title: "Employee Status",
                    widgetType: "Table")
            ]
        };
    }

    private static DashboardWidgetViewModel
        CreateTableWidget(
            string title,
            string widgetType)
    {
        return new DashboardWidgetViewModel
        {
            WidgetTitle =
                title,

            WidgetType =
                widgetType,

            Value =
                "0",

            DisplayOrder =
                2,

            WidgetWidth =
                6,

            RowPosition =
                1,

            ColumnPosition =
                2,

            Columns =
            [
                "Status",
                "Count"
            ],

            Rows =
            [
                new Dictionary<string, string>
                {
                    ["Status"] =
                        "Active",

                    ["Count"] =
                        "2"
                },

                new Dictionary<string, string>
                {
                    ["Status"] =
                        "Inactive",

                    ["Count"] =
                        "1"
                }
            ]
        };
    }
}
