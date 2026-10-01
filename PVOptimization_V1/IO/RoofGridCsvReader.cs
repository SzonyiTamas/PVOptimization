using System.Globalization;
using PVOptimization_V1.Models;

namespace PVOptimization_V1.IO;

/// <summary>
/// Loads a <see cref="RoofGrid"/> from a CSV file with <c>center_x</c>, <c>center_y</c> and <c>value</c> columns.
/// Rows whose coordinates or value cannot be parsed are skipped.
/// </summary>
public static class RoofGridCsvReader
{
    private const string CenterXColumn = "center_x";
    private const string CenterYColumn = "center_y";
    private const string ValueColumn = "value";

    public static RoofGrid Read(string path) => RoofGrid.FromSamples(ReadSamples(path));

    private static List<RoofSample> ReadSamples(string path)
    {
        using var reader = new StreamReader(path);

        string header = reader.ReadLine()
            ?? throw new InvalidDataException($"The roof CSV file is empty: {path}");
        string[] columns = header.Split(',');

        int xIndex = GetColumnIndex(columns, CenterXColumn, path);
        int yIndex = GetColumnIndex(columns, CenterYColumn, path);
        int valueIndex = GetColumnIndex(columns, ValueColumn, path);

        var samples = new List<RoofSample>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] fields = line.Split(',');
            if (TryParseDouble(fields[xIndex], out double x)
                && TryParseDouble(fields[yIndex], out double y)
                && TryParseDouble(fields[valueIndex], out double value))
            {
                samples.Add(new RoofSample(x, y, value));
            }
        }

        return samples;
    }

    private static int GetColumnIndex(string[] columns, string name, string path)
    {
        int index = Array.FindIndex(
            columns, column => string.Equals(column.Trim(), name, StringComparison.OrdinalIgnoreCase));

        return index >= 0
            ? index
            : throw new InvalidDataException($"Missing column '{name}' in roof CSV file: {path}");
    }

    private static bool TryParseDouble(string text, out double value) =>
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
