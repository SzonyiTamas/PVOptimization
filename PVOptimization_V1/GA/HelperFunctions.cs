using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.GA
{
    internal class HelperFunctions
    {
        public static double NextGaussian(Random rng)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
        public static void ShuffleInPlace<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        private static List<int> WindowGroupCountsByRot(List<Panel> list, Func<Panel, double> coord, double tol)
        {
            var counts = new List<int>();

            foreach (var rotGroup in list.GroupBy(p => p.Rotated))
            {
                var sorted = rotGroup.OrderBy(coord).ToList();

                int i = 0;
                while (i < sorted.Count)
                {
                    double anchor = coord(sorted[i]);
                    int j = i + 1;

                    while (j < sorted.Count && Math.Abs(coord(sorted[j]) - anchor) <= tol)
                        j++;

                    counts.Add(j - i);
                    i = j;
                }
            }

            return counts;
        }
        public static double EasyInstall(double easyInstallWeight, AlignmentOption alignment, double fitness, List<Panel> panels, RoofGrid grid)
        {
            if(easyInstallWeight > 0.0 && panels.Count >= 2)
            {
                const double yTol = 2.0;
                const double xTol = 2.0;
                const double wHoriz = 0.2;
                const double wVert = 0.2;
                const int minSize = 3;

                double maxDist = Panel.PanelWidthPx * 4;
                double maxDist2 = maxDist * maxDist;

                static bool CloseEnough(Panel a, Panel b, double maxDist2)
                {
                    double dx = a.CenterX - b.CenterX;
                    double dy = a.CenterY - b.CenterY;
                    return (dx * dx + dy * dy) <= maxDist2;
                }

                var filteredPanels = panels
                    .Where(p => panels.Any(q =>
                        !ReferenceEquals(p, q) &&
                        CloseEnough(p, q, maxDist2) &&
                        !grid.HasForbiddenBetweenCenters(p, q)))
                    .ToList();

                var evalPanels = filteredPanels.Count >= 2 ? filteredPanels : panels;
                int m = evalPanels.Count;

                var yBuckets = WindowGroupCountsByRot(evalPanels, p => p.YMin, yTol);
                int effectiveAlignedY = yBuckets.Sum(cnt => Math.Max(0, cnt - (minSize - 1)));
                double rowScore = Math.Min(1.0, effectiveAlignedY / (double)m);

                var xBuckets = WindowGroupCountsByRot(evalPanels, p => p.XMin, xTol);
                int effectiveAlignedX = xBuckets.Sum(cnt => Math.Max(0, cnt - (minSize - 1)));
                double colScore = Math.Min(1.0, effectiveAlignedX / (double)m);

                double boost;

                switch (alignment)
                {
                    case AlignmentOption.Grid:
                        boost = (wHoriz * rowScore + wVert * colScore);
                        break;
                    case AlignmentOption.Horizontal:
                        boost = (wHoriz * rowScore);
                        break;
                    case AlignmentOption.Vertical:
                        boost = (wVert * colScore);
                        break;
                    default:
                        boost = (wHoriz * rowScore + wVert * colScore);
                        break;
                }

                fitness = fitness * (1.0 + easyInstallWeight * boost);
            }

            return fitness;

        }

    }
}
