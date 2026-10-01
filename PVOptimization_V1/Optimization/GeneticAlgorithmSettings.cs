using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

/// <summary>
/// Configuration of a <see cref="GeneticAlgorithm"/> run.
/// </summary>
public sealed record GeneticAlgorithmSettings
{
    /// <summary>Number of individuals in each generation.</summary>
    public required int PopulationSize { get; init; }

    /// <summary>Target number of panels to place on the roof.</summary>
    public required int PanelsPerIndividual { get; init; }

    /// <summary>Maximum number of generations.</summary>
    public required int Generations { get; init; }

    /// <summary>Share of the best individuals copied unchanged into the next generation.</summary>
    public required double EliteRate { get; init; }

    /// <summary>Initial per-panel mutation probability; it decreases linearly during the run.</summary>
    public required double MutationRate { get; init; }

    /// <summary>The run stops early after this many generations without significant improvement.</summary>
    public required int PatienceGenerations { get; init; }

    /// <summary>Smallest fitness increase that counts as a significant improvement.</summary>
    public required double MinImprovement { get; init; }

    /// <summary>Strength of the easy-install bonus; 0 disables it.</summary>
    public required double EasyInstallWeight { get; init; }

    /// <summary>Panel alignment rewarded by the easy-install bonus.</summary>
    public required AlignmentOption Alignment { get; init; }

    /// <summary>Optional seed for generating the initial population.</summary>
    public int? Seed { get; init; }
}
