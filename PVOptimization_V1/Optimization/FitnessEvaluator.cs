using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

internal sealed class FitnessEvaluator
{
    private readonly RoofGrid _grid;
    private readonly int _targetPanelCount;
    private readonly EasyInstallEvaluator _easyInstallEvaluator;

    public FitnessEvaluator(RoofGrid grid, int targetPanelCount, EasyInstallEvaluator easyInstallEvaluator)
    {
        _grid = grid;
        _targetPanelCount = targetPanelCount;
        _easyInstallEvaluator = easyInstallEvaluator;
    }

    public void EvaluatePopulation(List<Individual> population) =>
        Parallel.ForEach(population, EvaluateIndividual);

    public void EvaluateIndividual(Individual individual)
    {
        double sumOfPanelAverages = 0.0;
        foreach (var panel in individual.Panels)
        {
            var cells = _grid.ToCellRange(panel);
            sumOfPanelAverages += _grid.Sum(cells) / cells.CellCount;
        }

        double fitness = _targetPanelCount > 0 ? sumOfPanelAverages / _targetPanelCount : 0.0;
        individual.Fitness = _easyInstallEvaluator.ApplyBonus(fitness, individual.Panels);
    }
}
