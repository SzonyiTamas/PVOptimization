using PVOptimization_V1.Models;

namespace PVOptimization_V1.Energy;

public static class EnergyEstimator
{
    private const double MinLocalFactor = 0.0;
    private const double MaxLocalFactor = 2.0;

    public static EnergyEstimate Estimate(Individual layout, RoofGrid grid, EnergySettings settings)
    {
        double panelKwp = settings.PanelPeakPowerWp / 1000.0;
        double systemKwp = layout.Panels.Count * panelKwp;
        double averageRoofValue = CalculateAverageRoofValue(grid);

        double sumOfPanelAverages = 0.0;
        double dailyEnergyKwh = 0.0;
        int countedPanels = 0;

        foreach (var panel in layout.Panels)
        {
            var cells = grid.ToCellRange(panel);
            if (cells.CellCount <= 0)
                continue;

            double panelAverage = grid.Sum(cells) / cells.CellCount;
            sumOfPanelAverages += panelAverage;
            countedPanels++;

            double localFactor = panelAverage / averageRoofValue;
            if (localFactor < MinLocalFactor) localFactor = MinLocalFactor;
            if (localFactor > MaxLocalFactor) localFactor = MaxLocalFactor;

            dailyEnergyKwh += panelKwp * settings.DailyIrradiationKwhPerM2 * settings.PerformanceRatio * localFactor;
        }

        double averagePanelValue = sumOfPanelAverages / countedPanels;
        double qualityFactor = averageRoofValue > 1e-9 ? (averagePanelValue / averageRoofValue) : 0.0;
        if (qualityFactor < MinLocalFactor) qualityFactor = MinLocalFactor;
        if (qualityFactor > MaxLocalFactor) qualityFactor = MaxLocalFactor;

        return new EnergyEstimate(dailyEnergyKwh, qualityFactor, averagePanelValue, averageRoofValue, systemKwp);
    }

    private static double CalculateAverageRoofValue(RoofGrid grid)
    {
        var roof = grid.EntireRoof;
        int allowedCells = roof.CellCount - grid.ForbiddenCount(roof);
        return grid.Sum(roof) / allowedCells;
    }
}
