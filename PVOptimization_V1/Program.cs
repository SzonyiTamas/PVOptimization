using PVOptimization_V1.Models;
using PVOptimization_V1.GA;
using PVOptimization_V1.Visualization;
using PVOptimization_V1.Viziulization;

internal class Program
{
    static void Main()
    {
        var grid = RoofGrid.FromCsv();
        int populationSize = 60;
        int generations = 35000;
        double eliteRate = 0.13;
        double mutationRate = 0.3;
        int patienceGenerations = 10000;
        double HPOA = 6.46432;
        double PR = 0.80;
        double minImprovement = 1e-3;
        string ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        //input parameters:

        int panelsPerIndividual = 18;
        AlignmentOption aligment = AlignmentOption.Grid;
        double easyInstallWeight = 1.0;
        double PANEL_WP = 450;
        
        var ga = new GeneticAlgorithm(grid,populationSize,panelsPerIndividual,generations,eliteRate,mutationRate,patienceGenerations,minImprovement,easyInstallWeight,aligment);
        Console.WriteLine("The optimization is running...");
        var bestResult = ga.Run();
        
        PanelRenderer.RenderPanelsOnImage(grid, bestResult, baseImagePath: Path.Combine("Data", "heatmap_satorteto_kemennyel_ablakkal.png"), outputPath: Path.Combine("Results",$"result_layout_{ts}.png"), strokePx: 2f);
        var (eday, f, avgPanel, avgRoof, systemKwp) = EnergyEstimator.EstimateDailyProductionKwh(bestResult, grid, PANEL_WP, HPOA, PR);
        ToConsole.ResultsToConsole(bestResult,eday,f,avgPanel,avgRoof,systemKwp);
        
    }
}
      