using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

/// <summary>
/// Selection, crossover and mutation operators of the genetic algorithm.
/// </summary>
internal static class GeneticOperators
{
    private const double OrientationFlipProbability = 0.25;
    private const double DefaultMutationStepSigmaPx = 0.3 * Panel.PanelWidthPx;

    public static Individual TournamentSelect(List<Individual> population, int tournamentSize, Random random)
    {
        var best = population[random.Next(population.Count)];
        for (int i = 1; i < tournamentSize; i++)
        {
            var challenger = population[random.Next(population.Count)];
            if (challenger.Fitness > best.Fitness)
                best = challenger;
        }

        return best;
    }

    /// <summary>
    /// Builds a child from the shuffled panels of both parents, keeping every panel
    /// that fits without overlap until the target panel count is reached.
    /// </summary>
    public static Individual Crossover(Individual parent1, Individual parent2, RoofGrid grid, int panelsPerIndividual, Random random)
    {
        var candidates = new List<Panel>(parent1.Panels.Count + parent2.Panels.Count);
        candidates.AddRange(parent1.Panels);
        candidates.AddRange(parent2.Panels);
        random.ShuffleInPlace(candidates);

        var childPanels = new List<Panel>(panelsPerIndividual);
        foreach (var source in candidates)
        {
            if (childPanels.Count >= panelsPerIndividual)
                break;

            var panel = source.Clone();
            grid.ClampInside(panel);

            if (PanelPlacement.IsValid(panel, childPanels, grid))
                childPanels.Add(panel);
        }

        return new Individual(childPanels);
    }

    /// <summary>
    /// Moves each panel with probability <paramref name="mutationRate"/> by a Gaussian step
    /// (occasionally also rotating it). Invalid moves are retried; the panel stays in place
    /// if no valid move is found.
    /// </summary>
    public static void Mutate(Individual individual, RoofGrid grid, double mutationRate, Random random,
        double stepSigmaPx = DefaultMutationStepSigmaPx)
    {
        var panels = individual.Panels;

        for (int i = 0; i < panels.Count; i++)
        {
            if (random.NextDouble() > mutationRate)
                continue;

            var original = panels[i];
            int maxAttempts = panels.Count;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                double dx = random.NextGaussian() * stepSigmaPx;
                double dy = random.NextGaussian() * stepSigmaPx;

                var candidate = original.Clone();
                candidate.MoveBy(dx, dy);
                if (random.NextDouble() < OrientationFlipProbability)
                    candidate.ToggleOrientation();
                grid.ClampInside(candidate);

                if (PanelPlacement.IsValid(candidate, panels, grid, ignoredIndex: i))
                {
                    panels[i] = candidate;
                    break;
                }
            }
        }
    }
}
