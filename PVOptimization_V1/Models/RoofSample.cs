namespace PVOptimization_V1.Models;

public readonly record struct RoofSample(double CenterX, double CenterY, double Value)
{
    public const double ForbiddenValue = -1.0;

    public bool IsForbidden => Value == ForbiddenValue;
}
