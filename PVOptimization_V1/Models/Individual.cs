namespace PVOptimization_V1.Models;

public sealed class Individual
{
    public Individual(List<Panel> panels)
    {
        Panels = panels;
    }

    public List<Panel> Panels { get; }
    public double Fitness { get; set; }

    public Individual DeepCopy()
    {
        var panels = Panels.Select(panel => panel.Clone()).ToList();
        return new Individual(panels) { Fitness = Fitness };
    }
}
