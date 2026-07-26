namespace EucSaaS.Web.ViewModels.Dashboard;

/// <summary>
/// Contains the validated data and presentation settings needed
/// to render one dynamic dashboard chart.
/// </summary>
public sealed class ChartWidgetDataViewModel
{
    public string ChartType { get; set; } = "bar";

    public bool ShowLegend { get; set; } = true;

    public List<ChartDataPointViewModel> Points { get; set; } = [];

    public bool HasData => Points.Count > 0;

    public IReadOnlyList<string> Labels =>
        Points
            .Select(x => x.Label)
            .ToList();

    public IReadOnlyList<decimal> Values =>
        Points
            .Select(x => x.Value)
            .ToList();
}
