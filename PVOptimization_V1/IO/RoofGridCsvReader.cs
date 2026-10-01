using System.Globalization;
using PVOptimization_V1.Models;

namespace PVOptimization_V1.IO;

public static class RoofGridCsvReader
{
    private const string CenterXColumn = "center_x";
    private const string CenterYColumn = "center_y";
    private const string ValueColumn = "value";

    public static RoofGrid Read(string path) => RoofGrid.FromSamples(ReadSamples(path));

    private static List<RoofSample> ReadSamples(string path)
    {
        using var reader = new StreamReader(path);

        string header = reader.ReadLine()!;
        string[] columns = header.Split(',').Select(s => s.Trim()).ToArray();

        int? xIndex = FindColumnIndex(columns, CenterXColumn);
        int? yIndex = FindColumnIndex(columns, CenterYColumn);
        int? valueIndex = FindColumnIndex(columns, ValueColumn);

        var samples = new List<RoofSample>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] fields = line.Split(',');
            if (!TryParseDouble(fields[xIndex!.Value], out double x)) continue;
            if (!TryParseDouble(fields[yIndex!.Value], out double y)) continue;
            if (!TryParseDouble(fields[valueIndex!.Value], out double value)) continue;
            samples.Add(new RoofSample(x, y, value));
        }

        return samples;
    }

    private static int? FindColumnIndex(string[] columns, string name)
    {
        name = name.Trim().ToLowerInvariant();

        for (int i = 0; i < columns.Length; i++)
        {
            if (columns[i].Trim().ToLowerInvariant() == name)
                return i;
        }

        return null;
    }

    private static bool TryParseDouble(string text, out double value)
    {
        text = text.Trim();
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        return double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
