using PVOptimization_V1.Models;
using PVOptimization_V1.GA;
using PVOptimization_V1.Visualization;

internal class Program
{
    static void Main()
    {
        var grid = RoofGrid.FromCsv();
        int populationSize = 75;
        int panelsPerIndividual =35;
        int generations = 35000;
        double eliteRate = 0.13;
        double mutationRate = 0.3;
        int patienceGenerations = 4000;
        double minImprovement = 1e-3;
        bool easyInstall = false;
        string ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        var ga = new GeneticAlgorithm(grid,populationSize,panelsPerIndividual,generations,eliteRate,mutationRate,patienceGenerations,minImprovement,easyInstall);
        var bestResult = ga.Run();
        PanelRenderer.RenderPanelsOnImage(grid, bestResult, baseImagePath: Path.Combine("Data","heatmap_forbidden.png"), outputPath: Path.Combine("Results",$"result_layout_{ts}.png"), strokePx: 2f);
    }
}
    