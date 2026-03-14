using PVOptimization_V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PVOptimization_V1.GA
{
    public sealed class GeneticAlgorithm
    {
        private readonly RoofGrid grid;
        private readonly Random rng;
        
        private static readonly ThreadLocal<Random> ThreadSafeRng = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public int PopulationSize { get; }
        public int PanelsPerIndividual { get; }
        public int Generations { get; }
        public double EliteRate { get; }
        public double MutationRate { get; }
        public int PatienceGenerations { get; }
        public double MinImprovement { get; }
        public double EasyInstallWeight { get; }
        public AlignmentOption Alignment { get; }


        public GeneticAlgorithm(RoofGrid grid, int populationSize, int panelsPerIndividual, int generations, double eliteRate, double mutationRate, int patienceGenerations, double minImprovement, double easyInstallWeight, AlignmentOption alignment, int? seed = null)
        {
            this.grid = grid;
            this.rng = seed.HasValue ? new Random(seed.Value) : new Random();
            PopulationSize = populationSize;
            PanelsPerIndividual = panelsPerIndividual;
            Generations = generations;
            EliteRate = eliteRate;
            MutationRate = mutationRate;
            PatienceGenerations = patienceGenerations;
            MinImprovement = minImprovement;
            EasyInstallWeight = easyInstallWeight;
            Alignment = alignment;
        }

        public Individual Run()
        {
            var pop = MainFunctions.CreateInitialPopulation(grid, PopulationSize, PanelsPerIndividual, rng);
            MainFunctions.EvaluatePopulation(pop, grid, PanelsPerIndividual,EasyInstallWeight, Alignment);

            var bestEver = pop.OrderByDescending(ind => ind.Fitness).First().DeepCopy();

            int gensSinceImprov = 0;
            double lastBest = bestEver.Fitness;

            for (int i = 0; i < Generations; i++)
            {
                var next = new List<Individual>();
                int eliteCount = Math.Max(1, (int)Math.Round(PopulationSize * EliteRate));

                next.AddRange(pop.OrderByDescending(ind => ind.Fitness).Take(eliteCount).Select(ind => ind.DeepCopy()));

                var nextChildren = Enumerable.Range(0, PopulationSize - eliteCount).AsParallel().Select(_ =>{
                var localRng = ThreadSafeRng.Value;
                var p1 = MainFunctions.TournamentSelect(pop, 4, localRng);
                var p2 = MainFunctions.TournamentSelect(pop, 4, localRng);
                if (ReferenceEquals(p1, p2))
                    p2 = MainFunctions.TournamentSelect(pop, 4, localRng);

                double currentMutationRate = MutationRate - (i / (double)(Generations - 1)) * (MutationRate - 0.05);

                var child = MainFunctions.Crossover(p1, p2, grid, PanelsPerIndividual, localRng);
                MainFunctions.Mutate(child, localRng, grid, currentMutationRate);
                return child;}).ToList();

                next.AddRange(nextChildren);
                pop = next;
                MainFunctions.EvaluatePopulation(pop, grid, PanelsPerIndividual,EasyInstallWeight,Alignment);

                var bestNow = pop.OrderByDescending(ind => ind.Fitness).First();
                if (bestNow.Fitness > bestEver.Fitness)
                    bestEver = bestNow.DeepCopy();

                if (bestEver.Fitness > lastBest + MinImprovement)
                {
                    lastBest = bestEver.Fitness;
                    gensSinceImprov = 0;
                }
                else
                {
                    gensSinceImprov++;
                }

                //if ((i + 1) % 100 == 0)
                //    Console.WriteLine($"Gen {i + 1}/{Generations} -> best={bestEver.Fitness:F10}");

                if (gensSinceImprov >= PatienceGenerations)
                    break;

            }
            
            return bestEver;
        }
    }
}
