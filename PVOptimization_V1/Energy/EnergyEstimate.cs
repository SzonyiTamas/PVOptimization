namespace PVOptimization_V1.Energy;

/// <param name="DailyEnergyKwh">Estimated daily production [kWh].</param>
/// <param name="QualityFactor">Average panel value relative to the roof average, clamped to 0–2.</param>
/// <param name="AveragePanelValue">Mean solar value under the panels.</param>
/// <param name="AverageRoofValue">Mean solar value of all non-forbidden roof cells.</param>
/// <param name="SystemPeakPowerKwp">Total peak power of the panels [kWp].</param>
public sealed record EnergyEstimate(
    double DailyEnergyKwh,
    double QualityFactor,
    double AveragePanelValue,
    double AverageRoofValue,
    double SystemPeakPowerKwp);
