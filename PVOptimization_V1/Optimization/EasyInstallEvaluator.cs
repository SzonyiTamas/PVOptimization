using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

/// <summary>
/// Rewards layouts that are easy to install: panels lined up in rows and/or columns,
/// close to each other, inside groups that are not separated by forbidden areas.
/// </summary>
internal sealed class EasyInstallEvaluator
{
    private const double HorizontalWeight = 0.2;
    private const double VerticalWeight = 0.2;

    /// <summary>Smallest group / line of panels that earns a bonus.</summary>
    private const int MinGroupSize = 3;
    private const double ClosenessWeight = 1.0;

    /// <summary>Panels whose centers are farther apart than this are not neighbors.</summary>
    private const double MaxNeighborDistancePx = Panel.PanelWidthPx * 4;
    private const double MaxNeighborDistanceSquared = MaxNeighborDistancePx * MaxNeighborDistancePx;

    /// <summary>Gap between panels in a line at which the closeness score drops to zero.</summary>
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

        var adjacency = BuildNeighborGraph(panels);

        // Panels without any neighbor are ignored, unless that would leave fewer than two.
        var panelsWithNeighbor = panels.Where((_, i) => adjacency[i].Count > 0).ToList();
        var evaluatedPanels = panelsWithNeighbor.Count >= 2 ? panelsWithNeighbor : panels;

        // Isolated panels form single-panel groups, which are below MinGroupSize and thus score nothing.
        var groups = _splitByForbiddenZones
            ? FindConnectedComponents(panels, adjacency)
            : [evaluatedPanels];

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
        AlignmentOption.Horizontal => HorizontalWeight * rowScore,
        AlignmentOption.Vertical => VerticalWeight * columnScore,
        _ => HorizontalWeight * rowScore + VerticalWeight * columnScore,
    };

    /// <summary>
    /// Connects two panels when their centers are close enough and (optionally)
    /// no forbidden area lies between them.
    /// </summary>
    private List<int>[] BuildNeighborGraph(List<Panel> panels)
    {
        var adjacency = new List<int>[panels.Count];
        for (int i = 0; i < panels.Count; i++)
            adjacency[i] = [];

        for (int i = 0; i < panels.Count; i++)
        {
            for (int j = i + 1; j < panels.Count; j++)
            {
                bool connected = AreCloseEnough(panels[i], panels[j])
                    && (!_splitByForbiddenZones || !_grid.HasForbiddenBetweenCenters(panels[i], panels[j]));

                if (!connected)
                    continue;

                adjacency[i].Add(j);
                adjacency[j].Add(i);
            }
        }

        return adjacency;
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

    /// <summary>
    /// Splits the group into lines along the axis and scores each line long enough:
    /// one point per panel beyond <c>MinGroupSize - 1</c>, plus a closeness bonus.
    /// </summary>
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

    /// <summary>
    /// Groups panels of the same orientation whose line coordinates follow each other
    /// within the axis tolerance.
    /// </summary>
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

    /// <summary>
    /// Average closeness of consecutive panels in a line: 1 when touching, 0 at <see cref="MaxGapPx"/> or more.
    /// </summary>
    private static double CalculateCloseness(List<Panel> line, AlignmentAxis axis)
    {
        if (line.Count < 2)
            return 0.0;

        var sorted = line.OrderBy(axis.Start).ToList();
        double sum = 0.0;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            double gap = Math.Max(0.0, axis.Start(sorted[i + 1]) - axis.End(sorted[i]));
            sum += 1.0 - Math.Min(1.0, gap / MaxGapPx);
        }

        return sum / (sorted.Count - 1);
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
