using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

/// <summary>
/// Creates the random starting population of the genetic algorithm.
/// </summary>
internal static class PopulationInitializer
{
    private const int MaxPlacementAttempts = 2000;

    public static List<Individual> CreateRandom(RoofGrid grid, int populationSize, int panelsPerIndividual, Random random)
    {
        var population = new List<Individual>(populationSize);

        for (int i = 0; i < populationSize; i++)
            population.Add(CreateRandomIndividual(grid, panelsPerIndividual, random));

        return population;
    }

    /// <summary>
    /// Places panels one by one at random valid positions. If a panel cannot be placed
    /// within <see cref="MaxPlacementAttempts"/> attempts, the individual keeps fewer panels.
    /// </summary>
    private static Individual CreateRandomIndividual(RoofGrid grid, int panelCount, Random random)
    {
        var panels = new List<Panel>(panelCount);

        for (int k = 0; k < panelCount; k++)
        {
            var panel = TryPlaceRandomPanel(grid, panels, random);
            if (panel is null)
                break;

            panels.Add(panel);
        }

        return new Individual(panels);
    }

    private static Panel? TryPlaceRandomPanel(RoofGrid grid, List<Panel> placedPanels, Random random)
    {
        for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            bool rotated = random.Next(2) == 0;
            double width = rotated ? Panel.PanelHeightPx : Panel.PanelWidthPx;
            double height = rotated ? Panel.PanelWidthPx : Panel.PanelHeightPx;

            double yMinAllowed = grid.MinY;
            double yMaxAllowed = grid.MaxY - height;
            if (yMaxAllowed < yMinAllowed)
                continue;

            double x = grid.MinX + random.NextDouble() * (grid.MaxX - grid.MinX - width);
            double y = yMinAllowed + random.NextDouble() * (yMaxAllowed - yMinAllowed);

            var candidate = new Panel(x, y, rotated);
            grid.ClampInside(candidate);

            if (PanelPlacement.IsValid(candidate, placedPanels, grid))
                return candidate;
        }

        return null;
    }
}
