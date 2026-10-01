using PVOptimization_V1.Energy;
using PVOptimization_V1.Models;

namespace PVOptimization_V1.Visualization;

public static class ConsoleReporter
{
    public static void PrintResults(Individual bestResult, EnergyEstimate estimate)
    {
        Console.Clear();
        Console.WriteLine("\n============== Results of the optimal panel placement ==============\n");

        Console.WriteLine($"Panels:          {bestResult.Panels.Count}");
        Console.WriteLine($"Quality score:   {estimate.QualityFactor:F3}");
        Console.WriteLine($"System:          {estimate.SystemPeakPowerKwp:F2} kWp");
        Console.WriteLine($"Estimated daily: {estimate.DailyEnergyKwh:F1} kWh\n");

        Console.WriteLine("\n==================== Coordinates of the panels ====================\n");

        Console.WriteLine($"{"#",3} {"TL_X",8} {"TL_Y",8} {"BR_X",8} {"BR_Y",8}");

        int index = 1;
        foreach (var panel in bestResult.Panels.OrderBy(p => p.XMin))
        {
            Console.WriteLine($"{index,3} {panel.XMin,8:F2} {panel.YMin,8:F2} {panel.XMax,8:F2} {panel.YMax,8:F2}");
            index++;
        }

        Console.WriteLine("\n\n\n");
    }
}
