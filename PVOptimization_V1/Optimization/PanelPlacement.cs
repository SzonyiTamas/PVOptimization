using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

internal static class PanelPlacement
{
    public static bool IsValid(Panel candidate, IReadOnlyList<Panel> placedPanels, RoofGrid grid, int ignoredIndex = -1)
    {
        for (int i = 0; i < placedPanels.Count; i++)
        {
            if (i != ignoredIndex && placedPanels[i].Overlaps(candidate))
                return false;
        }

        return !grid.OverlapsForbidden(candidate);
    }
}
