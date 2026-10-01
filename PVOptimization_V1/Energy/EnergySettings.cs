namespace PVOptimization_V1.Energy;

/// <param name="PanelPeakPowerWp">Peak power of one panel [Wp].</param>
/// <param name="DailyIrradiationKwhPerM2">Daily plane-of-array irradiation (H_POA) [kWh/m²].</param>
/// <param name="PerformanceRatio">System performance ratio (PR), 0–1.</param>
public sealed record EnergySettings(
    double PanelPeakPowerWp,
    double DailyIrradiationKwhPerM2,
    double PerformanceRatio);
