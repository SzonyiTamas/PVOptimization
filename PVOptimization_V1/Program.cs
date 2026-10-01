using PVOptimization_V1.Energy;
using PVOptimization_V1.IO;
using PVOptimization_V1.Models;
using PVOptimization_V1.Optimization;
using PVOptimization_V1.Visualization;

// ---------- Input files ----------
string roofCsvPath = Path.Combine("Data", "roof_avg_satorteto_ablakkal_forbidden.csv");
string heatmapImagePath = Path.Combine("Data", "heatmap_satorteto_kemennyel_ablakkal.png");

// ---------- Optimization parameters ----------
var gaSettings = new GeneticAlgorithmSettings
{
    PopulationSize = 60,
    Generations = 35000,
    EliteRate = 0.13,
    MutationRate = 0.3,
    PatienceGenerations = 10000,
    MinImprovement = 1e-3,

    PanelsPerIndividual = 10,
    Alignment = AlignmentOption.Horizontal,
    EasyInstallWeight = 1.0,
};

var energySettings = new EnergySettings(
    PanelPeakPowerWp: 450,
    DailyIrradiationKwhPerM2: 6.46432,
    PerformanceRatio: 0.80);

// ---------- Run ----------
var grid = RoofGridCsvReader.Read(roofCsvPath);
string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
string outputImagePath = Path.Combine("Results", $"result_layout_{timestamp}.png");

var geneticAlgorithm = new GeneticAlgorithm(grid, gaSettings);
Console.WriteLine("The optimization is running...");
var bestResult = geneticAlgorithm.Run();

PanelRenderer.RenderPanelsOnImage(grid, bestResult, heatmapImagePath, outputImagePath, strokePx: 2f);
var estimate = EnergyEstimator.Estimate(bestResult, grid, energySettings);
ConsoleReporter.PrintResults(bestResult, estimate);
