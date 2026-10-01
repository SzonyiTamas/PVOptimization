using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

internal static class PanelPlacement
{
    /// <summary>
    /// A placement is valid if the candidate overlaps neither another panel nor a forbidden area.
    /// </summary>
    /// <param name="ignoredIndex">Index in <paramref name="placedPanels"/> to skip (e.g. the panel being replaced).</param>
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
