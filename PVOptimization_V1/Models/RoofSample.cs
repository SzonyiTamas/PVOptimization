namespace PVOptimization_V1.Models;

/// <summary>
/// A single measured roof point: its center coordinates and solar value.
/// A value of <see cref="ForbiddenValue"/> marks an area where no panel may be placed.
/// </summary>
public readonly record struct RoofSample(double CenterX, double CenterY, double Value)
{
    public const double ForbiddenValue = -1.0;

    public bool IsForbidden => Value == ForbiddenValue;
}
