using PVOptimization_V1.Models;

namespace PVOptimization_V1.Optimization;

public sealed class GeneticAlgorithm
{
    private const int TournamentSize = 4;
    private const double FinalMutationRate = 0.05;

    private static readonly ThreadLocal<Random> ThreadRandom =
        new(() => new Random(Guid.NewGuid().GetHashCode()));

    private readonly RoofGrid _grid;
    private readonly GeneticAlgorithmSettings _settings;
    private readonly FitnessEvaluator _fitnessEvaluator;
    private readonly Random _random;

    public GeneticAlgorithm(RoofGrid grid, GeneticAlgorithmSettings settings)
    {
        _grid = grid;
        _settings = settings;
        _random = settings.Seed is int seed ? new Random(seed) : new Random();

        var easyInstallEvaluator = new EasyInstallEvaluator(grid, settings.EasyInstallWeight, settings.Alignment);
        _fitnessEvaluator = new FitnessEvaluator(grid, settings.PanelsPerIndividual, easyInstallEvaluator);
    }

    public Individual Run()
    {
        var population = PopulationInitializer.CreateRandom(
            _grid, _settings.PopulationSize, _settings.PanelsPerIndividual, _random);
        _fitnessEvaluator.EvaluatePopulation(population);

        var bestEver = SelectBest(population).DeepCopy();
        double lastSignificantBest = bestEver.Fitness;
        int generationsWithoutImprovement = 0;

        for (int generation = 0; generation < _settings.Generations; generation++)
        {
            population = CreateNextGeneration(population, generation);
            _fitnessEvaluator.EvaluatePopulation(population);

            var bestNow = SelectBest(population);
            if (bestNow.Fitness > bestEver.Fitness)
                bestEver = bestNow.DeepCopy();

            if (bestEver.Fitness > lastSignificantBest + _settings.MinImprovement)
            {
                lastSignificantBest = bestEver.Fitness;
                generationsWithoutImprovement = 0;
            }
            else
            {
                generationsWithoutImprovement++;
            }

            if (generationsWithoutImprovement >= _settings.PatienceGenerations)
                break;
        }

        return bestEver;
    }

    private List<Individual> CreateNextGeneration(List<Individual> population, int generation)
    {
        var nextGeneration = new List<Individual>();
        int eliteCount = Math.Max(1, (int)Math.Round(_settings.PopulationSize * _settings.EliteRate));

        nextGeneration.AddRange(population
            .OrderByDescending(individual => individual.Fitness)
            .Take(eliteCount)
            .Select(individual => individual.DeepCopy()));

        var children = Enumerable.Range(0, _settings.PopulationSize - eliteCount)
            .AsParallel()
            .Select(_ => CreateChild(population, generation))
            .ToList();

        nextGeneration.AddRange(children);
        return nextGeneration;
    }

    private Individual CreateChild(List<Individual> population, int generation)
    {
        var random = ThreadRandom.Value!;

        var parent1 = GeneticOperators.TournamentSelect(population, TournamentSize, random);
        var parent2 = GeneticOperators.TournamentSelect(population, TournamentSize, random);
        if (ReferenceEquals(parent1, parent2))
            parent2 = GeneticOperators.TournamentSelect(population, TournamentSize, random);

        double currentMutationRate = GetMutationRate(generation);

        var child = GeneticOperators.Crossover(parent1, parent2, _grid, _settings.PanelsPerIndividual, random);
        GeneticOperators.Mutate(child, _grid, currentMutationRate, random);
        return child;
    }

    private double GetMutationRate(int generation) =>
        _settings.MutationRate - (generation / (double)(_settings.Generations - 1)) * (_settings.MutationRate - FinalMutationRate);

    private static Individual SelectBest(IEnumerable<Individual> population) =>
        population.OrderByDescending(individual => individual.Fitness).First();
}
