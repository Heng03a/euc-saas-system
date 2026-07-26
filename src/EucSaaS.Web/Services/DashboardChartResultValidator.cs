using System.Data;
using System.Globalization;
using EucSaaS.Web.ViewModels.Dashboard;

namespace EucSaaS.Web.Services;

public sealed class DashboardChartResultValidator
{
    public const string LabelColumnName = "Label";
    public const string ValueColumnName = "Value";

    private const int DefaultMaximumRows = 50;

    public List<ChartDataPointViewModel> ValidateAndConvert(
        DataTable table,
        int maximumRows = DefaultMaximumRows)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (maximumRows <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumRows),
                "Maximum chart rows must be greater than zero.");
        }

        var labelColumn = FindColumn(
            table,
            LabelColumnName);

        var valueColumn = FindColumn(
            table,
            ValueColumnName);

        if (labelColumn is null)
        {
            throw new InvalidOperationException(
                $"Chart SQL must return a column named " +
                $"'{LabelColumnName}'.");
        }

        if (valueColumn is null)
        {
            throw new InvalidOperationException(
                $"Chart SQL must return a numeric column named " +
                $"'{ValueColumnName}'.");
        }

        if (table.Rows.Count > maximumRows)
        {
            throw new InvalidOperationException(
                $"Chart query returned {table.Rows.Count} rows. " +
                $"The maximum allowed is {maximumRows}.");
        }

        var points = new List<ChartDataPointViewModel>();

        foreach (DataRow row in table.Rows)
        {
            var label = Convert.ToString(
                row[labelColumn],
                CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(label))
            {
                label = "(Blank)";
            }

            var rawValue = row[valueColumn];

            if (rawValue is null || rawValue == DBNull.Value)
            {
                throw new InvalidOperationException(
                    $"Chart value for label '{label}' is null.");
            }

            if (!TryConvertToDecimal(
                    rawValue,
                    out var numericValue))
            {
                throw new InvalidOperationException(
                    $"Chart value '{rawValue}' for label " +
                    $"'{label}' is not numeric.");
            }

            points.Add(new ChartDataPointViewModel
            {
                Label = label.Trim(),
                Value = numericValue
            });
        }

        return points;
    }

    private static DataColumn? FindColumn(
        DataTable table,
        string expectedName)
    {
        return table.Columns
            .Cast<DataColumn>()
            .FirstOrDefault(column =>
                string.Equals(
                    column.ColumnName,
                    expectedName,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryConvertToDecimal(
        object value,
        out decimal result)
    {
        switch (value)
        {
            case byte byteValue:
                result = byteValue;
                return true;

            case short shortValue:
                result = shortValue;
                return true;

            case int intValue:
                result = intValue;
                return true;

            case long longValue:
                result = longValue;
                return true;

            case float floatValue:
                result = Convert.ToDecimal(floatValue);
                return true;

            case double doubleValue:
                result = Convert.ToDecimal(doubleValue);
                return true;

            case decimal decimalValue:
                result = decimalValue;
                return true;

            default:
                return decimal.TryParse(
                    Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out result);
        }
    }
}
