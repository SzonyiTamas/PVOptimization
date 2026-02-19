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

        public static double EasyInstall(bool easyInstall, int n, double fitness, List<Panel> panels, RoofGrid grid)
        {
            if(easyInstall && n >= 2)
            {
                const double yTol = 2.0;
                const double xTol = 2.0;
                const double wHoriz = 0.1;
                const double wVert = 0.1;
                const int minRowSize = 3;

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

                var yBuckets = evalPanels
                    .GroupBy(p => (Row: (int)Math.Round(p.YMin / yTol), Rot: p.Rotated))
                    .Select(g => g.Count())
                    .ToList();

                int effectiveAlignedY = yBuckets.Sum(cnt => Math.Max(0, cnt - (minRowSize - 1)));
                double rowScore = Math.Min(1.0, effectiveAlignedY / (double)m);

                var xBuckets = evalPanels
                    .GroupBy(p => (Col: (int)Math.Round(p.XMin / xTol), Rot: p.Rotated))
                    .Select(g => g.Count())
                    .ToList();

                int effectiveAlignedX = xBuckets.Sum(cnt => Math.Max(0, cnt - (minRowSize - 1)));
                double colScore = Math.Min(1.0, effectiveAlignedX / (double)m);

                fitness = fitness * (1.0 + wHoriz * rowScore + wVert * colScore);
            }
            
            return fitness;

        }

    }
}
