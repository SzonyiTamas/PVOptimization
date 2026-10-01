namespace PVOptimization_V1.Models;

/// <summary>
/// Rasterized roof surface with one cell per unit, supporting fast queries of
/// summed solar values and forbidden (non-placeable) cells over rectangles.
/// </summary>
public sealed class RoofGrid
{
    private readonly SummedAreaTable<double> _values;
    private readonly SummedAreaTable<int> _forbiddenCells;

    private RoofGrid(
        SummedAreaTable<double> values,
        SummedAreaTable<int> forbiddenCells,
        double minX, double minY, double maxX, double maxY)
    {
        _values = values;
        _forbiddenCells = forbiddenCells;
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }

    public int Width => _values.Width;
    public int Height => _values.Height;

    public CellRange EntireRoof => new(0, 0, Width - 1, Height - 1);

    public static RoofGrid FromSamples(IReadOnlyList<RoofSample> samples)
    {
        double minX = samples.Min(s => s.CenterX);
        double maxX = samples.Max(s => s.CenterX);
        double minY = samples.Min(s => s.CenterY);
        double maxY = samples.Max(s => s.CenterY);

        int width = (int)Math.Round(maxX - minX) + 1;
        int height = (int)Math.Round(maxY - minY) + 1;

        var values = new double[width, height];
        var forbiddenCells = new int[width, height];

        foreach (var sample in samples)
        {
            int ix = (int)Math.Round(sample.CenterX - minX);
            int iy = (int)Math.Round(sample.CenterY - minY);

            values[ix, iy] += sample.IsForbidden ? 0.0 : sample.Value;
            if (sample.IsForbidden)
                forbiddenCells[ix, iy]++;
        }

        return new RoofGrid(
            new SummedAreaTable<double>(values),
            new SummedAreaTable<int>(forbiddenCells),
            minX, minY, maxX, maxY);
    }

    /// <summary>Returns the grid cells fully covered by the given rectangle.</summary>
    public CellRange ToCellRange(double xMin, double yMin, double xMax, double yMax) =>
        new((int)Math.Ceiling(xMin - MinX),
            (int)Math.Ceiling(yMin - MinY),
            (int)Math.Floor(xMax - MinX),
            (int)Math.Floor(yMax - MinY));

    public CellRange ToCellRange(Panel panel) =>
        ToCellRange(panel.XMin, panel.YMin, panel.XMax, panel.YMax);

    public double Sum(CellRange range) => _values.Sum(range);

    public int ForbiddenCount(CellRange range) => _forbiddenCells.Sum(range);

    public bool OverlapsForbidden(Panel panel) => ForbiddenCount(ToCellRange(panel)) > 0;

    public void ClampInside(Panel panel) => panel.ClampToBounds(MinX, MinY, MaxX, MaxY);

    /// <summary>
    /// Checks whether the straight line between the centers of two panels crosses a forbidden cell.
    /// </summary>
    public bool HasForbiddenBetweenCenters(Panel a, Panel b)
    {
        var (x0, y0) = ToClampedCell(a.CenterX, a.CenterY);
        var (x1, y1) = ToClampedCell(b.CenterX, b.CenterY);

        var boundingBox = new CellRange(
            Math.Min(x0, x1), Math.Min(y0, y1),
            Math.Max(x0, x1), Math.Max(y0, y1));

        // Fast path: nothing forbidden in the bounding box, so the line cannot hit anything.
        if (ForbiddenCount(boundingBox) == 0)
            return false;

        // Bresenham's line algorithm, checking every visited cell.
        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        int x = x0;
        int y = y0;
        while (true)
        {
            if (ForbiddenCount(new CellRange(x, y, x, y)) > 0) return true;
            if (x == x1 && y == y1) break;

            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x += sx; }
            if (e2 <= dx) { err += dx; y += sy; }
        }

        return false;
    }

    private (int X, int Y) ToClampedCell(double x, double y)
    {
        int ix = (int)Math.Round(x - MinX);
        int iy = (int)Math.Round(y - MinY);
        return (Math.Clamp(ix, 0, Width - 1), Math.Clamp(iy, 0, Height - 1));
    }
}
