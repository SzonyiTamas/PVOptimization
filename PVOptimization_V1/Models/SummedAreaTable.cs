using System.Numerics;

namespace PVOptimization_V1.Models;

/// <summary>
/// 2D prefix-sum table answering rectangular range-sum queries in O(1).
/// </summary>
internal sealed class SummedAreaTable<T> where T : INumber<T>
{
    // Padded with a leading zero row and column, so queries need no edge cases.
    private readonly T[,] _prefix;

    public SummedAreaTable(T[,] cells)
    {
        Width = cells.GetLength(0);
        Height = cells.GetLength(1);
        _prefix = new T[Width + 1, Height + 1];

        for (int y = 1; y <= Height; y++)
        {
            for (int x = 1; x <= Width; x++)
            {
                _prefix[x, y] = cells[x - 1, y - 1]
                    + (_prefix[x - 1, y] + _prefix[x, y - 1] - _prefix[x - 1, y - 1]);
            }
        }
    }

    public int Width { get; }
    public int Height { get; }

    public T Sum(CellRange range) =>
        _prefix[range.X1 + 1, range.Y1 + 1]
        - _prefix[range.X0, range.Y1 + 1]
        - _prefix[range.X1 + 1, range.Y0]
        + _prefix[range.X0, range.Y0];
}
