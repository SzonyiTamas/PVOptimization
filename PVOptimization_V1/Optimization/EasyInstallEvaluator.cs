using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

internal sealed class EasyInstallEvaluator
{
    private const double HorizontalWeight = 0.2;
    private const double VerticalWeight = 0.2;

    private const int MinGroupSize = 3;
    private const double ClosenessWeight = 1.0;

    private const double MaxNeighborDistancePx = Panel.PanelWidthPx * 4;
    private const double MaxNeighborDistanceSquared = MaxNeighborDistancePx * MaxNeighborDistancePx;

    private const double MaxGapPx = Panel.PanelWidthPx * 2;

    private static readonly AlignmentAxis Rows = new(
        LineCoordinate: p => p.YMin, Tolerance: 1.0, Start: p => p.XMin, End: p => p.XMax);

    private static readonly AlignmentAxis Columns = new(
        LineCoordinate: p => p.XMin, Tolerance: 1.0, Start: p => p.YMin, End: p => p.YMax);

    private readonly RoofGrid _grid;
    private readonly double _weight;
    private readonly AlignmentOption _alignment;
    private readonly bool _splitByForbiddenZones;

    public EasyInstallEvaluator(RoofGrid grid, double weight, AlignmentOption alignment, bool splitByForbiddenZones = true)
    {
        _grid = grid;
        _weight = weight;
        _alignment = alignment;
        _splitByForbiddenZones = splitByForbiddenZones;
    }

    public double ApplyBonus(double fitness, List<Panel> panels)
    {
        if (_weight == 0.0 || panels.Count < 2)
            return fitness;

        var (adjacency, hasNeighbor) = BuildNeighborGraph(panels);
        var (filteredPanels, filteredIndexMap, evaluatedPanels) = BuildEvaluationPanels(panels, hasNeighbor);
        var groups = BuildGroups(panels, filteredPanels, filteredIndexMap, adjacency, evaluatedPanels);

        double rowSum = 0.0;
        double columnSum = 0.0;

        foreach (var group in groups)
        {
            if (group.Count < MinGroupSize)
                continue;

            rowSum += ScoreAlignment(group, Rows);
            columnSum += ScoreAlignment(group, Columns);
        }

        double rowScore = Math.Min(1.0, rowSum / evaluatedPanels.Count);
        double columnScore = Math.Min(1.0, columnSum / evaluatedPanels.Count);

        return fitness * (1.0 + _weight * CalculateAlignmentBoost(rowScore, columnScore));
    }

    private double CalculateAlignmentBoost(double rowScore, double columnScore) => _alignment switch
    {
        AlignmentOption.Grid => HorizontalWeight * rowScore + VerticalWeight * columnScore,
        AlignmentOption.Horizontal => HorizontalWeight * rowScore,
        AlignmentOption.Vertical => VerticalWeight * columnScore,
        _ => HorizontalWeight * rowScore + VerticalWeight * columnScore,
    };

    private (List<int>[] Adjacency, bool[] HasNeighbor) BuildNeighborGraph(List<Panel> panels)
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
                bool canConnect = AreCloseEnough(panels[i], panels[j])
                    && (!_splitByForbiddenZones || !_grid.HasForbiddenBetweenCenters(panels[i], panels[j]));

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

    private static (List<Panel> FilteredPanels, List<int> FilteredIndexMap, List<Panel> EvaluatedPanels) BuildEvaluationPanels(
        List<Panel> panels, bool[] hasNeighbor)
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

        var evaluatedPanels = filteredPanels.Count >= 2 ? filteredPanels : panels;
        return (filteredPanels, filteredIndexMap, evaluatedPanels);
    }

    private List<List<Panel>> BuildGroups(
        List<Panel> panels,
        List<Panel> filteredPanels,
        List<int> filteredIndexMap,
        List<int>[] adjacency,
        List<Panel> evaluatedPanels)
    {
        if (!_splitByForbiddenZones)
            return new List<List<Panel>> { evaluatedPanels };

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

    private static bool AreCloseEnough(Panel a, Panel b)
    {
        double dx = a.CenterX - b.CenterX;
        double dy = a.CenterY - b.CenterY;
        return (dx * dx + dy * dy) <= MaxNeighborDistanceSquared;
    }

    private static List<List<Panel>> FindConnectedComponents(List<Panel> panels, List<int>[] adjacency)
    {
        var visited = new bool[panels.Count];
        var components = new List<List<Panel>>();

        for (int start = 0; start < panels.Count; start++)
        {
            if (visited[start])
                continue;

            var component = new List<Panel>();
            var stack = new Stack<int>();
            stack.Push(start);
            visited[start] = true;

            while (stack.Count > 0)
            {
                int current = stack.Pop();
                component.Add(panels[current]);

                foreach (int neighbor in adjacency[current])
                {
                    if (visited[neighbor])
                        continue;

                    visited[neighbor] = true;
                    stack.Push(neighbor);
                }
            }

            components.Add(component);
        }

        return components;
    }

    private static double ScoreAlignment(List<Panel> group, AlignmentAxis axis)
    {
        double total = 0.0;

        foreach (var line in SplitIntoLines(group, axis))
        {
            int baseScore = Math.Max(0, line.Count - (MinGroupSize - 1));
            if (baseScore == 0)
                continue;

            total += baseScore + ClosenessWeight * CalculateCloseness(line, axis);
        }

        return total;
    }

    private static List<List<Panel>> SplitIntoLines(List<Panel> panels, AlignmentAxis axis)
    {
        var lines = new List<List<Panel>>();

        foreach (var orientationGroup in panels.GroupBy(p => p.Rotated))
        {
            var sorted = orientationGroup.OrderBy(axis.LineCoordinate).ToList();

            int lineStart = 0;
            while (lineStart < sorted.Count)
            {
                int lineEnd = lineStart + 1;
                while (lineEnd < sorted.Count
                       && Math.Abs(axis.LineCoordinate(sorted[lineEnd]) - axis.LineCoordinate(sorted[lineEnd - 1])) <= axis.Tolerance)
                {
                    lineEnd++;
                }

                lines.Add(sorted.GetRange(lineStart, lineEnd - lineStart));
                lineStart = lineEnd;
            }
        }

        return lines;
    }

    private static double CalculateCloseness(List<Panel> line, AlignmentAxis axis)
    {
        if (line.Count < 2)
            return 0.0;

        var sorted = line.OrderBy(axis.Start).ToList();
        double sum = 0.0;
        int count = 0;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            double gap = Math.Max(0.0, axis.Start(sorted[i + 1]) - axis.End(sorted[i]));
            double closeness = 1.0 - Math.Min(1.0, gap / MaxGapPx);
            sum += closeness;
            count++;
        }

        return count > 0 ? sum / count : 0.0;
    }

    /// <param name="LineCoordinate">Coordinate shared by panels in the same line.</param>
    /// <param name="Tolerance">Maximum difference of <paramref name="LineCoordinate"/> between consecutive panels in a line.</param>
    /// <param name="Start">Start of a panel along the line.</param>
    /// <param name="End">End of a panel along the line.</param>
    private sealed record AlignmentAxis(
        Func<Panel, double> LineCoordinate,
        double Tolerance,
        Func<Panel, double> Start,
        Func<Panel, double> End);
}
