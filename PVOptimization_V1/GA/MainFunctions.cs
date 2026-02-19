using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.GA
{
    internal class MainFunctions
    {
        public static List<Individual> CreateInitialPopulation(RoofGrid grid, int populationSize, int panelsPerIndividual, Random rng)
        {
            double minX = grid.MinX, maxX = grid.MaxX, minY = grid.MinY, maxY = grid.MaxY;

            var pop = new List<Individual>(populationSize);

            for (int i = 0; i < populationSize; i++)
            {
                var panels = new List<Panel>(panelsPerIndividual);

                for (int k = 0; k < panelsPerIndividual; k++)
                {
                    bool placed = false;
                    for (int attempt = 0; attempt < 2000; attempt++)
                    {

                        bool rotated = rng.Next(2) == 0;
                        double W = rotated ? Panel.PanelHeightPx : Panel.PanelWidthPx;
                        double H = rotated ? Panel.PanelWidthPx : Panel.PanelHeightPx;

                        double yMinAllowed = minY;
                        double yMaxAllowed = maxY - H;

                        if (yMaxAllowed < yMinAllowed) continue;

                        double x = minX + rng.NextDouble() * (maxX - minX - W);
                        double y = yMinAllowed + rng.NextDouble() * (yMaxAllowed - yMinAllowed);

                        var cand = new Panel(x, y, rotated);
                        cand.ClampToBounds(minX, minY, maxX, maxY);

                        bool overlaps = false;
                        foreach (var p in panels)
                        {
                            if (p.Overlaps(cand))
                            {
                                overlaps = true;
                                break;
                            }
                        }
                        if (overlaps) continue;

                        
                        if (grid.RectOverlapsForbidden(cand.XMin, cand.YMin, cand.XMax, cand.YMax))
                            continue;

                        panels.Add(cand);
                        placed = true;
                        break;

                    }

                    if (!placed)
                    {
                        break;
                    }
                }

                pop.Add(new Individual(panels));
            }

            return pop;
        }
        public static void EvaluatePopulation(List<Individual> pop, RoofGrid grid, int panelsPerIndividual, bool easyInstall)
        {
            Parallel.ForEach(pop, ind => EvaluateIndividual(ind, grid,panelsPerIndividual,easyInstall));
        }
        public static void EvaluateIndividual(Individual ind, RoofGrid grid, int panelsPerIndividual,bool easyInstall)
        {
            var panels = ind.Panels;
            int n = panels.Count;

            double sumAvg = 0.0;

            for (int i = 0; i < n; i++)
            {
                var p = panels[i];

                grid.ToIndexBounds(p.XMin, p.YMin, p.XMax, p.YMax,
                                   out int ix0, out int iy0, out int ix1, out int iy1);

                int count = (ix1 - ix0 + 1) * (iy1 - iy0 + 1);
                double sum = grid.RectSum(ix0, iy0, ix1, iy1);
                double avg = sum / count;

                sumAvg += avg;
            }

            int expected = panelsPerIndividual;
            double fitness = expected > 0 ? (sumAvg / expected) : 0.0;

            if (easyInstall && n >= 2)
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

            ind.Fitness = fitness;

        }
        public static Individual TournamentSelect(List<Individual> pop, int k, Random rng)
        {
            var best = pop[rng.Next(pop.Count)];
            for (int i = 1; i < k; i++)
            {
                var challenger = pop[rng.Next(pop.Count)];
                if (challenger.Fitness > best.Fitness)
                    best = challenger;
            }
            return best;
        }
        public static Individual Crossover(Individual p1, Individual p2, RoofGrid grid, int panelsPerIndividual, Random rng)
        {
            var childPanels = new List<Panel>(panelsPerIndividual);

            var candidates = new List<Panel>(p1.Panels.Count + p2.Panels.Count);
            candidates.AddRange(p1.Panels);
            candidates.AddRange(p2.Panels); 
            HelperFunctions.ShuffleInPlace(candidates,rng);

            foreach (var src in candidates)
            {
                if (childPanels.Count >= panelsPerIndividual) break;

                var p = new Panel(src.XMin, src.YMin,src.Rotated);
                p.ClampToBounds(grid.MinX, grid.MinY, grid.MaxX, grid.MaxY);

                bool overlap = false;
                foreach (var q in childPanels)
                {
                    if (q.Overlaps(p)) { overlap = true; break; }
                }
                if (overlap) continue;
                
                if (grid.RectOverlapsForbidden(p.XMin, p.YMin, p.XMax, p.YMax))
                    continue;

                childPanels.Add(p);

            }
            return new Individual(childPanels);
        }
        public static void Mutate(Individual ind, Random rng, RoofGrid grid, double mutationRate, double stepSigmaPx=0.3 * Panel.PanelWidthPx)
        {
            var panels = ind.Panels;

            for (int i = 0; i < panels.Count; i++)
            {
                if (rng.NextDouble() > mutationRate) continue;

                var orig = panels[i];

                for (int t = 0; t < panels.Count; t++)
                {
                    double dx = HelperFunctions.NextGaussian(rng) * stepSigmaPx;
                    double dy = HelperFunctions.NextGaussian(rng) * stepSigmaPx;

                    var cand = new Panel(orig.X, orig.Y, orig.Rotated);
                    cand.MoveBy(dx, dy);
                    if (rng.NextDouble() < 0.25)
                        cand.ToggleOrientation();
                    cand.ClampToBounds(grid.MinX, grid.MinY, grid.MaxX, grid.MaxY);

                    bool ok = true;
                    for (int j = 0; j < panels.Count; j++)
                    {
                        if (j == i) continue;
                        if (panels[j].Overlaps(cand)) { ok = false; break; }
                    }
                    if (!ok) continue;

                    if (grid.RectOverlapsForbidden(cand.XMin, cand.YMin, cand.XMax, cand.YMax))
                        continue;

                    panels[i] = cand;
                    break;
                }
            }
        }
    }
}
