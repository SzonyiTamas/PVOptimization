using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVOptimization_V1.GA
{
    internal class EasyInstallEvaluator
    {

        public static double EasyInstall(double easyInstallWeight, AlignmentOption alignment, double fitness, List<Panel> panels, RoofGrid grid, bool splitByForbiddenZones = true)
        {
            if (easyInstallWeight == 0.0 || panels.Count < 2)
                return fitness;

            const double yTol = 1.0;
            const double xTol = 1.0;
            const double wHoriz = 0.2;
            const double wVert = 0.2;
            const int minSize = 3;
            const double wBucketCloseness = 1.0;

            double maxDist = Panel.PanelWidthPx * 4;
            double maxDist2 = maxDist * maxDist;
            double maxGap = Panel.PanelWidthPx * 2;

            var (adjacency, hasNeighbor) = BuildPanelGraph(panels, grid, maxDist2, splitByForbiddenZones);
            var (filteredPanels, filteredIndexMap, evalPanels) = BuildEvaluationPanels(panels, hasNeighbor);
            var components = BuildComponents(panels, filteredPanels, filteredIndexMap, adjacency, evalPanels, splitByForbiddenZones);

            double rowAlignmentSum = 0.0;
            double colAlignmentSum = 0.0;

            foreach (var component in components)
            {
                if (component.Count < minSize)
                    continue;

                rowAlignmentSum += ScoreBuckets(
                    BuildBucketsByRot(component, p => p.YMin, yTol),
                    minSize,
                    wBucketCloseness,
                    bucket => HorizontalBucketCloseness(bucket, maxGap));

                colAlignmentSum += ScoreBuckets(
                    BuildBucketsByRot(component, p => p.XMin, xTol),
                    minSize,
                    wBucketCloseness,
                    bucket => VerticalBucketCloseness(bucket, maxGap));
            }

            double rowScore = Math.Min(1.0, rowAlignmentSum / evalPanels.Count);
            double colScore = Math.Min(1.0, colAlignmentSum / evalPanels.Count);
            double boost = CalculateAlignmentBoost(alignment, rowScore, colScore, wHoriz, wVert);

            return fitness * (1.0 + easyInstallWeight * boost);
        }
        private static List<List<Panel>> FindConnectedComponents(List<Panel> panels, List<int>[] adj)
        {
            int n = panels.Count;
            var visited = new bool[n];
            var components = new List<List<Panel>>();

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;

                var comp = new List<Panel>();
                var stack = new Stack<int>();
                stack.Push(i);
                visited[i] = true;

                while (stack.Count > 0)
                {
                    int u = stack.Pop();
                    comp.Add(panels[u]);

                    foreach (int v in adj[u])
                    {
                        if (visited[v]) continue;
                        visited[v] = true;
                        stack.Push(v);
                    }
                }

                components.Add(comp);
            }

            return components;
        }
        private static List<List<Panel>> BuildBucketsByRot(List<Panel> list, Func<Panel, double> coord, double tol)
        {
            var buckets = new List<List<Panel>>();

            foreach (var rotGroup in list.GroupBy(p => p.Rotated))
            {
                var sorted = rotGroup.OrderBy(coord).ToList();

                int i = 0;
                while (i < sorted.Count)
                {
                    int j = i + 1;

                    while (j < sorted.Count && Math.Abs(coord(sorted[j]) - coord(sorted[j - 1])) <= tol)
                        j++;

                    buckets.Add(sorted.GetRange(i, j - i));
                    i = j;
                }
            }

            return buckets;
        }
        private static double HorizontalBucketCloseness(List<Panel> bucket, double maxGap)
        {
            if (bucket.Count < 2) return 0.0;

            var sorted = bucket.OrderBy(p => p.XMin).ToList();
            double sum = 0.0;
            int cnt = 0;

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                double gap = Math.Max(0.0, sorted[i + 1].XMin - sorted[i].XMax);
                double s = 1.0 - Math.Min(1.0, gap / maxGap);
                sum += s;
                cnt++;
            }

            return cnt > 0 ? sum / cnt : 0.0;
        }
        private static double VerticalBucketCloseness(List<Panel> bucket, double maxGap)
        {
            if (bucket.Count < 2) return 0.0;

            var sorted = bucket.OrderBy(p => p.YMin).ToList();
            double sum = 0.0;
            int cnt = 0;

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                double gap = Math.Max(0.0, sorted[i + 1].YMin - sorted[i].YMax);
                double s = 1.0 - Math.Min(1.0, gap / maxGap);
                sum += s;
                cnt++;
            }

            return cnt > 0 ? sum / cnt : 0.0;
        }
        private static (List<int>[] Adjacency, bool[] HasNeighbor) BuildPanelGraph(List<Panel> panels, RoofGrid grid, double maxDist2, bool splitByForbiddenZones)
        {
            int panelCount = panels.Count;
            var adjacency = new List<int>[panelCount];
            var hasNeighbor = new bool[panelCount];

            for (int i = 0; i < panelCount; i++)
                adjacency[i] = new List<int>();

            for (int i = 0; i < panelCount; i++)
            {
                for (int j = i + 1; j < panelCount; j++)
                {
                    bool canConnect =
                        ArePanelsCloseEnough(panels[i], panels[j], maxDist2) &&
                        (!splitByForbiddenZones || !grid.HasForbiddenBetweenCenters(panels[i], panels[j]));

                    if (!canConnect)
                        continue;

                    adjacency[i].Add(j);
                    adjacency[j].Add(i);
                    hasNeighbor[i] = true;
                    hasNeighbor[j] = true;
                }
            }

            return (adjacency, hasNeighbor);
        }
        private static (List<Panel> FilteredPanels, List<int> FilteredIndexMap, List<Panel> EvalPanels) BuildEvaluationPanels(List<Panel> panels, bool[] hasNeighbor)
        {
            var filteredPanels = new List<Panel>();
            var filteredIndexMap = new List<int>();

            for (int i = 0; i < panels.Count; i++)
            {
                if (!hasNeighbor[i])
                    continue;

                filteredPanels.Add(panels[i]);
                filteredIndexMap.Add(i);
            }

            var evalPanels = filteredPanels.Count >= 2 ? filteredPanels : panels;
            return (filteredPanels, filteredIndexMap, evalPanels);
        }
        private static List<List<Panel>> BuildComponents(List<Panel> panels, List<Panel> filteredPanels, List<int> filteredIndexMap, List<int>[] adjacency, List<Panel> evalPanels, bool splitByForbiddenZones)
        {
            if (!splitByForbiddenZones)
                return new List<List<Panel>> { evalPanels };

            if (filteredPanels.Count < 2)
                return FindConnectedComponents(panels, adjacency);

            int filteredCount = filteredPanels.Count;
            var filteredAdjacency = new List<int>[filteredCount];
            var oldToNew = new Dictionary<int, int>(filteredCount);

            for (int i = 0; i < filteredCount; i++)
            {
                filteredAdjacency[i] = new List<int>();
                oldToNew[filteredIndexMap[i]] = i;
            }

            for (int i = 0; i < filteredCount; i++)
            {
                int oldIndex = filteredIndexMap[i];

                foreach (int oldNeighbor in adjacency[oldIndex])
                {
                    if (oldToNew.TryGetValue(oldNeighbor, out int newNeighbor))
                        filteredAdjacency[i].Add(newNeighbor);
                }
            }

            return FindConnectedComponents(filteredPanels, filteredAdjacency);
        }
        private static double ScoreBuckets(List<List<Panel>> buckets, int minSize, double closenessWeight, Func<List<Panel>, double> closenessFunc)
        {
            double total = 0.0;

            foreach (var bucket in buckets)
            {
                int baseScore = Math.Max(0, bucket.Count - (minSize - 1));
                if (baseScore == 0)
                    continue;

                double closenessBonus = closenessFunc(bucket);
                total += baseScore + closenessWeight * closenessBonus;
            }

            return total;
        }
        private static double CalculateAlignmentBoost(AlignmentOption alignment, double rowScore, double colScore, double horizontalWeight, double verticalWeight)
        {
            return alignment switch
            {
                AlignmentOption.Grid => horizontalWeight * rowScore + verticalWeight * colScore,
                AlignmentOption.Horizontal => horizontalWeight * rowScore,
                AlignmentOption.Vertical => verticalWeight * colScore,
                _ => horizontalWeight * rowScore + verticalWeight * colScore
            };
        }
        private static bool ArePanelsCloseEnough(Panel a, Panel b, double maxDist2)
        {
            double dx = a.CenterX - b.CenterX;
            double dy = a.CenterY - b.CenterY;
            return (dx * dx + dy * dy) <= maxDist2;
        }
    }
}
