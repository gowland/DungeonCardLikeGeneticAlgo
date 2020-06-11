using System;
using System.Collections.Concurrent;
using GeneticSolver;
using System.Text;
using System.Threading.Tasks;
using DungeonCardsGeneticAlgo.Support;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo
{
    class Program
    {
        static void Main(string[] args)
        {
            var maxEliteSize = 1000;
            var cache = new FitnessCache<GameAgentMultipliers, double>(2000*maxEliteSize); // TODO: need to clear on repeated runs
            var evaluator = new GameAgentEvaluator(cache);

            var solverParameters = new SolverParameters(
                maxEliteSize,
                2*maxEliteSize,
                0.3);

//            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                LaunchEvolutionRun(solverParameters, evaluator);
            }

//            Task.WaitAll(tasks.ToArray());

//            TestBellCurve();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void LaunchEvolutionRun(SolverParameters solverParameters, GameAgentEvaluator evaluator)
        {
            var bellWeightedRandom = new CyclableBellWeightedRandom(10.0, 3.0, 1.0, 0.5, 0.1);
            var genomeDescriptions = new GameAgentMultipliersDescription(bellWeightedRandom);
            var defaultGenomeFactory = new GeneticSolver.Genome.DefaultGenomeFactory<GameAgentMultipliers>(genomeDescriptions);
            var mutationProbabilities = new Cyclable<double>(new[]{0.3, 0.9, 1.0});
            var mutator = new GenomeMutator<GameAgentMultipliers>(genomeDescriptions, mutationProbabilities, new UnWeightedRandom());
            var logger = new GameAgentSolverLogger();
            var solver = new Solver<GameAgentMultipliers, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<GameAgentMultipliers, double>[]
                {
                    // new GeneticSolver.EarlyStoppingConditions.FitnessThresholdReachedEarlyStopCondition<GameAgentMultipliers, double>(fitness => fitness < 1e-6),
                    new GeneticSolver.EarlyStoppingConditions.ProgressStalledEarlyStoppingCondition<GameAgentMultipliers, double>(10, 0.5, 0.8),
                    new GeneticSolver.EarlyStoppingConditions.FitnessNotImprovingEarlyStoppingCondition<GameAgentMultipliers>(1, 100),
                },
                new IGenomeReproductionStrategy<GameAgentMultipliers>[]
                {
                    new GeneticSolver.ReproductionStrategies.SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new StratifiedBreedingStrategy(),
                        defaultGenomeFactory, genomeDescriptions, evaluator, 20, 2),
                    // new GeneticSolver.ReproductionStrategies.SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new GeneticSolver.PairingStrategies.RandomBreedingStrategy(),
                        // defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
                });
            solver.NewGeneration += (s, e) => bellWeightedRandom.CycleStdDev();
            solver.NewGeneration += (s, e) => mutationProbabilities.Cycle();

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }
}
