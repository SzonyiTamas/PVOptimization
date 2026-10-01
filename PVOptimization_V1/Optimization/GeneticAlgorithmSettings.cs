using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

public sealed record GeneticAlgorithmSettings
{
    public required int PopulationSize { get; init; }

    public required int PanelsPerIndividual { get; init; }

    public required int Generations { get; init; }

    public required double EliteRate { get; init; }

    public required double MutationRate { get; init; }

    public required int PatienceGenerations { get; init; }

    public required double MinImprovement { get; init; }

    public required double EasyInstallWeight { get; init; }

    public required AlignmentOption Alignment { get; init; }

    public int? Seed { get; init; }
}
