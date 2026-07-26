namespace EucSaaS.Web.Services;

public static class DashboardChartType
{
    public const string Bar = "bar";
    public const string Line = "line";
    public const string Pie = "pie";
    public const string Doughnut = "doughnut";

    private static readonly HashSet<string> SupportedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Bar,
            Line,
            Pie,
            Doughnut
        };

    public static bool IsSupported(string? chartType)
    {
        return !string.IsNullOrWhiteSpace(chartType)
               && SupportedTypes.Contains(chartType.Trim());
    }

    public static string Normalize(string? chartType)
    {
        if (string.IsNullOrWhiteSpace(chartType))
        {
            return Bar;
        }

        var normalized = chartType.Trim().ToLowerInvariant();

        return IsSupported(normalized)
            ? normalized
            : Bar;
    }

    public static bool IsChartWidget(string? widgetType)
    {
        if (string.IsNullOrWhiteSpace(widgetType))
        {
            return false;
        }

        return widgetType.Trim().ToLowerInvariant() switch
        {
            "bar" => true,
            "line" => true,
            "pie" => true,
            "doughnut" => true,

            // Compatibility with the existing generic Chart type.
            "chart" => true,

            _ => false
        };
    }

    public static string FromWidgetType(string? widgetType)
    {
        if (string.IsNullOrWhiteSpace(widgetType))
        {
            return Bar;
        }

        return widgetType.Trim().ToLowerInvariant() switch
        {
            "line" => Line,
            "pie" => Pie,
            "doughnut" => Doughnut,
            "chart" => Bar,
            "bar" => Bar,
            _ => Bar
        };
    }
}
