using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

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

    public static void Mutate(Individual individual, RoofGrid grid, double mutationRate, Random random,
        double stepSigmaPx = DefaultMutationStepSigmaPx)
    {
        var panels = individual.Panels;

        for (int i = 0; i < panels.Count; i++)
        {
            if (random.NextDouble() > mutationRate)
                continue;

            var original = panels[i];

            for (int attempt = 0; attempt < panels.Count; attempt++)
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
