namespace EucSaaS.Web.ViewModels.Dashboard;

/// <summary>
/// Represents one category/value point returned by a dynamic
/// dashboard chart SQL query.
/// </summary>
public sealed class ChartDataPointViewModel
{
    /// <summary>
    /// Category displayed on the chart axis or legend.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Numeric value represented by the chart point.
    /// </summary>
    public decimal Value { get; set; }
}
