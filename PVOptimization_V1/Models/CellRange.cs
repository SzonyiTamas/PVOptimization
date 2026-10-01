namespace PVOptimization_V1.Models;

/// <summary>
/// An inclusive rectangular range of roof grid cells.
/// </summary>
public readonly record struct CellRange(int X0, int Y0, int X1, int Y1)
{
    public int CellCount => (X1 - X0 + 1) * (Y1 - Y0 + 1);
}
